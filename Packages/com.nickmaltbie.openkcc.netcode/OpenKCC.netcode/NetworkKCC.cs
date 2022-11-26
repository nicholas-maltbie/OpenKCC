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
using nickmaltbie.NetworkStateMachineUnity;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Attributes;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Character.Events;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.StateMachineUnity;
using nickmaltbie.StateMachineUnity.Attributes;
using nickmaltbie.StateMachineUnity.Event;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations;
using static nickmaltbie.OpenKCC.Character.Animation.HumanoidKCCAnim;
using static nickmaltbie.OpenKCC.Utils.KCCUtils;

namespace nickmaltbie.OpenKCC.netcode
{
    /// <summary>
    /// Have a character controller push any dynamic rigidbody it hits
    /// </summary>
    [RequireComponent(typeof(ParentConstraint))]
    [RequireComponent(typeof(Rigidbody))]
    public class NetworkKCC : NetworkSMAnim, IJumping
    {
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

        /// <summary>
        /// Animation movement for the player
        /// </summary>
        private NetworkVariable<Vector2> animationMove = new NetworkVariable<Vector2>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner);

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
            if (!IsOwner)
            {
                return;
            }

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

            if (config.groundedState.Falling)
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
                Velocity += config.gravity * unityService.fixedDeltaTime;
            }

            config.jumpAction.ApplyJumpIfPossible();
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
            config.groundedState.CheckGrounded(config, transform.position, transform.rotation);
            IEvent groundedEvent;
            if (!config.groundedState.StandingOnGround)
            {
                groundedEvent = LeaveGroundEvent.Instance;
            }
            else if (config.groundedState.Sliding)
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
            KCCUtils.UpdateMovingGround(config.groundedState, config, transform, parentConstraint, ref floorConstraint);
        }

        /// <summary>
        /// Gets the velocity of the ground the player is standing on where the player is currently
        /// </summary>
        /// <returns>The velocity of the ground at the point the player is standing on</returns>
        public Vector3 GetGroundVelocity()
        {
            Vector3 groundVelocity = Vector3.zero;
            IMovingGround movingGround = config.groundedState.Floor?.GetComponent<IMovingGround>();
            if (movingGround != null && !movingGround.AvoidTransferMomentum())
            {
                // Weight movement of ground by ground movement weight
                float velocityWeight =
                    movingGround.GetMovementWeight(config.groundedState.GroundHitPosition, Velocity);
                float transferWeight =
                    movingGround.GetTransferMomentumWeight(config.groundedState.GroundHitPosition, Velocity);
                groundVelocity = movingGround.GetVelocityAtPoint(config.groundedState.GroundHitPosition);
                groundVelocity *= velocityWeight;
                groundVelocity *= transferWeight;
            }
            else if (config.groundedState.StandingOnGround)
            {
                float velocity = Mathf.Min(previousVelocity.magnitude, config.maxDefaultLaunchVelocity);
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
                float vel = config.walkingSpeed;

                string overrideParam = moveSettings.OverrideVelocityFunction;

                if (!string.IsNullOrEmpty(overrideParam))
                {
                    vel = (float)config.EvaluateMember(overrideParam);
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
                config.Down,
                config.verticalSnapDown,
                config.ColliderCast);
        }

        /// <summary>
        /// Configure kcc state machine operations.
        /// </summary>
        public override void Start()
        {
            base.Start();

            parentConstraint = GetComponent<ParentConstraint>();
            parentConstraint.constraintActive = false;
            parentConstraint.translationAxis = Axis.X | Axis.Y | Axis.Z;
            parentConstraint.rotationAxis = Axis.None;

            GetComponent<Rigidbody>().isKinematic = true;

            _cameraControls = GetComponent<ICameraControls>();
            config._characterPush = GetComponent<ICharacterPush>();
            config._colliderCast = GetComponent<IColliderCast>();

            if (IsOwner)
            {
                config.jumpAction.Setup(config.groundedState, config, this);
                config.moveAction.action.Enable();
            }
        }

        /// <inheritdoc/>
        public override void Update()
        {
            if (IsOwner)
            {
                bool denyMovement = PlayerInputUtils.playerMovementState == PlayerInputState.Deny;
                Vector2 moveVector = denyMovement ? Vector3.zero : config.moveAction.action.ReadValue<Vector2>();
                InputMovement = new Vector3(moveVector.x, 0, moveVector.y);
                bool moving = InputMovement.magnitude >= KCCUtils.Epsilon;
                RaiseEvent(moving ? StartMoveInput.Instance : StopMoveInput.Instance);

                if (moving)
                {
                    if (config.sprintAction.action.IsPressed())
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
                animationMove.Value = new Vector2(moveX, moveY);

                config.jumpAction.Update();
            }

            AttachedAnimator.SetFloat("MoveX", animationMove.Value.x);
            AttachedAnimator.SetFloat("MoveY", animationMove.Value.y);
            base.Update();
        }

        /// <summary>
        /// Get a vector of the projected movement onto the plane the player is standing on.
        /// </summary>
        /// <returns>Vector of player movement based on input velocity rotated by player view and projected onto the
        /// ground.</returns>
        public Vector3 GetProjectedMovement() => GetProjectedMovement(InputMovement);

        /// <summary>
        /// Push the player out of any overlapping objects. This will constrain movement to only 
        /// pushing the player at maxPushSpeed out of overlapping objects as to not violently teleport
        /// the player when they overlap with another object.
        /// </summary>
        /// <returns>Total distance player was pushed.</returns>
        public Vector3 PushOutOverlapping()
        {
            float fixedDeltaTime = Time.fixedDeltaTime;
            return config.ColliderCast.PushOutOverlapping(
                transform.position,
                transform.rotation,
                config.maxPushSpeed * fixedDeltaTime);
        }

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
        /// Move the player based on some vector of desired movement.
        /// </summary>
        /// <param name="movement">Movement in world space in which the player model should be moved.</param>
        public void MovePlayer(Vector3 movement)
        {
            foreach (KCCBounce bounce in KCCUtils.GetBounces(
                transform.position,
                movement,
                transform.rotation,
                config))
            {
                if (bounce.action == MovementAction.Stop)
                {
                    transform.position = bounce.finalPosition;
                }
            }
        }

        /// <summary>
        /// Teleport a player to a given position.
        /// </summary>
        /// <param name="position">Position to teleport player to.</param>
        public void TeleportPlayer(Vector3 position)
        {
            if (IsOwner)
            {
                KCCUtils.TeleportPlayer(transform, position, parentConstraint);
            }
        }

        /// <inheritdoc/>
        public void ApplyJump(Vector3 velocity)
        {
            if (IsOwner)
            {
                Velocity = velocity + GetGroundVelocity();
                RaiseEvent(JumpEvent.Instance);
            }
        }
    }
}
