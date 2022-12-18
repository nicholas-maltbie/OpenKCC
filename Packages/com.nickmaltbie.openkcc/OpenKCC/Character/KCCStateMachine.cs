﻿// Copyright (C) 2022 Nicholas Maltbie
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
    [DefaultExecutionOrder(1000)]
    public class KCCStateMachine : FixedSMAnim, IJumping, ISerializationCallbackReceiver
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
        /// Time in which the player has been falling.
        /// </summary>
        public float FallingTime { get; private set; }

        /// <summary>
        /// Camera controls associated with the player.
        /// </summary>
        protected ICameraControls _cameraControls;

        /// <summary>
        /// Get the camera controls associated with the state machine.
        /// </summary>
        public ICameraControls CameraControls { get => _cameraControls; internal set => _cameraControls = value; }

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
        /// Player velocity in world space.
        /// </summary>
        public Vector3 Velocity { get; private set; }

        /// <summary>
        /// Input movement from player input updated each frame.
        /// </summary>
        public Vector3 InputMovement { get; private set; }

        /// <summary>
        /// Position of the player previous frame.
        /// </summary>
        private Vector3 previousPosition;

        /// <summary>
        /// Velocity of the player from the previous frame.
        /// </summary>
        private Vector3 previousVelocity;

        /// <summary>
        /// Relative parent configuration for following the ground.
        /// </summary>
        private RelativeParentConfig relativeParentConfig;

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

        [ApplyGravity]
        [Animation(LandingAnimState, 0.1f, true)]
        [TransitionOnAnimationComplete(typeof(IdleState), 0.25f, true)]
        [AnimationTransition(typeof(StartMoveInput), typeof(WalkingState), 0.35f, true)]
        [AnimationTransition(typeof(JumpEvent), typeof(JumpState), 0.35f, true)]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [Transition(typeof(SteepSlopeEvent), typeof(SlidingState))]
        [MovementSettings(AllowVelocity = false, AllowWalk = true)]
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
        [MovementSettings(AllowVelocity = false, AllowWalk = true, SnapPlayerDown = true, OverrideVelocityFunction = nameof(config.sprintSpeed))]
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
        [Transition(typeof(JumpEvent), typeof(JumpState))]
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

        /// <summary>
        /// Update the grounded state of the kinematic character controller.
        /// </summary>
        public void UpdateGroundedState(Vector3 position, Quaternion rotation)
        {
            config.groundedState.CheckGrounded(config, position, rotation);
            var upwardVelocity = Vector3.Project(Velocity, config.Up);
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
        /// Apply movement of a player based on current state.
        /// </summary>
        /// <param name="position">Current position of the player.</param>
        /// <param name="rotation">Current rotation of the player.</param>
        /// <returns>Delta in position due to player movement.</returns>
        public Vector3 MovePlayer(Vector3 position, Quaternion rotation, float deltaTime)
        {
            // Move the player based on movement settings
            var moveSettings = Attribute.GetCustomAttribute(CurrentState, typeof(MovementSettingsAttribute)) as MovementSettingsAttribute;
            Vector3 delta = Vector3.zero;

            // Move the player if they are allowed to walk
            if (moveSettings?.AllowWalk ?? false)
            {
                Vector3 move = GetProjectedMovement();
                float speed = config.walkingSpeed;

                string overrideParam = moveSettings.OverrideVelocityFunction;

                if (!string.IsNullOrEmpty(overrideParam))
                {
                    speed = (float)config.EvaluateMember(overrideParam);
                }

                Vector3 moveDelta = GetMovement(position, move * speed * deltaTime, rotation, config);
                delta += moveDelta;
                position += moveDelta;
            }

            // Apply velocity if allowed to move via velocity
            // Only snap down if the player is currently grounded
            if (config.groundedState.StandingOnGround && (moveSettings?.SnapPlayerDown ?? false))
            {
                Vector3 snapDelta = GetSnapDelta(
                    position,
                    transform.rotation,
                    config.Down,
                    config.verticalSnapDown,
                    config.minSnapThreshold,
                    config.ColliderCast);
                delta += snapDelta;
                position += snapDelta;
            }

            // Apply velocity if allowed to move via velocity
            // Edge case, if player is  in sliding state, don't allow them to slide up surfaces
            bool slidingUp = CurrentState == typeof(SlidingState) && Vector3.Dot(config.Up, Velocity) > 0;
            if ((moveSettings?.AllowVelocity ?? false) && !slidingUp)
            {
                Vector3 velDelta = GetMovement(position, Velocity * deltaTime, rotation, config);
                delta += velDelta;
            }
            else
            {
                Velocity = Vector3.zero;
            }

            return delta;
        }

        /// <summary>
        /// Configure kcc state machine operations.
        /// </summary>
        public override void Awake()
        {
            base.Awake();

            GetComponent<Rigidbody>().isKinematic = true;
            _cameraControls = GetComponent<ICameraControls>();
            config._characterPush = GetComponent<ICharacterPush>();
            config._colliderCast = GetComponent<IColliderCast>();
            SetupInputs();
        }

        /// <summary>
        /// Setup inputs for the KCC
        /// </summary>
        public void SetupInputs()
        {
            config.jumpAction?.Setup(config.groundedState, config, this);
            config.MoveAction?.Enable();
        }

        public override void FixedUpdate()
        {
            GetComponent<Rigidbody>().isKinematic = true;
            ApplyMovement(unityService.fixedDeltaTime);
            base.FixedUpdate();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            ReadPlayerMovement();
            relativeParentConfig.FollowGround(transform);
            base.Update();
        }

        /// <inheritdoc/>
        public void ApplyJump(Vector3 velocity)
        {
            Vector3 groundVel = GetGroundVelocity(config.groundedState, config, previousVelocity);
            Velocity = velocity + groundVel;
            relativeParentConfig.Reset();
            RaiseEvent(JumpEvent.Instance);
        }

        /// <summary>
        /// Get a vector of the projected movement onto the plane the player is standing on.
        /// </summary>
        /// <returns>Vector of player movement based on input velocity rotated by player view and projected onto the
        /// ground.</returns>
        public Vector3 GetProjectedMovement() => GetProjectedMovement(InputMovement);

        /// <summary>
        /// The the player's projected movement onto the ground based on some input movement vector.
        /// </summary>
        /// <param name="inputMovement">Input movement of the player.</param>
        /// <returns>Vector of player movement based on input velocity rotated by player view and projected onto the
        /// ground.</returns>
        public Vector3 GetProjectedMovement(Vector3 inputMovement)
        {
            return config.groundedState.GetProjectedMovement(RotatedMovement(inputMovement));
        }

        /// <summary>
        /// Teleport player to a given position.
        /// </summary>
        /// <param name="position">Position to teleport player to.</param>
        public void TeleportPlayer(Vector3 position)
        {
            relativeParentConfig.Reset();
            transform.position = position;
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
            RaiseEvent(moving ? StartMoveInput.Instance : StopMoveInput.Instance);

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

        /// <summary>
        /// Applies player movement based on current state.
        /// Includes pushing out overlapping objects, updating grounded state, jumping,
        /// moving the player, and updating the grounded state.
        /// </summary>
        protected void ApplyMovement(float deltaTime)
        {
            relativeParentConfig.FollowGround(transform);
            Vector3 vel = (transform.position - previousPosition) / deltaTime;
            previousVelocity = Vector3.Lerp(previousVelocity, vel, 20 * deltaTime);

            Vector3 start = transform.position;
            Vector3 pos = start;

            // Push player out of overlapping objects
            Vector3 overlapPush = config.ColliderCast.PushOutOverlapping(pos, transform.rotation, config.maxPushSpeed * deltaTime);
            pos += overlapPush;

            // Allow player to move
            Vector3 playerMove = MovePlayer(pos, transform.rotation, deltaTime);
            pos += playerMove;

            // Compute player relative movement state based on final pos
            Vector3 delta = pos - start;
            UpdateGroundedState(pos, transform.rotation);

            // Allow player to attempt to jump
            config.jumpAction.ApplyJumpIfPossible();

            // Check if player is falling
            if (config.groundedState.Falling)
            {
                FallingTime += deltaTime;
            }
            else
            {
                FallingTime = 0;
            }

            // Apply gravity if needed
            if (Attribute.GetCustomAttribute(CurrentState, typeof(ApplyGravity)) is ApplyGravity)
            {
                Velocity += config.gravity * deltaTime;
            }

            transform.position += delta;
            transform.position += relativeParentConfig.UpdateMovingGround(transform.position, config.groundedState, delta, deltaTime);
            relativeParentConfig.FollowGround(transform);
            previousPosition = transform.position;
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
