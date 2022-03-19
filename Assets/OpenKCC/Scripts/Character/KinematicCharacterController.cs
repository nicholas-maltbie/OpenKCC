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

using System.Collections.Generic;
using System.Linq;

using nickmaltbie.OpenKCC.Environment;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using static nickmaltbie.OpenKCC.Utils.KCCUtils;

namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Kinematic character controller to move the player character
    /// as a kinematic object
    /// </summary>
    [RequireComponent(typeof(CapsuleColliderCast))]
    [RequireComponent(typeof(Rigidbody))]
    public class KinematicCharacterController : MonoBehaviour
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
        /// Distance to ground at which player is considered grounded
        /// </summary>
        [Tooltip("Distance from ground at which a player is considered standing on the ground")]
        [SerializeField]
        private float groundedDistance = 0.01f;

        /// <summary>
        /// Distance to ground at which player is considered standing on something
        /// </summary>
        [Tooltip("Distance to ground at which player is considered standing on something")]
        [SerializeField]
        private float standingDistance = 0.1f;

        /// <summary>
        /// Distance to check player distance to ground
        /// </summary>
        [Tooltip("Distance to draw rays down when checking if player is grounded")]
        [SerializeField]
        private float groundCheckDistance = 5f;

        /// <summary>
        /// Maximum angle at which the player can walk (in degrees)
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
        /// Maximum number of time player can bounce of walls/floors/objects during an update
        /// </summary>
        [Tooltip("Maximum number of bounces when a player is moving")]
        [SerializeField]
        [Range(1, 10)]
        private int maxBounces = 5;

        /// <summary>
        /// Decay value of momentum when hitting another object.
        /// Should be between [0, 1]
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
        /// Maximum distance the player can be pushed out of overlapping objects in units per second
        /// </summary>
        [Tooltip("Maximum distance a player can be pushed when overlapping other objects in units per second")]
        [SerializeField]
        private float maxPushSpeed = 1.0f;

        /// <summary>
        /// Distance that the character can "snap down" vertical steps
        /// </summary>
        [Tooltip("Snap down distance when snapping onto the floor")]
        [SerializeField]
        private float verticalSnapDown = 0.2f;

        [Header("Stair and Step")]

        /// <summary>
        /// Minimum depth of a stair for a user to climb up
        /// (thinner steps than this value will not let the player climb)
        /// </summary>
        [Tooltip("Minimum depth of stairs when climbing up steps")]
        [SerializeField]
        private float stepUpDepth = 0.1f;

        /// <summary>
        /// Distance that the player can snap up when moving up stairs or vertical steps in terrain
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
        /// Velocity of player jump in units per second
        /// </summary>
        [Tooltip("Vertical velocity of player jump")]
        [SerializeField]
        private float jumpVelocity = 5.0f;

        /// <summary>
        /// Maximum angle at which the player can jump (in degrees)
        /// </summary>
        [Tooltip("Maximum angle at which the player can jump (in degrees)")]
        [SerializeField]
        [Range(0, 90)]
        private float maxJumpAngle = 85f;

        /// <summary>
        /// Weight to which the player's jump is weighted towards the direction
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
        /// Time in seconds that a player can jump after their feet leave the ground
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
        /// Threshold angular velocity in degrees per second for existing prone early
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

        /// <summary>
        /// Player's given input movement for this frame.
        /// </summary>
        private Vector3 inputMovement;

        /// <summary>
        /// Is the player pressing the jump button.
        /// </summary>
        private bool pressedJump;

        /// <summary>
        /// Time since the player started the jump action.
        /// </summary>
        private float jumpBufferRemaining;

        /// <summary>
        /// Is the player sprinting.
        /// </summary>
        private bool isSprinting;

        /// <summary>
        /// /// How long has the player been falling.
        /// </summary>
        private float elapsedFalling;

        /// <summary>
        /// Has the player jumped while sliding?
        /// </summary>
        private bool notSlidingSinceJump = true;

        /// <summary>
        /// Current player velocity
        /// </summary>
        private Vector3 velocity;

        /// <summary>
        /// Current distance the player is from the ground.
        /// </summary>
        private float distanceToGround;

        /// <summary>
        /// Was the player grounded this frame
        /// </summary>
        private bool onGround;

        /// <summary>
        /// Angle between the ground and the player.
        /// </summary>
        private float angle;

        /// <summary>
        /// The surface normal vector of the ground the player is standing on.
        /// </summary>
        private Vector3 surfaceNormal;

        /// <summary>
        /// The point in which the player is hitting the ground.
        /// </summary>
        private Vector3 groundHitPosition;

        /// <summary>
        /// What is the player standing on.
        /// </summary>
        private GameObject floor;

        /// <summary>
        /// Amount of time that has elapsed since the player's last jump action.
        /// </summary>
        private float elapsedSinceJump;

        /// <summary>
        /// How much longer will the player be prone (lower bound).
        /// </summary>
        private float remainingMinProneTime;

        /// <summary>
        /// How much longer will the player be prone (upper bound).
        /// </summary>
        private float remainingMaxProneTime;

        /// <summary>
        /// Elapsed time player has been standing/lying still under threshold velocity.
        /// </summary>
        private float elapsedUnderThreshold;

        /// <summary>
        /// Did the player snap up steps or edges the previous frame
        /// </summary>
        private bool snapUpPrevious;

        /// <summary>
        /// Rigidbody attached to this character.
        /// </summary>
        private Rigidbody characterRigidbody;

        /// <summary>
        /// Camera controller for controlling player view.
        /// </summary>
        private ICameraControls cameraControls;

        /// <summary>
        /// Was the player grounded the start of previous frame.
        /// </summary>
        private bool previousGrounded;

        /// <summary>
        /// Was the player grounded the start of this frame.
        /// </summary>
        private bool startGrounded;

        /// <summary>
        /// The velocity of the ground the previous frame
        /// </summary>
        private Vector3 previousGroundVelocity;

        /// <summary>
        /// Player collider for checking collisions.
        /// </summary>
        public CapsuleColliderCast capsuleColliderCast { get; private set; }

        /// <summary>
        /// Push action associated with this kcc.
        /// </summary>
        public CharacterPush characterPush { get; private set; }

        /// <summary>
        /// Rotation of the plane the player is viewing
        /// </summary>
        private Quaternion HorizPlaneView =>
            cameraControls != null ?
                Quaternion.Euler(0, cameraControls.Yaw, 0) : Quaternion.Euler(0, transform.eulerAngles.y, 0);

        /// <summary>
        /// Previous objects that the player is standing on.
        /// </summary>
        private List<GameObject> previousStanding = new List<GameObject>();

        /// <summary>
        /// Object for feet to follow.
        /// </summary>
        private GameObject feetFollowObj;

        /// <summary>
        /// Offset of feet from the center of the character.
        /// </summary>
        private Vector3 footOffset;

        /// <summary>
        /// Previous position of the player in world space.
        /// </summary>
        private Vector3 previousPosition;

        /// <summary>
        /// Previous rotation of the character.
        /// </summary>
        private Quaternion previousRotation;

        /// <summary>
        /// Did the player jump the previous frame?
        /// </summary>
        private bool previousJumped;

        /// <summary>
        /// Player's displacement caused by the moving floor this update.
        /// </summary>
        private Vector3 movingGroundDisplacement;

        /// <summary>
        /// linear velocity of the player measured via displacement from the previous movement.
        /// </summary>
        public Vector3 LinearVelocity { get; private set; }

        /// <summary>
        /// Can the player jump right now.
        /// </summary>
        public bool CanJump => elapsedFalling >= 0 && (!FallingAngle(maxJumpAngle) || elapsedFalling <= coyoteTime) &&
            AttemptingJump && elapsedSinceJump >= jumpCooldown && (!Falling || notSlidingSinceJump);

        /// <summary>
        /// Can a player snap down this frame, a player is only allowed to snap down
        /// if they were standing on the ground this frame or was not falling within a given buffer time.
        /// Additionally, a player must have not jumped within a small buffer time in order to
        /// attempt the action of snapping down. This stops the player from teleporting into the ground right
        /// as they start to jump.
        /// </summary>
        /// <returns>If a player is allowed to snap down</returns>
        public bool CanSnapDown => (StandingOnGround || elapsedFalling <= snapBufferTime) && (elapsedSinceJump >= snapBufferTime);

        /// <summary>
        /// Is the player attempting to jump this frame. True if they pressed jump or if the jump buffer time remaining
        /// is greater than zero.
        /// </summary>
        public bool AttemptingJump => pressedJump || jumpBufferRemaining > 0;

        /// <summary>
        /// Get the current player velocity.
        /// </summary>
        public Vector3 Velocity => velocity;

        /// <summary>
        /// How long has the player been falling.
        /// </summary>
        public float FallingTime => elapsedFalling;

        /// <summary>
        /// Can the player snap up this update?
        /// </summary>
        public bool CanSnapUp => snapUpPrevious || (!Falling || elapsedFalling <= snapBufferTime);

        /// <summary>
        /// Direction of down relative to gravity with a unit length of 1
        /// </summary>
        public Vector3 Down => gravity.normalized;

        /// <summary>
        /// Direction of up relative to gravity with a unit length of 1
        /// </summary>
        public Vector3 Up => -gravity.normalized;

        /// <summary>
        /// Intended direction of movement provided by.
        /// </summary>
        public Vector3 InputMovement => inputMovement;

        /// <summary>
        /// Player rotated movement that they intend to move.
        /// </summary>
        /// <param name="inputMovement">Input movement vector of the player</param>
        public Vector3 RotatedMovement(Vector3 inputMovement) => HorizPlaneView * inputMovement;

        /// <summary>
        /// Get the current object this player is standing on.
        /// </summary>
        public GameObject Floor => floor;

        /// <summary>
        /// Is the player currently prone?
        /// </summary>
        public bool IsProne => remainingMaxProneTime > 0;

        /// <summary>
        /// Is the player currently standing on the ground?
        /// Will be true if the hit the ground and the distance to the ground is less than
        /// the grounded threshold. NOTE, will be false if the player is overlapping with the
        /// ground or another object as it is difficult to tell whether they are stuck in a wall
        /// (and would therefore not be on the ground) versus when they are stuck in the floor.
        /// </summary>
        public bool StandingOnGround => onGround && distanceToGround <= groundedDistance && distanceToGround > 0;

        /// <summary>
        /// Is the player currently falling? this is true if they are either not standing on 
        /// the ground or if the angle between them and the ground is grater than the player's
        /// ability to walk.
        /// </summary>
        public bool Falling => FallingAngle(maxWalkAngle);

        /// <summary>
        /// Check if a player is falling for a given max walk angle.
        /// </summary>
        /// <param name="maxAngle">Maximum walk angle for the player.</param>
        /// <returns>True if the player is slipping/falling on the slope they are currently standing on.</returns>
        public bool FallingAngle(float maxAngle) => !StandingOnGround || angle > maxAngle;

        /// <summary>
        /// Maximum number of time player can bounce of walls/floors/objects during an update
        /// </summary>
        public int MaxBounces => maxBounces;

        /// <summary>
        /// Decay value of momentum when hitting another object.
        /// Should be between [0, 1]
        /// </summary>
        public float PushDecay => pushDecay;

        /// <summary>
        /// Distance that the player can snap up when moving up stairs or vertical steps in terrain
        /// </summary>
        public float VerticalSnapUp => verticalSnapUp;

        /// <summary>
        /// Minimum depth of a stair for a user to climb up
        /// (thinner steps than this value will not let the player climb)
        /// </summary>
        public float StepUpDepth => stepUpDepth;

        /// <summary>
        /// Decrease in momentum factor due to angle change when walking.
        /// Should be a positive float value. It's an exponential applied to 
        /// values between [0, 1] so values smaller than 1 create a positive
        /// curve and grater than 1 for a negative curve.
        /// </summary>
        public float AnglePower => anglePower;

        /// <summary>
        /// Is the character collider frozen in place as of now. When frozen, the character collider will
        /// not update in any way.
        /// </summary>
        public bool Frozen { get; set; }

        /// <summary>
        /// Cleanup attached foot object when this is destroyed.
        /// </summary>
        public void OnDestroy()
        {
            GameObject.Destroy(feetFollowObj);
        }

        /// <summary>
        /// Setup KCC components on start.
        /// </summary>
        public void Start()
        {
            this.cameraControls = GetComponent<ICameraControls>();
            this.characterRigidbody = GetComponent<Rigidbody>();
            this.capsuleColliderCast = GetComponent<CapsuleColliderCast>();
            this.feetFollowObj = new GameObject();
            this.feetFollowObj.name = "feetFollowObj";
            this.feetFollowObj.transform.SetParent(transform);
        }

        /// <summary>
        /// When enabled, ensure actions are configured properly.
        /// </summary>
        public void OnEnable()
        {
            jumpAction.action.performed += OnJump;
            moveAction.action.performed += OnMove;
            sprintAction.action.performed += OnSprint;
        }

        /// <summary>
        /// On disable, detach any connected actions.
        /// </summary>
        public void OnDisable()
        {
            jumpAction.action.performed -= OnJump;
            moveAction.action.performed -= OnMove;
            sprintAction.action.performed -= OnSprint;
        }

        /// <summary>
        /// Fixed update to move the player based on player input.
        /// </summary>
        public void FixedUpdate()
        {
            if (Frozen)
            {
                // If frozen do not update or change any settings
                return;
            }

            float fixedDeltaTime = Time.fixedDeltaTime;

            LinearVelocity = (transform.position - previousPosition) / fixedDeltaTime;
            float deltaAngle = Quaternion.Angle(transform.rotation, previousRotation);

            float linearSpeed = LinearVelocity.magnitude;
            float angularSpeed = deltaAngle / fixedDeltaTime;

            previousPosition = transform.position;
            previousRotation = transform.rotation;

            if (IsProne)
            {
                remainingMaxProneTime -= fixedDeltaTime;
                remainingMinProneTime -= fixedDeltaTime;
                characterRigidbody.isKinematic = false;
                characterRigidbody.velocity += gravity * fixedDeltaTime;

                CheckGrounded();

                feetFollowObj.transform.parent = transform;
                feetFollowObj.transform.localPosition = Vector3.zero;
                feetFollowObj.transform.localRotation = Quaternion.identity;
                distanceToGround = Mathf.Infinity;

                // Exclude the floor's velocity from the linear speed calculation for checking prone
                float movingSpeedCalculation = Mathf.Max(
                    0,
                    linearSpeed - (movingGroundDisplacement / fixedDeltaTime).magnitude);

                // If player's velocity is less than threshold for threshold time, exit prone early
                if (remainingMinProneTime <= 0 &&
                    movingSpeedCalculation <= thresholdVelocity &&
                    angularSpeed <= thresholdAngularVelocity)
                {
                    elapsedUnderThreshold += fixedDeltaTime;
                    if (elapsedUnderThreshold >= earlyStopProneThreshold)
                    {
                        elapsedUnderThreshold = 0;
                        remainingMinProneTime = 0;
                        remainingMaxProneTime = 0;
                    }
                }
                else
                {
                    elapsedUnderThreshold = 0;
                }
            }
            else
            {
                transform.rotation = Quaternion.identity;
                characterRigidbody.isKinematic = true;
                characterRigidbody.velocity = Vector3.zero;
                characterRigidbody.angularVelocity = Vector3.zero;
                startGrounded = StandingOnGround;

                // If player is not allowed to move, stop player movement
                if (PlayerInputUtils.playerMovementState == PlayerInputState.Deny)
                {
                    inputMovement = Vector3.zero;
                }

                // If we are standing on an object marked as a moving platform, move the player
                // with the moving ground object.
                MoveWithGround();

                CheckGrounded();

                if (PushOutOverlapping().magnitude > 0)
                {
                    CheckGrounded();
                }

                // Update player velocity based on grounded state
                if (!Falling)
                {
                    velocity = Vector3.zero;
                    elapsedFalling = 0.0f;
                    notSlidingSinceJump = true;
                }
                else if (Falling)
                {
                    velocity += gravity * fixedDeltaTime;
                    elapsedFalling += fixedDeltaTime;
                }

                // Compute player jump if they are attempting to jump
                bool jumped = PlayerJump(fixedDeltaTime);

                // If the player was standing on the ground and is not now, increment velocity by ground
                // velocity if the player did nto jump this frame
                if (!startGrounded && previousGrounded && !previousJumped)
                {
                    velocity = previousGroundVelocity;
                }

                // These are broken into two steps so the player's world velocity (usually due to falling)
                //    does not interfere with their ability to walk around according to inputs
                // Move the player according to their movement
                bool snapUpMove = MovePlayer(GetProjectedMovement() * fixedDeltaTime);
                // Move the player according to their world velocity
                MovePlayer(velocity * fixedDeltaTime, stopSnapUp: true);

                snapUpPrevious = snapUpMove;

                // if the player was standing on the ground at the start of the frame and is not 
                //    trying to jump right now, snap them down to the ground
                if (CanSnapDown)
                {
                    SnapPlayerDown();
                }

                CheckGrounded();
                feetFollowObj.transform.position = groundHitPosition;
                footOffset = transform.position - groundHitPosition;

                var currentStanding = capsuleColliderCast
                    .GetHits(transform.position, transform.rotation, Down, standingDistance)
                    .Select(hit => hit.collider.gameObject).ToList();

                // Detect if the floor the player is standing on has changed
                // For each object that we are currently standing on that we were not standing on the previous update
                currentStanding.Where(floor => !previousStanding.Contains(floor))
                    .Where(floor => floor != null)
                    .Select(floor => floor.GetComponent<DetectPlayerStand>())
                    .Where(detectStand => detectStand != null)
                    .ToList()
                    .ForEach(detectStand => detectStand.StepOn());

                // For each object that were standing on previously that we are not standing on now
                previousStanding.Where(floor => !currentStanding.Contains(floor))
                    .Where(floor => floor != null)
                    .Select(floor => floor.GetComponent<DetectPlayerStand>())
                    .Where(detectStand => detectStand != null)
                    .ToList()
                    .ForEach(detectStand => detectStand.StepOff());

                // Save state of player
                previousGrounded = startGrounded;
                previousJumped = jumped;
                previousGroundVelocity = GetGroundVelocity();
                previousStanding = capsuleColliderCast
                    .GetHits(transform.position, transform.rotation, Down, groundedDistance)
                    .Select(hit => hit.collider.gameObject).ToList();
            }

            // Decrement the time since the player last jumped (if it's greater than zero)
            if (jumpBufferRemaining > 0)
            {
                jumpBufferRemaining = Mathf.Max(0, jumpBufferRemaining - fixedDeltaTime);
            }
        }

        /// <summary>
        /// Gets the velocity of the ground the player is standing on where the player is currently
        /// </summary>
        /// <returns>The velocity of the ground at the point the player is standong on</returns>
        private Vector3 GetGroundVelocity()
        {
            Vector3 groundVelocity = Vector3.zero;
            IMovingGround movingGround = floor == null ? null : floor.GetComponent<IMovingGround>();
            if (movingGround != null && !movingGround.AvoidTransferMomentum())
            {
                // Weight movement of ground by ground movement weight
                float velocityWeight =
                    movingGround.GetMovementWeight(groundHitPosition, LinearVelocity, Time.fixedDeltaTime);
                float transferWeight =
                    movingGround.GetTransferMomentumWeight(groundHitPosition, LinearVelocity, Time.fixedDeltaTime);
                groundVelocity = movingGround.GetVelocityAtPoint(groundHitPosition, Time.fixedDeltaTime);
                groundVelocity *= velocityWeight;
                groundVelocity *= transferWeight;
            }

            return groundVelocity;
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
            Vector3 movement = RotatedMovement(inputMovement) * (isSprinting ? sprintSpeed : walkingSpeed);

            // If the player is standing on the ground, project their movement onto the ground plane
            // This allows them to walk up gradual slopes without facing a hit in movement speed
            if (!Falling)
            {
                Vector3 projectedMovement = Vector3.ProjectOnPlane(movement, surfaceNormal).normalized *
                    movement.magnitude;
                if (projectedMovement.magnitude + KCCUtils.Epsilon >= movement.magnitude)
                {
                    movement = projectedMovement;
                }
            }

            return movement;
        }

        /// <summary>
        /// Give player vertical velocity if they can jump and are attempting to jump.
        /// </summary>
        /// <param name="fixedDeltaTime">Time in an update</param>
        /// <returns>true if the player successfully jumped, false otherwise</returns>
        public bool PlayerJump(float fixedDeltaTime)
        {
            // Give the player some vertical velocity if they are jumping and grounded
            if (CanJump)
            {
                Vector3 jumpDirection = (
                    (StandingOnGround ? surfaceNormal : Up) *
                    jumpAngleWeightFactor + Up * (1 - jumpAngleWeightFactor)
                    ).normalized;
                velocity = GetGroundVelocity() + jumpVelocity * jumpDirection;

                // If the player successfully jumped, reset elapsed since jump and jump buffer
                elapsedSinceJump = 0.0f;
                jumpBufferRemaining = 0;
                notSlidingSinceJump = false;

                return true;
            }
            else
            {
                elapsedSinceJump += fixedDeltaTime;
                return false;
            }
        }

        /// <summary>
        /// Move the player with the ground if possible based on the ground's
        /// velocity at a given point. 
        /// </summary>
        public void MoveWithGround()
        {
            movingGroundDisplacement = Vector3.zero;

            Vector3 moveWithGroundStart = transform.position;
            // Check if we were standing on moving ground the previous frame
            IMovingGround movingGround = floor == null ? null : floor.GetComponent<IMovingGround>();
            if (movingGround == null || !onGround || distanceToGround > groundedDistance)
            {
                // We aren't standing on something, don't do anything
                feetFollowObj.transform.parent = transform;
                feetFollowObj.transform.localPosition = Vector3.zero;
                return;
            }

            // Get the displacement of the floor at the previous position
            Vector3 displacement = movingGround.GetDisplacementAtPoint(groundHitPosition, Time.fixedDeltaTime);
            // Check if we were standing on moving ground the previous frame
            if (feetFollowObj.transform.parent != transform)
            {
                displacement = (feetFollowObj.transform.position + footOffset) - transform.position;
            }

            float weight = movingGround.GetMovementWeight(groundHitPosition, LinearVelocity, Time.fixedDeltaTime);

            // Move player by floor displacement this frame
            transform.position += displacement * weight;

            if (movingGround.ShouldAttach())
            {
                // Attach foot follow object to floor
                feetFollowObj.transform.parent = floor.transform;
            }

            // GetMovementWeight(Vector3 point, Vector3 playerVelocity, float deltaTime)

            transform.position += (verticalSnapDown * 0.5f) * surfaceNormal;
            transform.position = KCCUtils.SnapPlayerDown(
                transform.position,
                transform.rotation,
                -surfaceNormal,
                verticalSnapDown,
                capsuleColliderCast);

            CheckGrounded();

            movingGroundDisplacement = transform.position - moveWithGroundStart;
        }

        /// <summary>
        /// Snap the player down onto the ground
        /// </summary>
        public void SnapPlayerDown()
        {
            transform.position = KCCUtils.SnapPlayerDown(
                transform.position,
                transform.rotation,
                Down,
                verticalSnapDown,
                capsuleColliderCast);
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
            return capsuleColliderCast.PushOutOverlapping(
                transform.position,
                transform.rotation,
                maxPushSpeed * fixedDeltaTime);
        }

        /// <summary>
        /// Update the current grounded state of this kinematic character controller
        /// </summary>
        public void CheckGrounded()
        {
            bool didHit = capsuleColliderCast.CastSelf(
                transform.position,
                transform.rotation,
                Down,
                groundCheckDistance,
                out RaycastHit hit);

            angle = Vector3.Angle(hit.normal, Up);
            distanceToGround = hit.distance;
            onGround = didHit;
            surfaceNormal = hit.normal;
            groundHitPosition = hit.distance > 0 ? hit.point : transform.position;
            floor = hit.collider != null ? hit.collider.gameObject : null;
        }

        /// <summary>
        /// Move the player based on some vector of desired movement.
        /// </summary>
        /// <param name="movement">Movement in world space in which the player model should be moved.</param>
        /// <param name="stopSnapUp">Should snapping up be forcibly stopped.</param>
        public bool MovePlayer(Vector3 movement, bool stopSnapUp = false)
        {
            bool snappedUp = false;
            foreach (KCCBounce bounce in KCCUtils.GetBounces(
                maxBounces,
                pushDecay,
                verticalSnapUp,
                stepUpDepth,
                anglePower,
                AttemptingJump,
                stopSnapUp ? false : CanSnapUp,
                transform.position,
                movement,
                transform.rotation,
                Up,
                capsuleColliderCast,
                GetComponent<CharacterPush>()))
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
        /// Knock a player prone for a given period of time.
        /// </summary>
        /// <param name="minProneTime"></param>
        /// <param name="maxProneTime"></param>
        public void KnockPlayerProne(float minProneTime, float maxProneTime)
        {
            KnockPlayerProne(minProneTime, maxProneTime, Vector3.zero, Vector3.zero);
        }

        /// <summary>
        /// Knock the player prone for a given time and with an initial velocity.
        /// </summary>
        /// <param name="minProneTime">Minimum time for player to be prone.</param>
        /// <param name="maxProneTime">Maximum time for player to be prone.</param>
        /// <param name="linearVelocity">Linear velocity of player when knocked prone.</param>
        /// <param name="angularVelocity">Angular velocity of player when knocked prone.</param>
        public void KnockPlayerProne(
            float minProneTime,
            float maxProneTime,
            Vector3 linearVelocity,
            Vector3 angularVelocity)
        {
            characterRigidbody.isKinematic = false;
            remainingMinProneTime = minProneTime;
            remainingMaxProneTime = maxProneTime;
            characterRigidbody.velocity = linearVelocity;
            characterRigidbody.angularVelocity = angularVelocity;
        }

        /// <summary>
        /// Action for when player attempts to move.
        /// </summary>
        public void OnMove(InputAction.CallbackContext context)
        {
            Vector2 movement = context.ReadValue<Vector2>();
            inputMovement = new Vector3(movement.x, 0, movement.y);
            inputMovement = inputMovement.magnitude > 1 ? inputMovement / inputMovement.magnitude : inputMovement;
        }

        /// <summary>
        /// Action for when player attempts to jump.
        /// </summary>
        public void OnJump(InputAction.CallbackContext context)
        {
            pressedJump = context.ReadValueAsButton();
            // If the player released the button, reset the remaining buffer time.
            if (!pressedJump)
            {
                jumpBufferRemaining = jumpBufferTime;
            }
        }

        /// <summary>
        /// Action for when player attempts to sprint.
        /// </summary>
        public void OnSprint(InputAction.CallbackContext context)
        {
            isSprinting = context.ReadValueAsButton();
        }
    }
}
