// Copyright (C) 2023 Nicholas Maltbie
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
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.OpenKCC.Character.Attributes;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Character.Events;
using nickmaltbie.OpenKCC.netcode.Utils;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.OpenKCC.Utils.ColliderCast;
using nickmaltbie.StateMachineUnity;
using nickmaltbie.StateMachineUnity.Attributes;
using nickmaltbie.StateMachineUnity.Event;
using nickmaltbie.StateMachineUnity.netcode;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

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
    [RequireComponent(typeof(MoleParticles))]
    [RequireComponent(typeof(MoleMovementEngine))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereColliderCast))]
    [DefaultExecutionOrder(1000)]
    public class MoleCharacter : NetworkSMAnim, IJumping
    {
        [SerializeField]
        public Vector3 avatarDiggingOffset = new Vector3(0, 0, 0);

        [Header("Input Controls")]

        /// <summary>
        /// Action reference for moving the player.
        /// </summary>
        [Tooltip("Action reference for moving the player")]
        [SerializeField]
        public InputActionReference moveActionReference;

        /// <summary>
        /// Action reference for sprinting.
        /// </summary>
        [Tooltip("Action reference for player jumping")]
        [SerializeField]
        public InputActionReference jumpActionReference;

        [Header("Movement Settings")]

        /// <summary>
        /// Speed of player movement when walking.
        /// </summary>
        [Tooltip("Speed of player when walking")]
        [SerializeField]
        public float walkingSpeed = 7.5f;

        /// <summary>
        /// Velocity of player jump.
        /// </summary>
        [Tooltip("Velocity of player jump.")]
        [SerializeField]
        public float jumpVelocity = 6.5f;

        [Tooltip("Threshold angle for deciding between up and down")]
        [SerializeField]
        [Range(0, 90)]
        public float upFlipThreshold = 80.0f;

        private bool previousFacingUp = true;

        /// <summary>
        /// Action reference for jumping.
        /// </summary>
        internal JumpAction jumpAction;

        /// <summary>
        /// Override move action for testing.
        /// </summary>
        private InputAction overrideMoveAction;

        /// <summary>
        /// Gets the move action associated with this kcc.
        /// </summary>
        public InputAction MoveAction
        {
            get => overrideMoveAction ?? moveActionReference?.action;
            set => overrideMoveAction = value;
        }

        private NetworkVariable<Vector3> relativeUp = new NetworkVariable<Vector3>(
            Vector3.up,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner);

        private Vector3 defaultOffset;

        /// <summary>
        /// Time in which the player has been falling.
        /// </summary>
        public float FallingTime { get; private set; }

        /// <summary>
        /// Player velocity in world space.
        /// </summary>
        public Vector3 Velocity { get; protected set; }

        /// <summary>
        /// Input movement from player input updated each frame.
        /// </summary>
        public Vector3 InputMovement { get; private set; }

        /// <summary>
        /// Movement engine for controlling the kinematic character controller.
        /// </summary>
        protected MoleMovementEngine movementEngine;

        /// <summary>
        /// Relative parent configuration for following the ground.
        /// </summary>
        public RelativeParentConfig relativeParentConfig { get; protected set; } = new RelativeParentConfig();

        /// <summary>
        /// Position from previous frame for rotating mole.
        /// </summary>
        private Vector3 previousPosition;

        [InitialState]
        [Animation("Idle")]
        [Transition(typeof(StartMoveInput), typeof(WalkingState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        public class IdleState : State { }

        [Animation("Jumping")]
        [TransitionOnAnimationComplete(typeof(FallingState), 0.15f, true)]
        [Transition(typeof(GroundedEvent), typeof(IdleState))]
        [MovementSettings(SpeedConfig = nameof(walkingSpeed))]
        public class JumpState : State { }

        [Animation("Digging")]
        [DiggingParticlesEnabled]
        [AvatarOffsetAttribute(OffsetValue = nameof(avatarDiggingOffset))]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(StopMoveInput), typeof(IdleState))]
        [Transition(typeof(LeaveGroundEvent), typeof(FallingState))]
        [MovementSettings(SpeedConfig = nameof(walkingSpeed))]
        public class WalkingState : State { }

        [Animation("Walking")]
        [Transition(typeof(JumpEvent), typeof(JumpState))]
        [Transition(typeof(GroundedEvent), typeof(IdleState))]
        [MovementSettings(SpeedConfig = nameof(walkingSpeed))]
        public class FallingState : State { }

        /// <summary>
        /// Update the grounded state of the kinematic character controller.
        /// </summary>
        public void UpdateGroundedState()
        {
            if (movementEngine.GroundedState.Falling)
            {
                RaiseEvent(LeaveGroundEvent.Instance);
            }
            else if (movementEngine.GroundedState.StandingOnGround)
            {
                RaiseEvent(GroundedEvent.Instance);
            }
        }

        public void Awake()
        {
            jumpAction = new JumpAction()
            {
                jumpInput = new Input.BufferedInput()
                {
                    inputActionReference = jumpActionReference,
                    cooldown = 0.25f,
                    bufferTime = 0.05f,
                },
                jumpVelocity = jumpVelocity,
                maxJumpAngle = 180.0f,
                jumpAngleWeightFactor = 0.5f,
            };
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
            SetupInputs();
        }

        /// <summary>
        /// Setup inputs for the KCC
        /// </summary>
        public void SetupInputs()
        {
            if (IsOwner)
            {
                jumpAction?.Setup(movementEngine.GroundedState, movementEngine, this);
                MoveAction?.Enable();
            }
        }

        public override void LateUpdate()
        {
            relativeParentConfig.FollowGround(transform);
            transform.position += movementEngine.ColliderCast.PushOutOverlapping(transform.position, transform.rotation, movementEngine.MaxPushSpeed * unityService.deltaTime);

            base.LateUpdate();
        }

        /// <summary>
        /// The the player's desired velocity for their current input value.
        /// </summary>
        /// <returns>Vector of player velocity based on input movement rotated by player view and projected onto the
        /// ground.</returns>
        public Vector3 GetDesiredMovement()
        {
            // If the player is not on the ground, simply have them move
            // along the plane perpendicular to gravity.
            Vector3 planeNormal = movementEngine.Up;

            // Compare the direction the player is looking with
            // the surface the mole is standing on.
            IManagedCamera camera = GetComponent<IManagedCamera>();
            float yaw = camera.Yaw;

            float dot = Vector3.Dot(Vector3.Project(planeNormal, Vector3.up), Vector3.up);
            bool facingUp = dot > 0 || Mathf.Approximately(dot, 0);

            if (previousFacingUp != facingUp)
            {
                float angle = Vector3.Angle(planeNormal, Vector3.up);
                if (Mathf.Abs(angle % 90) < upFlipThreshold)
                {
                    facingUp = previousFacingUp;
                }
            }

            previousFacingUp = facingUp;
            Vector3 worldUp = facingUp ? Vector3.up : Vector3.down;

            if (Mathf.Approximately(dot, -1))
            {
                yaw *= -1;
            }

            var planeRotation = Quaternion.FromToRotation(worldUp, planeNormal);
            var playerRotation = Quaternion.AngleAxis(yaw, worldUp);
            Vector3 movementForward = planeRotation * playerRotation * Vector3.forward;

            Debug.DrawRay(transform.position, planeNormal, Color.green, 0);
            Debug.DrawRay(transform.position, movementForward, Color.blue, 0);

            // Apply the player's two axis movement to the movement input
            var movementRotation = Quaternion.LookRotation(movementForward, planeNormal);
            Vector3 axisMovement = movementRotation * InputMovement;

            // Scale movement by speed
            float speed = MovementSettingsAttribute.GetSpeed(CurrentState, this);
            return speed * axisMovement;
        }

        public override void FixedUpdate()
        {
            GetComponent<Rigidbody>().isKinematic = true;
            if (IsOwner)
            {
                jumpAction.ApplyJumpIfPossible(movementEngine.GroundedState);
                movementEngine.MovePlayer(
                    GetDesiredMovement() * unityService.fixedDeltaTime,
                    Velocity * unityService.fixedDeltaTime);
                UpdateGroundedState();

                // Apply gravity if needed
                if (movementEngine.GroundedState.Falling || movementEngine.GroundedState.Sliding)
                {
                    Velocity += Physics.gravity * unityService.fixedDeltaTime;
                }
                else if (movementEngine.GroundedState.StandingOnGround)
                {
                    Velocity = Vector3.zero;
                }

                relativeUp.Value = movementEngine.Up;
            }

            if (movementEngine.GroundedState.StandingOnGround)
            {
                // Set the player's rotation to follow the ground
                var rotation = Quaternion.FromToRotation(Vector3.up, relativeUp.Value);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, 10 * unityService.deltaTime);
            }
            else
            {
                // or Velocity if not grounded
                transform.rotation = Quaternion.Lerp(
                    transform.rotation,
                    Quaternion.FromToRotation(Vector3.up, (transform.position - previousPosition).normalized),
                    10 * unityService.deltaTime);
            }

            previousPosition = transform.position;
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

            if (!IsOwner)
            {
                // movementEngine.SetNormal(relativeUp.Value);
                movementEngine.CheckGrounded(false);
            }

            bool particlesEnabled = Attribute.GetCustomAttribute(CurrentState, typeof(DiggingParticlesEnabled)) != null;
            MoleParticles digging = GetComponent<MoleParticles>();
            digging.DrawParticles = particlesEnabled;

            if (Attribute.GetCustomAttribute(CurrentState, typeof(AvatarOffsetAttribute)) is AvatarOffsetAttribute avatarOffset)
            {
                AttachedAnimator.transform.localPosition = (Vector3)this.EvaluateMember(avatarOffset.OffsetValue);
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
                Velocity = velocity + movementEngine.GetGroundVelocity();
                RaiseEvent(JumpEvent.Instance);
            }
        }

        /// <summary>
        /// Read the current player input values.
        /// </summary>
        public void ReadPlayerMovement()
        {
            bool denyMovement = PlayerInputUtils.playerMovementState == PlayerInputState.Deny;
            Vector2 moveVector = denyMovement ? Vector2.zero : MoveAction?.ReadValue<Vector2>() ?? Vector2.zero;
            InputMovement = new Vector3(moveVector.x, 0, moveVector.y);
            jumpAction.Update();

            bool moving = InputMovement.magnitude >= KCCUtils.Epsilon;
            IEvent moveEvent = moving ? StartMoveInput.Instance as IEvent : StopMoveInput.Instance as IEvent;
            RaiseEvent(moveEvent);
        }
    }
}
