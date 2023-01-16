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
using nickmaltbie.OpenKCC.CameraControls;
using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.OpenKCC.Character.Attributes;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Character.Events;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.StateMachineUnity;
using nickmaltbie.StateMachineUnity.Attributes;
using nickmaltbie.StateMachineUnity.Event;
using nickmaltbie.StateMachineUnity.Fixed;
using UnityEngine;
using UnityEngine.InputSystem;
using static nickmaltbie.OpenKCC.Character.Animation.HumanoidKCCAnim;
using static nickmaltbie.OpenKCC.Utils.KCCUtils;

namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Have a character controller push any dynamic rigidbody it hits
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(KCCMovementEngine))]
    [DefaultExecutionOrder(1000)]
    public class KCCStateMachine :
        FixedSMAnim,
        IJumping,
        IGetKCCConfig,
        IGetKCCGrounded,
        ISerializationCallbackReceiver
    {
        /// <summary>
        /// Current version fo the serialization.
        /// </summary>
        public const string CurrentSerializationVersion = "v1.0.0";

        /// <summary>
        /// Version of serialization for the KCC state machine.
        /// Used to migrate to newer version if using older version.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        internal string serializationVersion;

        /// <summary>
        /// Values for configuring and managing KCC Config.
        /// </summary>
        [SerializeField]
        public HumanoidKCCConfig config = new HumanoidKCCConfig();

        /// <summary>
        /// Camera controls associated with the player.
        /// </summary>
        protected ICameraControls _cameraControls;

        /// <summary>
        /// Movement engine for controlling the kinematic character controller.
        /// </summary>
        protected KCCMovementEngine movementEngine;

        /// <inheritdoc/>
        public IKCCConfig kccConfig => config;

        /// <inheritdoc/>
        public IKCCGrounded kccGrounded => config.groundedState;

        /// <summary>
        /// Get the camera controls associated with the state machine.
        /// </summary>
        public ICameraControls CameraControls { get => _cameraControls; internal set => _cameraControls = value; }

        /// <summary>
        /// Rotation of the plane the player is viewing
        /// </summary>
        public Quaternion HorizPlaneView => Quaternion.Euler(0, CameraControls?.Yaw ?? transform.eulerAngles.y, 0);

        /// <summary>
        /// Input movement from player input updated each frame.
        /// </summary>
        public Vector3 InputMovement { get; private set; }

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
        [MovementSettings(AllowVelocity = true, AllowWalk = true, SpeedConfig = nameof(config.walkingSpeed))]
        public class JumpState : State { }

        [ApplyGravity]
        [Animation(LandingAnimState, 0.1f, true)]
        [TransitionOnAnimationComplete(typeof(IdleState), 0.25f, true)]
        [AnimationTransition(typeof(StartMoveInput), typeof(WalkingState), 0.35f, true)]
        [AnimationTransition(typeof(JumpEvent), typeof(JumpState), 0.35f, true)]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [MovementSettings(AllowVelocity = false, AllowWalk = true, SpeedConfig = nameof(config.walkingSpeed))]
        public class LandingState : State { }

        [Animation(WalkingAnimState, 0.1f, true)]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(StopMoveInput), typeof(IdleState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [Transition(typeof(StartSprintEvent), typeof(SprintingState))]
        [MovementSettings(AllowVelocity = false, AllowWalk = true, SnapPlayerDown = true, SpeedConfig = nameof(config.walkingSpeed))]
        public class WalkingState : State { }

        [Animation(SprintingAnimState, 0.1f, true)]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(StopMoveInput), typeof(IdleState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [Transition(typeof(StopSprintEvent), typeof(WalkingState))]
        [MovementSettings(
            AllowVelocity = false,
            AllowWalk = true,
            SnapPlayerDown = true,
            SpeedConfig = nameof(config.sprintSpeed))]
        public class SprintingState : State { }

        [ApplyGravity]
        [Animation(SlidingAnimState, 0.35f, true)]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [AnimationTransition(typeof(GroundedEvent), typeof(LandingState), 0.35f, true, 0.25f)]
        [MovementSettings(AllowVelocity = true, AllowWalk = true, SnapPlayerDown = true, SpeedConfig = nameof(config.walkingSpeed))]
        public class SlidingState : State { }

        [ApplyGravity]
        [Animation(FallingAnimState, 0.1f, true)]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [AnimationTransition(typeof(GroundedEvent), typeof(LandingState), 0.35f, true, 0.25f)]
        [TransitionAfterTime(typeof(LongFallingState), 2.0f)]
        [MovementSettings(AllowVelocity = true, AllowWalk = true, SpeedConfig = nameof(config.walkingSpeed))]
        public class FallingState : State { }

        [ApplyGravity]
        [Animation(LongFallingAnimState, 0.1f, true)]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [AnimationTransition(typeof(GroundedEvent), typeof(LandingState), 0.35f, true, 1.0f)]
        [MovementSettings(AllowVelocity = true, AllowWalk = true, SpeedConfig = nameof(config.walkingSpeed))]
        public class LongFallingState : State { }

        /// <summary>
        /// Configure kcc state machine operations.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            GetComponent<Rigidbody>().isKinematic = true;
            movementEngine = GetComponent<KCCMovementEngine>();
            _cameraControls = GetComponent<ICameraControls>();
            config._characterPush = GetComponent<ICharacterPush>();
            config._colliderCast = GetComponent<IColliderCast>();
            SetupInputs();
        }

        /// <summary>
        /// Update the grounded state of the kinematic character controller.
        /// </summary>
        public void UpdateGroundedState()
        {
            var upwardVelocity = Vector3.Project(movementEngine.Velocity, config.Up);
            bool movingUp = Vector3.Dot(upwardVelocity, config.Up) > 0;

            if (config.groundedState.Falling)
            {
                RaiseEvent(LeaveGroundEvent.Instance);
            }
            else if (config.groundedState.Sliding)
            {
                RaiseEvent(SteepSlopeEvent.Instance);
            }
            else if (config.groundedState.StandingOnGround && !movingUp)
            {
                RaiseEvent(GroundedEvent.Instance);
            }
        }

        /// <summary>
        /// Setup inputs for the KCC
        /// </summary>
        public void SetupInputs()
        {
            config.jumpAction?.Setup(config.groundedState, config, this);
            config.MoveAction?.Enable();
        }

        /// <inheritdoc/>
        public override void FixedUpdate()
        {
            GetComponent<Rigidbody>().isKinematic = true;
            movementEngine.MovePlayer(
                unityService.fixedDeltaTime,
                GetDesiredVelocity() * unityService.fixedDeltaTime,
                CurrentState);
            UpdateGroundedState();
            config.jumpAction.ApplyJumpIfPossible();

            base.FixedUpdate();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            ReadPlayerMovement();
            base.Update();
        }

        /// <inheritdoc/>
        public void ApplyJump(Vector3 velocity)
        {
            movementEngine.ApplyJump(velocity);
            RaiseEvent(JumpEvent.Instance);
        }

        /// <summary>
        /// The the player's desired velocity for their current input value.
        /// </summary>
        /// <returns>Vector of player velocity based on input movement rotated by player view and projected onto the
        /// ground.</returns>
        public Vector3 GetDesiredVelocity()
        {
            Vector3 rotatedMovement = HorizPlaneView * InputMovement;
            Vector3 projectedMovement = config.groundedState.GetProjectedMovement(rotatedMovement);

            var moveSettings = Attribute.GetCustomAttribute(
                CurrentState,
                typeof(MovementSettingsAttribute)) as MovementSettingsAttribute;
            
            float speed = moveSettings?.Speed(config) ?? config.walkingSpeed;
            Vector3 scaledMovement = projectedMovement * speed;
            return scaledMovement;
        }

        /// <summary>
        /// Teleport player to a given position.
        /// </summary>
        /// <param name="position">Position to teleport player to.</param>
        public void TeleportPlayer(Vector3 position)
        {
            movementEngine.TeleportPlayer(position);
        }

        /// <summary>
        /// Read the current player input values.
        /// </summary>
        public void ReadPlayerMovement()
        {
            bool denyMovement = PlayerInputUtils.playerMovementState == PlayerInputState.Deny;
            Vector2 moveVector = denyMovement ? Vector2.zero : config.MoveAction?.ReadValue<Vector2>() ?? Vector2.zero;
            InputMovement = new Vector3(moveVector.x, 0, moveVector.y);
            config.jumpAction.Update();

            float moveX = AttachedAnimator.GetFloat("MoveX");
            float moveY = AttachedAnimator.GetFloat("MoveY");
            moveX = Mathf.Lerp(moveX, moveVector.x, 4 * unityService.deltaTime);
            moveY = Mathf.Lerp(moveY, moveVector.y, 4 * unityService.deltaTime);
            AttachedAnimator.SetFloat("MoveX", moveX);
            AttachedAnimator.SetFloat("MoveY", moveY);

            bool moving = InputMovement.magnitude >= KCCUtils.Epsilon;
            IEvent moveEvent = moving ? StartMoveInput.Instance as IEvent : StopMoveInput.Instance as IEvent;
            RaiseEvent(moveEvent);

            if (moving)
            {
                if (config.SprintAction?.IsPressed() ?? false)
                {
                    RaiseEvent(StartSprintEvent.Instance);
                }
                else
                {
                    RaiseEvent(StopSprintEvent.Instance);
                }
            }
        }

        #region Parse Depreciated Configurations of KCC StateMachine.
        /// <summary>
        /// Persist volatile fields to serialized data before
        /// unity serializes the object.
        /// </summary>
        public void OnBeforeSerialize()
        {
            // Do nothing for now...
        }

        /// <summary>
        /// Allow for parsing depreciated fields into the newer
        /// format.
        /// </summary>
        public void OnAfterDeserialize()
        {
            // If HumanoidKCCConfig config is uninitialized, read in values from previous
            // depreciated fields
            if (string.IsNullOrEmpty(serializationVersion))
            {
                // Disabling warnings because this method is to convert from obsolete
                // fields to current structure.
#pragma warning disable CS0618 // Type or member is obsolete
                config.moveActionReference = moveAction;
                config.sprintActionReference = sprintAction;
                config.jumpAction = jumpAction;
                config.groundedState = groundedState;
                config.gravity = gravity;
                config.walkingSpeed = walkingSpeed;
                config.sprintSpeed = sprintSpeed;
                config.maxBounces = maxBounces;
                config.pushDecay = pushDecay;
                config.anglePower = anglePower;
                config.maxPushSpeed = maxPushSpeed;
                config.verticalSnapDown = verticalSnapDown;
                config.stepUpDepth = stepUpDepth;
                config.verticalSnapUp = verticalSnapUp;
                config.snapBufferTime = snapBufferTime;
                config.maxDefaultLaunchVelocity = maxDefaultLaunchVelocity;
                serializationVersion = CurrentSerializationVersion;
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
        #endregion

        #region Fields To Be Depreciated
        private const string DepreciatedMessage =
            "This field is no longer used and has been replaced with a" +
            "corresponding HumanoidKCCConfig config field. Please do not use" +
            "this value as it will be removed in a future update.";

        /// <summary>
        /// Action reference for moving the player.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public InputActionReference moveAction;

        /// <summary>
        /// Action reference for sprinting.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public InputActionReference sprintAction;

        /// <summary>
        /// Action reference for jumping.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public JumpAction jumpAction;

        /// <summary>
        /// Current grounded state and configuration of the player.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public KCCGroundedState groundedState = new KCCGroundedState();

        /// <summary>
        /// Direction and strength of gravity
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public Vector3 gravity = new Vector3(0, -9.807f, 0);

        /// <summary>
        /// Speed of player movement when walking.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public float walkingSpeed = 7.5f;

        /// <summary>
        /// Speed of player when sprinting.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public float sprintSpeed = 10.0f;

        /// <summary>
        /// Maximum number of time player can bounce of walls/floors/objects during an update.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public int maxBounces = 5;

        /// <summary>
        /// Decay value of momentum when hitting another object.
        /// Should be between [0, 1].
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public float pushDecay = 0.9f;

        /// <summary>
        /// Decrease in momentum factor due to angle change when walking.
        /// Should be a positive float value. It's an exponential applied to 
        /// values between [0, 1] so values smaller than 1 create a positive
        /// curve and grater than 1 for a negative curve.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public float anglePower = 0.5f;

        /// <summary>
        /// Maximum distance the player can be pushed out of overlapping objects in units per second.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public float maxPushSpeed = 1.0f;

        /// <summary>
        /// Distance that the character can "snap down" vertical steps.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public float verticalSnapDown = 0.2f;

        /// <summary>
        /// Minimum depth of a stair for a user to climb up
        /// (thinner steps than this value will not let the player climb).
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public float stepUpDepth = 0.1f;

        /// <summary>
        /// Distance that the player can snap up when moving up stairs or vertical steps in terrain.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public float verticalSnapUp = 0.3f;

        /// <summary>
        /// Time in which the player can snap up or down steps even after starting to fall.
        /// This property is useful to reduce the jerky stopping and moving effects when
        /// going up or down cliffs.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public float snapBufferTime = 0.05f;

        /// <summary>
        /// Max velocity at which the player can be launched
        /// when gaining momentum from a floor object without
        /// an IMovingGround attached to it.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        [Obsolete(DepreciatedMessage)]
        public float maxDefaultLaunchVelocity = 5.0f;
        #endregion
    }
}
