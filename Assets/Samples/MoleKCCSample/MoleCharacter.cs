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
using nickmaltbie.OpenKCC.CameraControls;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Attributes;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Character.Events;
using nickmaltbie.OpenKCC.netcode.Utils;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.OpenKCC.Utils.ColliderCast;
using nickmaltbie.StateMachineUnity;
using nickmaltbie.StateMachineUnity.Attributes;
using nickmaltbie.StateMachineUnity.Event;
using Unity.Netcode;
using UnityEngine;

namespace nickmaltbie.OpenKCC.MoleKCCSample
{
    public class AvatarOffsetAttribute : Attribute
    {
        public string OffsetValue;
    }

    public class DiggingParticlesEnabled : Attribute { }

    /// <summary>
    /// Have a character controller push any dynamic rigidbody it hits
    /// </summary>
    [RequireComponent(typeof(MoleMovementEngine))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereColliderCast))]
    [DefaultExecutionOrder(1000)]
    public class MoleCharacter : NetworkSMAnim, IJumping, IGetKCCConfig, IGetKCCGrounded
    {
        [SerializeField]
        public ParticleSystem diggingParticles;

        [SerializeField]
        public ParticleSystem trailParticles;

        [SerializeField]
        public Vector3 avatarDiggingOffset = new Vector3(0, -0.5f, 0);

        private Vector3 defaultOffset;

        /// <summary>
        /// Values for configuring and managing KCC Config.
        /// </summary>
        [SerializeField]
        public MoleKCCConfig config = new MoleKCCConfig();

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
        private Quaternion HorizPlaneView
        {
            get
            {
                float rotation = CameraControls?.Yaw ?? transform.eulerAngles.y;
                return Quaternion.Euler(0, rotation, 0);
            }
        }

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

        /// <inheritdoc/>
        public IKCCConfig kccConfig => config;

        /// <inheritdoc/>
        public IKCCGrounded kccGrounded => config.groundedState;

        /// <summary>
        /// Movement engine for controlling the kinematic character controller.
        /// </summary>
        protected MoleMovementEngine movementEngine;

        /// <summary>
        /// Relative parent configuration for following the ground.
        /// </summary>
        private RelativeParentConfig relativeParentConfig;

        [InitialState]
        [Animation("Idle")]
        [Transition(typeof(StartMoveInput), typeof(WalkingState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [MovementSettings(AllowVelocity = false, AllowWalk = false, SnapPlayerDown = true)]
        public class IdleState : State { }

        [Animation("Jumping")]
        [ApplyGravity]
        [TransitionOnAnimationComplete(typeof(FallingState), 0.15f, true)]
        [Transition(typeof(GroundedEvent), typeof(IdleState))]
        [MovementSettings(AllowVelocity = true, AllowWalk = true, SpeedConfig = nameof(config.walkingSpeed))]
        public class JumpState : State { }

        [Animation("Digging")]
        [DiggingParticlesEnabled]
        [AvatarOffsetAttribute(OffsetValue=nameof(avatarDiggingOffset))]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(StopMoveInput), typeof(IdleState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [MovementSettings(AllowVelocity = false, AllowWalk = true, SnapPlayerDown = true, SpeedConfig = nameof(config.walkingSpeed))]
        public class WalkingState : State { }

        [Animation("Walking")]
        [ApplyGravity]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(GroundedEvent), typeof(IdleState))]
        [MovementSettings(AllowVelocity = true, AllowWalk = true, SpeedConfig = nameof(config.walkingSpeed))]
        public class FallingState : State { }

        /// <summary>
        /// Update the grounded state of the kinematic character controller.
        /// </summary>
        public void UpdateGroundedState()
        {
            if (config.groundedState.Falling)
            {
                RaiseEvent(LeaveGroundEvent.Instance);
            }
            else if (config.groundedState.StandingOnGround)
            {
                RaiseEvent(GroundedEvent.Instance);
            }
        }

        /// <summary>
        /// Configure kcc state machine operations.
        /// </summary>
        public override void Start()
        {
            base.Start();

            defaultOffset = AttachedAnimator.transform.localPosition;
            GetComponent<Rigidbody>().isKinematic = true;

            movementEngine = GetComponent<MoleMovementEngine>();
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
            if (IsOwner)
            {
                config.jumpAction?.Setup(config.groundedState, config, this);
                config.MoveAction?.Enable();
            }
        }

        public override void LateUpdate()
        {
            relativeParentConfig.FollowGround(transform);
            transform.position += config.ColliderCast.PushOutOverlapping(transform.position, transform.rotation, config.maxPushSpeed * unityService.deltaTime);

            base.LateUpdate();
        }

        /// <summary>
        /// The the player's desired velocity for their current input value.
        /// </summary>
        /// <returns>Vector of player velocity based on input movement rotated by player view and projected onto the
        /// ground.</returns>
        public Vector3 GetDesiredVelocity()
        {
            var moveDir = Quaternion.FromToRotation(Vector3.up, kccConfig.Up);
            Vector3 rotatedMovement = moveDir * (HorizPlaneView * InputMovement);

            var moveSettings = Attribute.GetCustomAttribute(
                CurrentState,
                typeof(MovementSettingsAttribute)) as MovementSettingsAttribute;

            float speed = moveSettings?.Speed(config) ?? config.walkingSpeed;
            Vector3 scaledMovement = rotatedMovement * speed;
            return scaledMovement;
        }

        public override void FixedUpdate()
        {
            GetComponent<Rigidbody>().isKinematic = true;
            if (IsOwner)
            {
                movementEngine.MovePlayer(
                    unityService.fixedDeltaTime,
                    GetDesiredVelocity() * unityService.fixedDeltaTime,
                    CurrentState);
                UpdateGroundedState();
                config.jumpAction.ApplyJumpIfPossible();

                // Set the player's rotation to follow the floor
                Quaternion rotation = Quaternion.identity;
                if (config.groundedState.StandingOnGround)
                {
                    rotation = Quaternion.FromToRotation(Vector3.up, config.groundedState.SurfaceNormal);
                }

                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 10 * unityService.fixedDeltaTime);
            }

            GetComponent<NetworkRelativeTransform>()?.UpdateState(relativeParentConfig);
            base.FixedUpdate();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            if (IsOwner)
            {
                ReadPlayerMovement();
            }

            bool particlesEnabled = Attribute.GetCustomAttribute(CurrentState, typeof(DiggingParticlesEnabled)) != null;

            if (particlesEnabled)
            {
                ParticleSystem.MainModule particleSettings = trailParticles.main;
                if (relativeParentConfig.previousParent == null)
                {
                    particleSettings.simulationSpace = ParticleSystemSimulationSpace.World;
                }
                else
                {
                    particleSettings.simulationSpace = ParticleSystemSimulationSpace.Custom;
                    particleSettings.customSimulationSpace = relativeParentConfig.previousParent;
                }
            }

            if (particlesEnabled != diggingParticles.isPlaying)
            {
                if (particlesEnabled)
                {
                    diggingParticles.Play();
                    trailParticles.Play();
                }
                else
                {
                    diggingParticles.Stop();
                    diggingParticles.Clear();
                    trailParticles.Stop();
                }
            }

            if (Attribute.GetCustomAttribute(CurrentState, typeof(AvatarOffsetAttribute)) is AvatarOffsetAttribute avatarOffset)
            {
                AttachedAnimator.transform.localPosition = (Vector3) this.EvaluateMember(avatarOffset.OffsetValue);
            }
            else
            {
                AttachedAnimator.transform.localPosition = defaultOffset;
            }

            base.Update();
        }

        /// <inheritdoc/>
        public void ApplyJump(Vector3 velocity)
        {
            if (IsOwner)
            {
                movementEngine.ApplyJump(velocity);
                RaiseEvent(JumpEvent.Instance);
            }
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

            bool moving = InputMovement.magnitude >= KCCUtils.Epsilon;
            IEvent moveEvent = moving ? StartMoveInput.Instance as IEvent : StopMoveInput.Instance as IEvent;
            RaiseEvent(moveEvent);
        }
    }
}
