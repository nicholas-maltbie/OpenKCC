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
using System.Collections.Generic;
using nickmaltbie.NetworkStateMachineUnity;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Character.Events;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.StateMachineUnity;
using nickmaltbie.StateMachineUnity.Attributes;
using nickmaltbie.StateMachineUnity.Event;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using static nickmaltbie.OpenKCC.Utils.KCCUtils;

namespace nickmaltbie.OpenKCC.netcode
{
    /// <summary>
    /// Have a character controller push any dynamic rigidbody it hits
    /// </summary>
    [RequireComponent(typeof(ParentConstraint))]
    [RequireComponent(typeof(Rigidbody))]
    public class KCCStateMachine : NetworkSMAnim, IKCCConfig, IJumping
    {
        public const string IdleAnimState = "Idle";
        public const string JumpAnimState = "Jump";
        public const string LandingAnimState = "Landing";
        public const string WalkingAnimState = "Walking";
        public const string SprintingAnimState = "Sprinting";
        public const string SlidingAnimState = "Sliding";
        public const string FallingAnimState = "Falling";
        public const string LongFallingAnimState = "Long Falling";

        [Header("Input Controls")]

        /// <summary>
        /// Action reference for moving the player.
        /// </summary>
        [Tooltip("Action reference for moving the player")]
        [SerializeField]
        public InputActionReference moveAction;

        /// <summary>
        /// Action reference for sprinting.
        /// </summary>
        [Tooltip("Action reference for moving the player")]
        [SerializeField]
        public InputActionReference sprintAction;

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
        public float AnglePower => anglePower;

        /// <inheritdoc/>
        public bool CanSnapUp => !groundedState.Falling;

        /// <summary>
        /// Time in which the player has been falling.
        /// </summary>
        public float FallingTime { get; private set; }

        /// <inheritdoc/>
        public Vector3 Up => Vector3.up;

        /// <summary>
        /// Collider cast associated with the character.
        /// </summary>
        internal IColliderCast _colliderCast;

        /// <inheritdoc/>
        public IColliderCast ColliderCast => _colliderCast;

        /// <summary>
        /// Character push associated with the player.
        /// </summary>
        internal ICharacterPush _characterPush;

        /// <inheritdoc/>
        public ICharacterPush Push => _characterPush;

        /// <summary>
        /// Camera controls associated with the player.
        /// </summary>
        internal ICameraControls _cameraControls;

        /// <summary>
        /// Get the camera controls associated with the state machine.
        /// </summary>
        public ICameraControls CameraControls => _cameraControls;

        /// <summary>
        /// Rotation of the plane the player is viewing
        /// </summary>
        private Quaternion HorizPlaneView => Quaternion.Euler(0, CameraControls?.Yaw ?? transform.eulerAngles.y, 0);

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

        /// <summary>
        /// Parent constraints to attach position to floor.
        /// </summary>
        private ParentConstraint parentConstraint;

        /// <summary>
        /// Floor Constraint used for parent constraint to move with ground/
        /// </summary>
        private ConstraintSource floorConstraint;

        /// <summary>
        /// Position of the player previous frame.
        /// </summary>
        private Vector3 previousPosition;

        /// <summary>
        /// Velocity of the player from the previous frame.
        /// </summary>
        private Vector3 previousVelocity;

        [InitialState]
        [Animation(IdleAnimState, 0.35f, true)]
        [Transition(typeof(StartMoveInput), typeof(WalkingState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [MovementSettings(AllowVelocity = false, AllowWalk = false, SnapPlayerDown = true)]
        public class IdleState : State { }

        [ApplyGravity]
        [Animation(JumpAnimState, 0.1f, true)]
        [TransitionOnAnimationComplete(typeof(FallingState), 0.15f, true)]
        [AnimationTransition(typeof(GroundedEvent), typeof(LandingState), 0.35f, true, 0.25f)]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [MovementSettings(AllowVelocity = true, AllowWalk = true)]
        public class JumpState : State { }

        [Animation(LandingAnimState, 0.1f, true)]
        [TransitionOnAnimationComplete(typeof(IdleState), 0.25f, true)]
        [AnimationTransition(typeof(StartMoveInput), typeof(WalkingState), 0.35f, true)]
        [AnimationTransition(typeof(JumpEvent), typeof(JumpState), 0.35f, true)]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [MovementSettings(AllowVelocity = false, AllowWalk = true, SnapPlayerDown = true)]
        public class LandingState : State { }

        [Animation(WalkingAnimState, 0.1f, true)]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(StopMoveInput), typeof(IdleState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [Transition(typeof(StartSprintEvent), typeof(SprintingState))]
        [MovementSettings(AllowVelocity = false, AllowWalk = true, SnapPlayerDown = true)]
        public class WalkingState : State { }

        [Animation(SprintingAnimState, 0.1f, true)]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(StopMoveInput), typeof(IdleState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [Transition(typeof(StopSprintEvent), typeof(WalkingState))]
        [MovementSettings(AllowVelocity = false, AllowWalk = true, SnapPlayerDown = true, OverrideVelocityFunction = nameof(sprintSpeed))]
        public class SprintingState : State { }

        [ApplyGravity]
        [Animation(SlidingAnimState, 0.35f, true)]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [AnimationTransition(typeof(GroundedEvent), typeof(LandingState), 0.35f, true, 0.25f)]
        [MovementSettings(AllowVelocity = true, AllowWalk = true, SnapPlayerDown = true)]
        public class SlidingState : State { }

        [ApplyGravity]
        [Animation(FallingAnimState, 0.1f, true)]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [AnimationTransition(typeof(GroundedEvent), typeof(LandingState), 0.35f, true, 0.25f)]
        [TransitionAfterTime(typeof(LongFallingState), 2.0f)]
        [MovementSettings(AllowVelocity = true, AllowWalk = true)]
        public class FallingState : State { }

        [ApplyGravity]
        [Animation(LongFallingAnimState, 0.1f, true)]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [AnimationTransition(typeof(GroundedEvent), typeof(LandingState), 0.35f, true, 1.0f)]
        [MovementSettings(AllowVelocity = true, AllowWalk = true)]
        public class LongFallingState : State { }

        /// <inheritdoc/>
        public override void FixedUpdate()
        {
            // Compute displacement without player movement
            Vector3 start = transform.position;

            while (parentConstraint.sourceCount > 0)
            {
                parentConstraint.RemoveSource(0);
            }

            // Push player out of overlapping objects
            PushOutOverlapping();

            // Update grounded state
            UpdateGroundedState();

            if (CurrentState == typeof(FallingState))
            {
                FallingTime += unityService.fixedDeltaTime;
            }
            else
            {
                FallingTime = 0;
            }

            // Apply gravity if needed
            if (Attribute.GetCustomAttribute(CurrentState, typeof(ApplyGravity)) is ApplyGravity)
            {
                Velocity += gravity * unityService.fixedDeltaTime;
            }

            jumpAction.ApplyJumpIfPossible();
            ApplyMovement();

            base.FixedUpdate();

            UpdateMovingGround();
            UpdateGroundedState();

            Vector3 disp = start - previousPosition;
            Vector3 vel = disp / unityService.fixedDeltaTime;

            previousVelocity = Vector3.Lerp(previousVelocity, vel, 0.5f);
            previousPosition = transform.position;
        }

        /// <summary>
        /// Update the grounded state of the kinematic character controller.
        /// </summary>
        public void UpdateGroundedState()
        {
            groundedState.CheckGrounded(this, transform.position, transform.rotation);
            IEvent groundedEvent;
            if (!groundedState.StandingOnGround)
            {
                groundedEvent = LeaveGroundEvent.Instance;
            }
            else if (groundedState.Sliding)
            {
                groundedEvent = SteepSlopeEvent.Instance;
            }
            else
            {
                groundedEvent = GroundedEvent.Instance;
            }

            RaiseEvent(groundedEvent);
        }

        /// <summary>
        /// Update the moving ground state of the KCC State machine.
        /// </summary>
        public void UpdateMovingGround()
        {
            groundedState.CheckGrounded(this, transform.position, transform.rotation);
            parentConstraint.constraintActive = groundedState.StandingOnGround;
            parentConstraint.translationAtRest = transform.position;
            parentConstraint.rotationAtRest = transform.rotation.eulerAngles;

            if (groundedState.StandingOnGroundOrOverlap && groundedState.Floor != null)
            {
                IMovingGround ground = groundedState.Floor.GetComponent<IMovingGround>();

                if (ground == null || ground.ShouldAttach())
                {
                    Transform floorTransform = groundedState.Floor.transform;
                    floorConstraint.sourceTransform = floorTransform;
                    floorConstraint.weight = 1.0f;
                    parentConstraint.AddSource(floorConstraint);

                    Vector3 relativePos = transform.position - floorTransform.position;
                    parentConstraint.SetTranslationOffset(0, floorTransform.InverseTransformDirection(relativePos));
                }
                else
                {
                    transform.position += ground.GetDisplacementAtPoint(transform.position);
                }
            }
            else
            {
                floorConstraint = default;
            }
        }

        /// <summary>
        /// Gets the velocity of the ground the player is standing on where the player is currently
        /// </summary>
        /// <returns>The velocity of the ground at the point the player is standing on</returns>
        public Vector3 GetGroundVelocity()
        {
            Vector3 groundVelocity = Vector3.zero;
            IMovingGround movingGround = groundedState.Floor?.GetComponent<IMovingGround>();
            if (movingGround != null && !movingGround.AvoidTransferMomentum())
            {
                // Weight movement of ground by ground movement weight
                float velocityWeight =
                    movingGround.GetMovementWeight(groundedState.GroundHitPosition, Velocity);
                float transferWeight =
                    movingGround.GetTransferMomentumWeight(groundedState.GroundHitPosition, Velocity);
                groundVelocity = movingGround.GetVelocityAtPoint(groundedState.GroundHitPosition);
                groundVelocity *= velocityWeight;
                groundVelocity *= transferWeight;
            }
            else if (groundedState.StandingOnGround)
            {
                float velocity = Mathf.Min(previousVelocity.magnitude, maxDefaultLaunchVelocity);
                groundVelocity = previousVelocity.normalized * velocity;
            }

            return groundVelocity;
        }

        /// <summary>
        /// Apply player movement if allowed.
        /// </summary>
        public void ApplyMovement()
        {
            // Move the player based on movement settings
            var moveSettings = Attribute.GetCustomAttribute(CurrentState, typeof(MovementSettingsAttribute)) as MovementSettingsAttribute;

            // Move the player if they are allowed to walk
            if (moveSettings?.AllowWalk ?? false)
            {
                Vector3 movementDir = GetProjectedMovement();
                float vel = walkingSpeed;

                string overrideParam = moveSettings.OverrideVelocityFunction;

                if (!string.IsNullOrEmpty(overrideParam))
                {
                    vel = (float)GetType().GetField(overrideParam).GetValue(this);
                }

                MovePlayer(movementDir * vel * unityService.fixedDeltaTime);
            }

            // Apply velocity fi allowed to move via velocity
            if (moveSettings?.AllowVelocity ?? false)
            {
                MovePlayer(Velocity * unityService.fixedDeltaTime);
            }
            else
            {
                Velocity = Vector3.zero;
            }

            // Snap player down if requested
            if (moveSettings?.SnapPlayerDown ?? false)
            {
                SnapPlayerDown();
            }
        }

        /// <summary>
        /// Snap the player down based on their current position.
        /// </summary>
        public void SnapPlayerDown()
        {
            transform.position = KCCUtils.SnapPlayerDown(
                transform.position,
                transform.rotation,
                Down,
                verticalSnapDown,
                ColliderCast);
        }

        /// <summary>
        /// Configure kcc state machine operations.
        /// </summary>
        public void Awake()
        {
            parentConstraint = GetComponent<ParentConstraint>();
            parentConstraint.constraintActive = false;
            parentConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
            parentConstraint.rotationAxis = Axis.None;

            GetComponent<Rigidbody>().isKinematic = true;

            _cameraControls = GetComponent<ICameraControls>();
            _characterPush = GetComponent<ICharacterPush>();
            _colliderCast = GetComponent<IColliderCast>();
        }

        /// <summary>
        /// Setup KCC StateMachine inputs.
        /// </summary>
        public override void Start()
        {
            base.Start();

            jumpAction.Setup(groundedState, this, this);
            moveAction.action.Enable();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            bool denyMovement = PlayerInputUtils.playerMovementState == PlayerInputState.Deny;
            Vector2 moveVector = denyMovement ? Vector3.zero : moveAction.action.ReadValue<Vector2>();
            InputMovement = new Vector3(moveVector.x, 0, moveVector.y);
            bool moving = InputMovement.magnitude >= KCCUtils.Epsilon;
            RaiseEvent(moving ? StartMoveInput.Instance : StopMoveInput.Instance);

            if (moving)
            {
                if (sprintAction.action.IsPressed())
                {
                    RaiseEvent(StartSprintEvent.Instance);
                }
                else
                {
                    RaiseEvent(StopSprintEvent.Instance);
                }
            }

            float moveX = AttachedAnimator.GetFloat("MoveX");
            float moveY = AttachedAnimator.GetFloat("MoveY");
            moveX = Mathf.Lerp(moveX, moveVector.x, 4 * unityService.deltaTime);
            moveY = Mathf.Lerp(moveY, moveVector.y, 4 * unityService.deltaTime);
            AttachedAnimator.SetFloat("MoveX", moveX);
            AttachedAnimator.SetFloat("MoveY", moveY);

            jumpAction.Update();
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
        /// Push the player out of any overlapping objects. This will constrain movement to only 
        /// pushing the player at maxPushSpeed out of overlapping objects as to not violently teleport
        /// the player when they overlap with another object.
        /// </summary>
        /// <returns>Total distance player was pushed.</returns>
        public Vector3 PushOutOverlapping()
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            return ColliderCast.PushOutOverlapping(
                transform.position,
                transform.rotation,
                maxPushSpeed * fixedDeltaTime);
        }

        /// <summary>
        /// The the player's projected movement onto the ground based on some input movement vector.
        /// </summary>
        /// <param name="inputMovement">Input movement of the player.</param>
        /// <returns>Vector of player movement based on input velocity rotated by player view and projected onto the
        /// ground.</returns>
        public Vector3 GetProjectedMovement(Vector3 inputMovement)
        {
            Vector3 movement = RotatedMovement(inputMovement);

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
        public void MovePlayer(Vector3 movement)
        {
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
            }
        }

        public void TeleportPlayer(Vector3 position)
        {
            var sources = new List<ConstraintSource>();
            parentConstraint.GetSources(sources);
            if (parentConstraint.sourceCount > 0)
            {
                parentConstraint.RemoveSource(0);
            }

            transform.position = position;
            parentConstraint.SetSources(sources);
        }

        /// <inheritdoc/>
        public void ApplyJump(Vector3 velocity)
        {
            Velocity = velocity + GetGroundVelocity();
            RaiseEvent(JumpEvent.Instance);
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
            public bool AllowVelocity = false;

            /// <summary>
            /// Allow movement by player input movement.
            /// </summary>
            public bool AllowWalk = false;

            /// <summary>
            /// Should the player be snapped down after moving.
            /// </summary>
            public bool SnapPlayerDown = false;

            /// <summary>
            /// Function to override velocity value.
            /// </summary>
            public string OverrideVelocityFunction = null;
        }
    }
}
