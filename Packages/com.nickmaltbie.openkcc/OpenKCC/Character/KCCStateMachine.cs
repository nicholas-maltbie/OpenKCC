// Copyright (C) 2022 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using nickmaltbie.OpenKCC.FSM;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Have a character controller push any dynamic rigidbody it hits
    /// </summary>
    public class KCCStateMachine : FixedStateMachineBehaviour, IKCCConfig
    {

        [Header("Input Controls")]

        /// <summary>
        /// Action reference for moving the player.
        /// </summary>
        [Tooltip("Action reference for moving the player")]
        [SerializeField]
        private InputActionReference moveAction;

        /// <summary>
        /// Action reference for jumping.
        /// </summary>
        [Tooltip("Action reference for moving the player")]
        [SerializeField]
        private InputActionReference jumpAction;

        /// <summary>
        /// Action reference for sprinting.
        /// </summary>
        [Tooltip("Action reference for moving the player")]
        [SerializeField]
        private InputActionReference sprintAction;

        [Header("Ground Checking")]

        /// <summary>
        /// Distance to ground at which player is considered grounded.
        /// </summary>
        [Tooltip("Distance from ground at which a player is considered standing on the ground")]
        [SerializeField]
        private float groundedDistance = 0.01f;

        /// <summary>
        /// Distance to ground at which player is considered standing on something.
        /// </summary>
        [Tooltip("Distance to ground at which player is considered standing on something")]
        [SerializeField]
        private float standingDistance = 0.1f;

        /// <summary>
        /// Distance to check player distance to ground.
        /// </summary>
        [Tooltip("Distance to draw rays down when checking if player is grounded")]
        [SerializeField]
        private float groundCheckDistance = 5f;

        /// <summary>
        /// Maximum angle at which the player can walk (in degrees).
        /// </summary>
        [Tooltip("Maximum angle at which the player can walk")]
        [SerializeField]
        [Range(0, 90)]
        private float maxWalkAngle = 60f;

        /// <summary>
        /// Direction and strength of gravity
        /// </summary>
        [Tooltip("Direction and strength of gravity in units per second squared")]
        [SerializeField]
        private Vector3 gravity = new Vector3(0, -9.807f, 0);

        [Header("Motion Settings")]

        /// <summary>
        /// Speed of player movement when walking.
        /// </summary>
        [Tooltip("Speed of player when walking")]
        [SerializeField]
        private float walkingSpeed = 7.5f;

        /// <summary>
        /// Speed of player when sprinting.
        /// </summary>
        [Tooltip("Speed of player when sprinting")]
        [SerializeField]
        private float sprintSpeed = 10.0f;

        /// <summary>
        /// Maximum number of time player can bounce of walls/floors/objects during an update.
        /// </summary>
        [Tooltip("Maximum number of bounces when a player is moving")]
        [SerializeField]
        [Range(1, 10)]
        private int maxBounces = 5;

        /// <summary>
        /// Decay value of momentum when hitting another object.
        /// Should be between [0, 1].
        /// </summary>
        [Tooltip("Decay in momentum when hitting another object")]
        [SerializeField]
        [Range(0, 1)]
        private float pushDecay = 0.9f;

        /// <summary>
        /// Decrease in momentum factor due to angle change when walking.
        /// Should be a positive float value. It's an exponential applied to 
        /// values between [0, 1] so values smaller than 1 create a positive
        /// curve and grater than 1 for a negative curve.
        /// </summary>
        [Tooltip("Decrease in momentum when walking into objects (such as walls) at an angle as an exponential." +
        "Values between [0, 1] so values smaller than 1 create a positive curve and grater than 1 for a negative curve")]
        [SerializeField]
        private float anglePower = 0.5f;

        /// <summary>
        /// Maximum distance the player can be pushed out of overlapping objects in units per second.
        /// </summary>
        [Tooltip("Maximum distance a player can be pushed when overlapping other objects in units per second")]
        [SerializeField]
        private float maxPushSpeed = 1.0f;

        /// <summary>
        /// Distance that the character can "snap down" vertical steps.
        /// </summary>
        [Tooltip("Snap down distance when snapping onto the floor")]
        [SerializeField]
        private float verticalSnapDown = 0.2f;

        [Header("Stair and Step")]

        /// <summary>
        /// Minimum depth of a stair for a user to climb up
        /// (thinner steps than this value will not let the player climb).
        /// </summary>
        [Tooltip("Minimum depth of stairs when climbing up steps")]
        [SerializeField]
        private float stepUpDepth = 0.1f;

        /// <summary>
        /// Distance that the player can snap up when moving up stairs or vertical steps in terrain.
        /// </summary>
        [Tooltip("Maximum height of step the player can step up")]
        [SerializeField]
        private float verticalSnapUp = 0.3f;

        /// <summary>
        /// Time in which the player can snap up or down steps even after starting to fall.
        /// This property is useful to reduce the jerky stopping and moving effects when
        /// going up or down cliffs.
        /// </summary>
        [Tooltip("Time in which the player can snap up or down steps even after starting to fall")]
        [SerializeField]
        private float snapBufferTime = 0.05f;

        [Header("Player Jump Settings")]

        /// <summary>
        /// Velocity of player jump in units per second.
        /// </summary>
        [Tooltip("Vertical velocity of player jump")]
        [SerializeField]
        private float jumpVelocity = 5.0f;

        /// <summary>
        /// Maximum angle at which the player can jump (in degrees).
        /// </summary>
        [Tooltip("Maximum angle at which the player can jump (in degrees)")]
        [SerializeField]
        [Range(0, 90)]
        private float maxJumpAngle = 85f;

        /// <summary>
        /// Weight to which the player's jump is weighted towards the direction.
        /// of the surface they are standing on.
        /// </summary>
        [Tooltip("Weight to which the player's jump is weighted towards the angle of their surface")]
        [SerializeField]
        [Range(0, 1)]
        private float jumpAngleWeightFactor = 0.0f;

        /// <summary>
        /// Minimum time in seconds between player jumps
        /// </summary>
        [Tooltip("Minimum time in seconds between player jumps")]
        [SerializeField]
        private float jumpCooldown = 0.5f;

        /// <summary>
        /// Time in seconds that a player can jump after their feet leave the ground.
        /// </summary>
        [Tooltip("Time in seconds that a player can jump after their feet leave the ground")]
        [SerializeField]
        private float coyoteTime = 0.25f;

        /// <summary>
        /// Time in seconds that an input can be buffered for jumping.
        /// </summary>
        [Tooltip("Time in seconds that an input can be buffered for jumping")]
        [SerializeField]
        private float jumpBufferTime = 0.05f;

        [Header("Player Prone Settings")]

        /// <summary>
        /// Threshold time in which player is not moving to exit prone state.
        /// </summary>
        [Tooltip("Threshold time (in seconds) in which player is not moving to exit prone state")]
        [SerializeField]
        private float earlyStopProneThreshold = 0.2f;

        /// <summary>
        /// Threshold angular velocity in degrees per second for existing prone early.
        /// </summary>
        [Tooltip("Threshold angular velocity in degrees per second for existing prone early")]
        [SerializeField]
        private float thresholdAngularVelocity = 15;

        /// <summary>
        /// Threshold linear velocity in units per seconds for exiting prone early.
        /// </summary>
        [Tooltip("Threshold linear velocity in units per seconds for exiting prone early.")]
        [SerializeField]
        private float thresholdVelocity = 0.1f;

        /// <inheritdoc/>
        public int MaxBounces => maxBounces;

        /// <inheritdoc/>
        public float PushDecay => pushDecay;

        /// <inheritdoc/>
        public float VerticalSnapUp => verticalSnapUp;

        /// <inheritdoc/>
        public float StepUpDepth => stepUpDepth;

        /// <inheritdoc/>
        public float AnglePower => anglePower;

        /// <inheritdoc/>
        public bool CanSnapUp => !AttemptingJump && (snapUpPrevious || !Falling || elapsedFalling <= snapBufferTime);

        /// <inheritdoc/>
        public Vector3 Up => Vector3.up;

        /// <inheritdoc/>
        public IColliderCast ColliderCast => GetComponent<IColliderCast>();

        /// <inheritdoc/>
        public ICharacterPush Push => GetComponent<CharacterPush>();
    }
}
