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
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Character.Events;
using nickmaltbie.OpenKCC.FSM;
using nickmaltbie.OpenKCC.FSM.Attributes;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using static nickmaltbie.OpenKCC.Utils.KCCUtils;

namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Have a character controller push any dynamic rigidbody it hits
    /// </summary>
    public class KCCStateMachine : FixedStateMachineBehaviour, IKCCConfig
    {
        /// <summary>
        /// Unity service for managing unit inputs in a testable manner.
        /// </summary>
        public IUnityService unityService = UnityService.Instance;

        [Header("Input Controls")]

        /// <summary>
        /// Action reference for moving the player.
        /// </summary>
        [Tooltip("Action reference for moving the player")]
        [SerializeField]
        public InputActionReference moveAction;

        /// <summary>
        /// Action reference for jumping.
        /// </summary>
        [Tooltip("Action reference for moving the player")]
        [SerializeField]
        public InputActionReference jumpAction;

        /// <summary>
        /// Action reference for sprinting.
        /// </summary>
        [Tooltip("Action reference for moving the player")]
        [SerializeField]
        public InputActionReference sprintAction;

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
        public float maxPushSpeed = 1.0f;

        /// <summary>
        /// Distance that the character can "snap down" vertical steps.
        /// </summary>
        [Tooltip("Snap down distance when snapping onto the floor")]
        [SerializeField]
        public float verticalSnapDown = 0.2f;

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

        [Header("Player Jump Settings")]

        /// <summary>
        /// Velocity of player jump in units per second.
        /// </summary>
        [Tooltip("Vertical velocity of player jump")]
        [SerializeField]
        public float jumpVelocity = 5.0f;

        /// <summary>
        /// Maximum angle at which the player can jump (in degrees).
        /// </summary>
        [Tooltip("Maximum angle at which the player can jump (in degrees)")]
        [SerializeField]
        [Range(0, 90)]
        public float maxJumpAngle = 85f;

        /// <summary>
        /// Weight to which the player's jump is weighted towards the direction.
        /// of the surface they are standing on.
        /// </summary>
        [Tooltip("Weight to which the player's jump is weighted towards the angle of their surface")]
        [SerializeField]
        [Range(0, 1)]
        public float jumpAngleWeightFactor = 0.0f;

        /// <summary>
        /// Minimum time in seconds between player jumps
        /// </summary>
        [Tooltip("Minimum time in seconds between player jumps")]
        [SerializeField]
        public float jumpCooldown = 0.5f;

        /// <summary>
        /// Time in seconds that a player can jump after their feet leave the ground.
        /// </summary>
        [Tooltip("Time in seconds that a player can jump after their feet leave the ground")]
        [SerializeField]
        public float coyoteTime = 0.25f;

        /// <summary>
        /// Time in seconds that an input can be buffered for jumping.
        /// </summary>
        [Tooltip("Time in seconds that an input can be buffered for jumping")]
        [SerializeField]
        public float jumpBufferTime = 0.05f;

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
        public bool CanSnapUp => !groundedState.Falling;

        /// <inheritdoc/>
        public Vector3 Up => Vector3.up;

        /// <inheritdoc/>
        public IColliderCast ColliderCast => GetComponent<IColliderCast>();

        /// <inheritdoc/>
        public ICharacterPush Push => GetComponent<ICharacterPush>();

        /// <summary>
        /// Get the camera controls associated with the state machine.
        /// </summary>
        public ICameraControls Cameracontrols => GetComponent<ICameraControls>();

        /// <summary>
        /// Rotation of the plane the player is viewing
        /// </summary>
        private Quaternion HorizPlaneView => Quaternion.Euler(0, Cameracontrols?.Yaw ?? transform.eulerAngles.y, 0);

        /// <summary>
        /// Player rotated movement that they intend to move.
        /// </summary>
        /// <param name="inputMovement">Input movement vector of the player</param>
        public Vector3 RotatedMovement(Vector3 inputMovement) => HorizPlaneView * inputMovement;

        /// <summary>
        /// Downward direction for the player.
        /// </summary>
        public Vector3 Down => -Up;

        /// <summary>
        /// Player velocity in world space.
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// Input movement from player input updated each frame.
        /// </summary>
        public Vector3 InputMovement { get; private set; }

        [InitialState]
        [Transition(typeof(MoveInput), typeof(WalkingState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [MovementSettings(AllowVelocity = false, AllowWalk = false)]
        public class IdleState : State { }

        [Transition(typeof(StopMoveInput), typeof(IdleState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [MovementSettings(AllowVelocity = false, AllowWalk = true)]
        public class WalkingState : State { }

        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [Transition(typeof(GroundedEvent), typeof(IdleState))]
        [MovementSettings(AllowVelocity = true, AllowWalk = true)]
        [ApplyGravity]
        public class SlidingState : State { }

        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [Transition(typeof(GroundedEvent), typeof(IdleState))]
        [MovementSettings(AllowVelocity = true, AllowWalk = true)]
        [ApplyGravity]
        public class FallingState : State { }

        /// <inheritdoc/>
        public override void FixedUpdate()
        {
            groundedState.CheckGrounded(this, transform.position, transform.rotation);
            RaiseEvent(
                !groundedState.StandingOnGround ? LeaveGroundEvent.Instance :
                groundedState.Sliding ? SteepSlopeEvent.Instance :
                    GroundedEvent.Instance as IEvent);

            // Move the player based on movement settings
            var moveSettings = Attribute.GetCustomAttribute(CurrentState, typeof(MovementSettingsAttribute)) as MovementSettingsAttribute;

            // Move the player if they are allowed to walk
            if (moveSettings?.AllowWalk ?? false)
            {
                MovePlayer(GetProjectedMovement() * unityService.fixedDeltaTime);
            }

            // Apply gravity if needed
            if (Attribute.GetCustomAttribute(CurrentState, typeof(ApplyGravity)) is ApplyGravity)
            {
                Velocity += gravity * unityService.fixedDeltaTime;
            }

            if (moveSettings?.AllowVelocity ?? false)
            {
                MovePlayer(Velocity * unityService.fixedDeltaTime);
            }
            else
            {
                Velocity = Vector3.zero;
            }

            transform.position = KCCUtils.SnapPlayerDown(
                transform.position,
                transform.rotation,
                Down,
                verticalSnapDown,
                ColliderCast);

            base.FixedUpdate();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            Vector2 moveVector = moveAction.action.ReadValue<Vector2>();
            InputMovement = new Vector3(moveVector.x, 0, moveVector.y);
            RaiseEvent(InputMovement.magnitude >= KCCUtils.Epsilon ?
                MoveInput.Instance as IEvent : StopMoveInput.Instance as IEvent);

            Debug.Log(CurrentState);

            base.Update();
        }

        /// <summary>
        /// Get a vector of the projected movement onto the plane the player is standing on.
        /// </summary>
        /// <returns>Vector of player movement based on input velocity rotated by player view and projected onto the
        /// ground.</returns>
        public Vector3 GetProjectedMovement()
        {
            return GetProjectedMovement(InputMovement);
        }

        /// <summary>
        /// The the player's projected movement onto the ground based on some input movement vector.
        /// </summary>
        /// <param name="inputMovement">Input movement of the player.</param>
        /// <returns>Vector of player movement based on input velocity rotated by player view and projected onto the
        /// ground.</returns>
        public Vector3 GetProjectedMovement(Vector3 inputMovement)
        {
            Vector3 movement = RotatedMovement(inputMovement) * walkingSpeed;

            // If the player is standing on the ground, project their movement onto the ground plane
            // This allows them to walk up gradual slopes without facing a hit in movement speed
            if (!groundedState.Falling)
            {
                Vector3 projectedMovement = Vector3.ProjectOnPlane(movement, groundedState.SurfaceNormal).normalized *
                    movement.magnitude;
                if (projectedMovement.magnitude + KCCUtils.Epsilon >= movement.magnitude)
                {
                    movement = projectedMovement;
                }
            }

            return movement;
        }

        /// <summary>
        /// Move the player based on some vector of desired movement.
        /// </summary>
        /// <param name="movement">Movement in world space in which the player model should be moved.</param>
        public bool MovePlayer(Vector3 movement)
        {
            bool snappedUp = false;
            foreach (KCCBounce bounce in KCCUtils.GetBounces(
                transform.position,
                movement,
                transform.rotation,
                this))
            {
                if (bounce.action == MovementAction.Stop)
                {
                    transform.position = bounce.finalPosition;
                }
                else if (bounce.action == MovementAction.SnapUp)
                {
                    snappedUp = true;
                }
            }

            return snappedUp;
        }

        /// <summary>
        /// Attribute to apply gravity to player in a given state.
        /// </summary>
        public class ApplyGravity : Attribute { }

        /// <summary>
        /// Attribute to represent player movement settings for a given state.
        /// </summary>
        public class MovementSettingsAttribute : Attribute
        {
            /// <summary>
            /// Allow movement by normal velocity.
            /// </summary>
            public bool AllowVelocity = true;

            /// <summary>
            /// Allow movement by player input movement.
            /// </summary>
            public bool AllowWalk = true;
        }
    }
}
