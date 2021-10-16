using System.Collections.Generic;
using System.Linq;
using nickmaltbie.OpenKCC.Environment;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Environment.Pushable;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Kinematic character controller to move the player character
    /// as a kinematic object
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(CameraController))]
    public class KinematicCharacterController : MonoBehaviour
    {
        /// <summary>
        /// Small offset for computing when player has stopped moving
        /// </summary>
        public const float Epsilon = 0.001f;

        /// <summary>
        /// Maximum angle between two colliding objects
        /// </summary>
        public const float MaxAngleShoveRadians = 90.0f;

        /// <summary>
        /// Player collider for checking collisions.
        /// </summary>
        private CapsuleCollider capsuleCollider;

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

        /// <summary>
        /// Direction of down relative to gravity with a unit length of 1
        /// </summary>
        public Vector3 Down => gravity.normalized;

        /// <summary>
        /// Direction of up relative to gravity with a unit length of 1
        /// </summary>
        public Vector3 Up => -gravity.normalized;

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
        private float jumpAngleWeightFactor = 0.2f;

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
        /// Is the player attempting to jump.
        /// </summary>
        private bool attemptingJump;

        /// <summary>
        /// Is the player sprinting.
        /// </summary>
        private bool isSprinting;

        /// <summary>
        /// How long has the player been falling.
        /// </summary>
        private float elapsedFalling;

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
        /// Is the player attempting to jump this frame.
        /// </summary>
        public bool AttemptingJump => attemptingJump;

        /// <summary>
        /// Get the current player velocity.
        /// </summary>
        public Vector3 Velocity => velocity;

        /// <summary>
        /// How long has the player been falling.
        /// </summary>
        public float FallingTime => elapsedFalling;

        /// <summary>
        /// Rigidbody attached to this character.
        /// </summary>
        private Rigidbody characterRigidbody;

        /// <summary>
        /// Camera controller for controlling player view.
        /// </summary>
        private CameraController cameraController;

        /// <summary>
        /// Rotation of the plane the player is viewing
        /// </summary>
        private Quaternion HorizPlaneView =>
            cameraController != null ? Quaternion.Euler(0, cameraController.Yaw, 0) : Quaternion.identity;

        /// <summary>
        /// Intended direction of movement provided by.
        /// </summary>
        public Vector3 InputMovement => this.inputMovement;

        /// <summary>
        /// Player rotated movement that they intend to move.
        /// </summary>
        public Vector3 RotatedMovement => HorizPlaneView * InputMovement;

        /// <summary>
        /// Get the current object this player is standing on.
        /// </summary>
        public GameObject Floor => this.floor;

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
        /// Was the player grounded the start of previous frame.
        /// </summary>
        private bool previousGrounded;

        /// <summary>
        /// Was the player grounded the start of this frame.
        /// </summary>
        private bool startGrounded;

        /// <summary>
        /// Was the player falling the previous frame.
        /// </summary>
        private bool previousFalling;

        /// <summary>
        /// The velocity of the ground the previous frame
        /// </summary>
        private Vector3 previousGroundVelocity;

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
            attemptingJump && elapsedSinceJump >= jumpCooldown;

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
        /// Is the character collider frozen in place as of now. When frozen, the character collider will
        /// not update in any way.
        /// </summary>
        public bool Frozen { get; set; }

        /// <summary>
        /// Gets transformed parameters describing this capsule collider
        /// </summary>
        public (Vector3, Vector3, float, float) GetParams()
        {
            var center = transform.TransformPoint(capsuleCollider.center);
            var radius = capsuleCollider.radius;
            var height = capsuleCollider.height;

            var bottom = center + Down * (height / 2 - radius);
            var top = center + Up * (height / 2 - radius);
            return (top, bottom, radius, height);
        }

        public void OnDestroy()
        {
            GameObject.Destroy(feetFollowObj);
        }

        public void Start()
        {
            this.cameraController = GetComponent<CameraController>();
            this.characterRigidbody = GetComponent<Rigidbody>();
            this.capsuleCollider = GetComponent<CapsuleCollider>();
            feetFollowObj = new GameObject();
            feetFollowObj.name = "feetFollowObj";
            feetFollowObj.transform.SetParent(transform);
        }

        public void OnEnable()
        {
            this.jumpAction.action.performed += this.OnJump;
            this.moveAction.action.performed += this.OnMove;
            this.sprintAction.action.performed += this.OnSprint;
        }

        public void OnDisable()
        {
            this.jumpAction.action.performed -= this.OnJump;
            this.moveAction.action.performed -= this.OnMove;
            this.sprintAction.action.performed -= this.OnSprint;
        }

        public void FixedUpdate()
        {
            if (this.Frozen)
            {
                // If frozen do not update or change any settings
                return;
            }

            float fixedDeltaTime = Time.fixedDeltaTime;

            LinearVelocity = (transform.position - this.previousPosition) / fixedDeltaTime;
            float deltaAngle = Quaternion.Angle(transform.rotation, this.previousRotation);

            float linearSpeed = LinearVelocity.magnitude;
            float angularSpeed = deltaAngle / fixedDeltaTime;

            this.previousPosition = transform.position;
            this.previousRotation = transform.rotation;

            if (IsProne)
            {
                this.remainingMaxProneTime -= fixedDeltaTime;
                this.remainingMinProneTime -= fixedDeltaTime;
                this.characterRigidbody.isKinematic = false;
                this.characterRigidbody.velocity += this.gravity * fixedDeltaTime;

                CheckGrounded();

                this.feetFollowObj.transform.parent = transform;
                this.feetFollowObj.transform.localPosition = Vector3.zero;
                this.feetFollowObj.transform.localRotation = Quaternion.identity;
                this.distanceToGround = Mathf.Infinity;

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
                this.transform.rotation = Quaternion.identity;
                this.characterRigidbody.isKinematic = true;
                this.characterRigidbody.velocity = Vector3.zero;
                this.characterRigidbody.angularVelocity = Vector3.zero;
                this.startGrounded = StandingOnGround;

                // If player is not allowed to move, stop player movement
                if (PlayerInputManager.playerMovementState == PlayerInputState.Deny)
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
                    this.elapsedFalling = 0.0f;
                }
                else if (Falling)
                {
                    velocity += gravity * fixedDeltaTime;
                    this.elapsedFalling += fixedDeltaTime;
                }

                // Compute player jump if they are attempting to jump
                bool jumped = PlayerJump(fixedDeltaTime);

                Vector3 movement = RotatedMovement * (isSprinting ? sprintSpeed : walkingSpeed);

                // If the player is standing on the ground, project their movement onto the ground plane
                // This allows them to walk up gradual slopes without facing a hit in movement speed
                if (!Falling)
                {
                    Vector3 projectedMovement = Vector3.ProjectOnPlane(movement, surfaceNormal).normalized *
                        movement.magnitude;
                    if (projectedMovement.magnitude + Epsilon >= movement.magnitude)
                    {
                        movement = projectedMovement;
                    }
                }

                // If the player was standing on the ground and is not now, increment velocity by ground
                // velocity if the player did nto jump this frame
                if (!startGrounded && previousGrounded && !previousJumped)
                {
                    velocity = previousGroundVelocity;
                }

                // These are broken into two steps so the player's world velocity (usually due to falling)
                //    does not interfere with their ability to walk around according to inputs
                // Move the player according to their movement
                MovePlayer(movement * fixedDeltaTime);
                // Move the player according to their world velocity
                MovePlayer(velocity * fixedDeltaTime);

                // if the player was standing on the ground at the start of the frame and is not 
                //    trying to jump right now, snap them down to the ground
                if (CanSnapDown)
                {
                    SnapPlayerDown();
                }

                CheckGrounded();
                feetFollowObj.transform.position = groundHitPosition;
                footOffset = transform.position - groundHitPosition;

                var currentStanding = GetHits(Down, standingDistance).Select(hit => hit.collider.gameObject).ToList();

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
                previousFalling = Falling;
                previousGrounded = startGrounded;
                previousJumped = jumped;
                previousGroundVelocity = GetGroundVelocity();
                previousStanding = GetHits(Down, groundedDistance).Select(hit => hit.collider.gameObject).ToList();
            }
        }

        /// <summary>
        /// Cast self and get the objects hit that exclude this object.
        /// </summary>
        /// <param name="direction">Direction to cast self collider.</param>
        /// <param name="distance">Distance to cast self collider.</param>
        /// <returns></returns>
        public IEnumerable<RaycastHit> GetHits(Vector3 direction, float distance)
        {
            (var top, var bottom, var radius, _) = GetParams();
            return Physics.CapsuleCastAll(top, bottom, radius, direction, distance, ~0, QueryTriggerInteraction.Ignore)
                .Where(hit => hit.collider.transform != transform);
        }

        /// <summary>
        /// Cast self in a given direction and get the first object hit.
        /// </summary>
        /// <param name="direction">Direction of the raycast.</param>
        /// <param name="distance">Maximum distance of raycast.</param>
        /// <param name="hit">First object hit and related information, will have a distance of Mathf.Infinity if none
        /// is found.</param>
        /// <returns>True if an object is hit within distance, false otherwise.</returns>
        public bool CastSelf(Vector3 direction, float distance, out RaycastHit hit)
        {
            RaycastHit closest = new RaycastHit() { distance = Mathf.Infinity };
            bool hitSomething = false;
            foreach (RaycastHit objHit in GetHits(direction, distance))
            {
                if (objHit.collider.gameObject.transform != gameObject.transform)
                {
                    if (objHit.distance < closest.distance)
                    {
                        closest = objHit;
                    }
                    hitSomething = true;
                }
            }

            hit = closest;
            return hitSomething;
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
                velocity = GetGroundVelocity() + this.jumpVelocity * jumpDirection;
                elapsedSinceJump = 0.0f;
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
            SnapPlayerDown(-surfaceNormal, verticalSnapDown);

            CheckGrounded();

            movingGroundDisplacement = transform.position - moveWithGroundStart;
        }

        /// <summary>
        /// Snap the player down onto the ground
        /// </summary>
        public void SnapPlayerDown()
        {
            SnapPlayerDown(Down, verticalSnapDown);
        }

        /// <summary>
        /// Snap the player down onto the ground
        /// </summary>
        public void SnapPlayerDown(Vector3 dir, float dist)
        {
            bool didHit = CastSelf(dir, dist, out var hit);

            if (didHit && hit.distance > Epsilon)
            {
                transform.position += dir * (hit.distance - Epsilon * 2);
            }
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
            return PushOutOverlapping(maxPushSpeed * fixedDeltaTime);
        }

        /// <summary>
        /// Push a player out of overlappin objects with a max distance.
        /// </summary>
        /// <param name="maxDistance">Maximum distance the player can be pushed</param>
        /// <returns>Total distance player was pushed.</returns>
        public Vector3 PushOutOverlapping(float maxDistance)
        {
            Vector3 pushed = Vector3.zero;
            foreach (Collider overlap in this.GetOverlapping())
            {
                Physics.ComputePenetration(
                    capsuleCollider, transform.position, transform.rotation,
                    overlap, overlap.gameObject.transform.position, overlap.gameObject.transform.rotation,
                    out Vector3 direction, out float distance
                );
                float distPush = Mathf.Min(maxDistance, distance + Epsilon);
                Vector3 push = direction.normalized * distPush;
                transform.position += push;
                pushed += push;
            }
            return pushed;
        }

        public IEnumerable<Collider> GetOverlapping()
        {
            (var top, var bottom, var radius, var height) = GetParams();
            return Physics.OverlapCapsule(top, bottom, radius, ~0, QueryTriggerInteraction.Ignore).Where(c => c.transform != transform);
        }

        /// <summary>
        /// Update the current grounded state of this kinematic character controller
        /// </summary>
        public void CheckGrounded()
        {
            bool didHit = CastSelf(Down, groundCheckDistance, out var hit);
            (var top, var bottom, var radius, var height) = GetParams();

            this.angle = Vector3.Angle(hit.normal, Up);
            this.distanceToGround = hit.distance;
            this.onGround = didHit;
            this.surfaceNormal = hit.normal;
            this.groundHitPosition = hit.distance > 0 ? hit.point : transform.position;
            this.floor = hit.collider != null ? hit.collider.gameObject : null;
        }

        /// <summary>
        /// Attempt to snap the player up some distance. This will check if there
        /// is available space on the ledge above the point that the player collided with.
        /// If there is space, the player will be teleported up some distance. If
        /// there is not enough space on the ledge above, then this will move the player back to where
        /// they were before the attempt was made.
        /// </summary>
        /// <param name="distanceToSnap">Distance that the player is teleported up</param>
        /// <param name="hit">Wall/step that the player ran into</param>
        /// <param name="momentum">The remaining momentum of the player</param>
        /// <returns>True if the player had space on the ledge and was able to move, false if
        /// there was not enough room the player is moved back to their original position</returns>
        public bool AttemptSnapUp(float distanceToSnap, RaycastHit hit, Vector3 momentum)
        {
            // If we were to snap the player up and they moved forward, would they hit something?
            Vector3 currentPosition = transform.position;
            Vector3 snapUp = distanceToSnap * Up;
            transform.position += snapUp;

            Vector3 directionAfterSnap = Vector3.ProjectOnPlane(Vector3.Project(momentum, -hit.normal), Vector3.up).normalized * momentum.magnitude;
            bool didSnapHit = this.CastSelf(directionAfterSnap.normalized, Mathf.Max(stepUpDepth, momentum.magnitude), out RaycastHit snapHit);

            // If they can move without instantly hitting something, then snap them up
            if ((!Falling || elapsedFalling <= snapBufferTime) && snapHit.distance > Epsilon && (!didSnapHit || snapHit.distance > stepUpDepth))
            {
                // Project rest of movement onto plane perpendicular to gravity
                transform.position = currentPosition;
                transform.position += distanceToSnap * Vector3.up;
                return true;
            }
            else
            {
                // Otherwise move the player back down
                transform.position = currentPosition;
                return false;
            }
        }

        public void MovePlayer(Vector3 movement)
        {
            // Save current momentum
            Vector3 momentum = movement;

            Collider selfCollider = GetComponent<Collider>();
            // current number of bounces
            int bounces = 0;

            // Character ability to push objects
            CharacterPush push = GetComponent<CharacterPush>();

            // Continue computing while there is momentum and bounces remaining
            while (momentum.magnitude > Epsilon && bounces <= maxBounces)
            {
                // Do a cast of the collider to see if an object is hit during this
                // movement bounce
                float distance = momentum.magnitude;
                if (!this.CastSelf(momentum.normalized, distance, out RaycastHit hit))
                {
                    // If there is no hit, move to desired position
                    transform.position += momentum;
                    // Exit as we are done bouncing
                    break;
                }

                // Apply some force to the object hit if it is moveable, Apply force on entity hit
                if (push != null &&
                    hit.collider.attachedRigidbody != null &&
                    !hit.collider.attachedRigidbody.isKinematic &&
                    hit.collider.gameObject.GetComponent<IPushable>() != null)
                {
                    push.PushObject(new KinematicCharacterControllerHit(
                        hit.collider, hit.collider.attachedRigidbody, hit.collider.gameObject,
                        hit.collider.transform, hit.point, hit.normal, momentum.normalized, movement.magnitude
                    ));
                    // If pushing something, reduce remaining force significantly
                    momentum *= pushDecay;
                }

                float fraction = hit.distance / distance;
                // Set the fraction of remaining movement (minus some small value)
                transform.position += momentum * (fraction);
                // Push slightly along normal to stop from getting caught in walls
                transform.position += hit.normal * Epsilon;
                // Decrease remaining momentum by fraction of movement remaining
                momentum *= (1 - fraction);

                // Plane to project rest of movement onto
                Vector3 planeNormal = hit.normal;

                // Snap character vertically up if they hit something
                //  close enough to their feet
                float distanceToFeet = hit.point.y - (transform.position - selfCollider.bounds.extents).y;
                if (hit.distance > 0 && !attemptingJump && distanceToFeet < verticalSnapUp && distanceToFeet > 0)
                {
                    // Sometimes snapping up the exact distance leads to odd behaviour around steps and walls.
                    // It's good to check the maximum and minimum snap distances and take whichever one works.
                    // Attempt to snap up the maximum vertical distance
                    if (!AttemptSnapUp(verticalSnapUp, hit, momentum))
                    {
                        // If that movement doesn't work, snap them up the minimum vertical distance
                        AttemptSnapUp(distanceToFeet + Epsilon * 2, hit, momentum);
                    }
                }
                // Only apply angular change if hitting something
                // Get angle between surface normal and remaining movement
                float angleBetween = Vector3.Angle(hit.normal, momentum) - 90.0f;
                // Normalize angle between to be between 0 and 1
                // 0 means no angle, 1 means 90 degree angle
                angleBetween = Mathf.Min(MaxAngleShoveRadians, Mathf.Abs(angleBetween));
                float normalizedAngle = angleBetween / MaxAngleShoveRadians;
                // Reduce the momentum by the remaining movement that ocurred
                momentum *= Mathf.Pow(1 - normalizedAngle, anglePower) * 0.9f + 0.1f;
                // Rotate the remaining remaining movement to be projected along the plane 
                // of the surface hit (emulate pushing against the object)
                Vector3 projectedMomentum = Vector3.ProjectOnPlane(momentum, planeNormal).normalized * momentum.magnitude;
                if (projectedMomentum.magnitude + Epsilon < momentum.magnitude)
                {
                    momentum = Vector3.ProjectOnPlane(momentum, Up).normalized * momentum.magnitude;
                }
                else
                {
                    momentum = projectedMomentum;
                }

                // Track number of times the character has bounced
                bounces++;
            }
            // We're done, player was moved as part of loop
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
            attemptingJump = context.ReadValueAsButton();
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
