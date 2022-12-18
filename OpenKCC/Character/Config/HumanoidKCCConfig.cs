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

using System;
using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace nickmaltbie.OpenKCC.Character.Config
{
    [Serializable]
    public class HumanoidKCCConfig : IKCCConfig, IEquatable<HumanoidKCCConfig>
    {
        [Header("Input Controls")]

        /// <summary>
        /// Action reference for moving the player.
        /// </summary>
        [Tooltip("Action reference for moving the player")]
        [FormerlySerializedAs("moveAction")]
        [SerializeField]
        public InputActionReference moveActionReference;

        /// <summary>
        /// Action reference for sprinting.
        /// </summary>
        [Tooltip("Action reference for moving the player")]
        [FormerlySerializedAs("sprintAction")]
        [SerializeField]
        public InputActionReference sprintActionReference;

        /// <summary>
        /// Override move action for testing.
        /// </summary>
        private InputAction overrideMoveAction;

        /// <summary>
        /// Override move action for testing.
        /// </summary>
        private InputAction overrideSprintAction;

        /// <summary>
        /// Gets the move action associated with this humaoid kcc config.
        /// </summary>
        public InputAction MoveAction
        {
            get => overrideMoveAction ?? moveActionReference?.action;
            set => overrideMoveAction = value;
        }

        /// <summary>
        /// Gets the move action associated with this humaoid kcc config.
        /// </summary>
        public InputAction SprintAction
        {
            get => overrideSprintAction ?? sprintActionReference?.action;
            set => overrideSprintAction = value;
        }

        /// <summary>
        /// Action reference for jumping.
        /// </summary>
        [Tooltip("Action reference for jumping")]
        [SerializeField]
        public JumpAction jumpAction;

        [Header("Ground Checking")]

        /// <summary>
        /// Current grounded state and configuration of the player.
        /// </summary>
        [SerializeField]
        public KCCGroundedState groundedState = new KCCGroundedState();

        /// <summary>
        /// Direction and strength of gravity
        /// </summary>
        [Tooltip("Direction and strength of gravity in units per second squared")]
        [SerializeField]
        public Vector3 gravity = new Vector3(0, -9.807f, 0);

        [Header("Motion Settings")]

        /// <summary>
        /// Speed of player movement when walking.
        /// </summary>
        [Tooltip("Speed of player when walking")]
        [SerializeField]
        public float walkingSpeed = 7.5f;

        /// <summary>
        /// Speed of player when sprinting.
        /// </summary>
        [Tooltip("Speed of player when sprinting")]
        [SerializeField]
        public float sprintSpeed = 10.0f;

        /// <summary>
        /// Maximum number of time player can bounce of walls/floors/objects during an update.
        /// </summary>
        [Tooltip("Maximum number of bounces when a player is moving")]
        [SerializeField]
        [Range(1, 10)]
        public int maxBounces = 5;

        /// <summary>
        /// Decay value of momentum when hitting another object.
        /// Should be between [0, 1].
        /// </summary>
        [Tooltip("Decay in momentum when hitting another object")]
        [SerializeField]
        [Range(0, 1)]
        public float pushDecay = 0.9f;

        /// <summary>
        /// Decrease in momentum factor due to angle change when walking.
        /// Should be a positive float value. It's an exponential applied to 
        /// values between [0, 1] so values smaller than 1 create a positive
        /// curve and grater than 1 for a negative curve.
        /// </summary>
        [Tooltip("Decrease in momentum when walking into objects (such as walls) at an angle as an exponential." +
        "Values between [0, 1] so values smaller than 1 create a positive curve and grater than 1 for a negative curve")]
        [SerializeField]
        public float anglePower = 0.5f;

        /// <summary>
        /// Maximum distance the player can be pushed out of overlapping objects in units per second.
        /// </summary>
        [Tooltip("Maximum distance a player can be pushed when overlapping other objects in units per second")]
        [SerializeField]
        public float maxPushSpeed = 10.0f;

        /// <summary>
        /// Distance that the character can "snap down" vertical steps.
        /// </summary>
        [Tooltip("Snap down distance when snapping onto the floor")]
        [SerializeField]
        public float verticalSnapDown = 0.2f;

        /// <summary>
        /// delta for distance floating over the ground when snapping down.
        /// </summary>
        [Tooltip("Delta for distance floating over the ground when snapping down.")]
        [SerializeField]
        public float minSnapThreshold = 0.01f;

        [Header("Stair and Step")]

        /// <summary>
        /// Minimum depth of a stair for a user to climb up
        /// (thinner steps than this value will not let the player climb).
        /// </summary>
        [Tooltip("Minimum depth of stairs when climbing up steps")]
        [SerializeField]
        public float stepUpDepth = 0.1f;

        /// <summary>
        /// Distance that the player can snap up when moving up stairs or vertical steps in terrain.
        /// </summary>
        [Tooltip("Maximum height of step the player can step up")]
        [SerializeField]
        public float verticalSnapUp = 0.3f;

        /// <summary>
        /// Time in which the player can snap up or down steps even after starting to fall.
        /// This property is useful to reduce the jerky stopping and moving effects when
        /// going up or down cliffs.
        /// </summary>
        [Tooltip("Time in which the player can snap up or down steps even after starting to fall")]
        [SerializeField]
        public float snapBufferTime = 0.05f;

        [Header("Moving Ground")]

        /// <summary>
        /// Max velocity at which the player can be launched
        /// when gaining momentum from a floor object without
        /// an IMovingGround attached to it.
        /// </summary>
        [Tooltip("Max velocity for launch without a rigidbody attached.")]
        [SerializeField]
        public float maxDefaultLaunchVelocity = 5.0f;

        /// <inheritdoc/>
        public int MaxBounces => maxBounces;

        /// <inheritdoc/>
        public float PushDecay => pushDecay;

        /// <inheritdoc/>
        public float VerticalSnapUp => verticalSnapUp;

        /// <inheritdoc/>
        public float StepUpDepth => stepUpDepth;

        /// <inheritdoc/>
        public float MaxDefaultLaunchVelocity => maxDefaultLaunchVelocity;

        /// <inheritdoc/>
        public float AnglePower => anglePower;

        /// <inheritdoc/>
        public bool CanSnapUp => !groundedState.Falling;

        /// <inheritdoc/>
        public Vector3 Up => Vector3.up;

        /// <inheritdoc/>
        public IColliderCast ColliderCast => _colliderCast;

        /// <inheritdoc/>
        public ICharacterPush Push => _characterPush;

        /// <summary>
        /// Downward direction for the player.
        /// </summary>
        public Vector3 Down => gravity.normalized;

        /// <summary>
        /// Collider cast associated with the character.
        /// </summary>
        public IColliderCast _colliderCast;

        /// <summary>
        /// Character push associated with the player.
        /// </summary>
        public ICharacterPush _characterPush;

        /// <summary>
        /// Check if two kcc configs have equivalent values.
        /// </summary>
        /// <param name="other">Other kcc config for comparison.</param>
        /// <returns>True if equal, false otherwise.</returns>
        public bool Equals(HumanoidKCCConfig other)
        {
            return
                other.moveActionReference.EqualOrNull(moveActionReference) &&
                other.moveActionReference.EqualOrNull(sprintActionReference) &&
                other.jumpAction.EqualOrNull(jumpAction) &&
                other.groundedState.Equals(groundedState) &&
                other.gravity.Equals(gravity) &&
                other.walkingSpeed.Equals(walkingSpeed) &&
                other.sprintSpeed.Equals(sprintSpeed) &&
                other.maxBounces.Equals(maxBounces) &&
                other.pushDecay.Equals(pushDecay) &&
                other.anglePower.Equals(anglePower) &&
                other.maxPushSpeed.Equals(maxPushSpeed) &&
                other.verticalSnapDown.Equals(verticalSnapDown) &&
                other.stepUpDepth.Equals(stepUpDepth) &&
                other.verticalSnapUp.Equals(verticalSnapUp) &&
                other.snapBufferTime.Equals(snapBufferTime) &&
                other.maxDefaultLaunchVelocity.Equals(maxDefaultLaunchVelocity);
        }
    }
}
