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

using System.Collections.Generic;
using System.Linq;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Movement engine for the kcc state machine
    /// to abstract calls to the KCC Utils
    /// for a basic character controller.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(IColliderCast))]
    public class KCCMovementEngine : MonoBehaviour, IKCCConfig
    {
        /// <summary>
        /// Default value for distance from ground player is considered grounded.
        /// </summary>
        public const float DefaultGroundedDistance = 0.05f;

        /// <summary>
        /// Default value for distance to check for ground below player.
        /// </summary>
        public const float DefaultGroundCheckDistance = 0.25f;

        /// <summary>
        /// Default maximum number of bounces for computing player movement.
        /// </summary>
        public const float DefaultMaxBounces = 5f;

        /// <summary>
        /// Default expected depth of a step for walking up staircases.
        /// </summary>
        public const float DefaultStepUpDepth = 0.1f;

        /// <summary>
        /// Default angle power for bouncing and sliding off surfaces.
        /// </summary>
        public const float DefaultAnglePower = 2.0f;

        /// <summary>
        /// Default max push speed when moving the player.
        /// </summary>
        public const float DefaultMaxPushSpeed = 100.0f;

        /// <summary>
        /// Modifier of step height for snapping the player down.
        /// </summary>
        public const float SnapDownModifier = 2.0f;

        /// <summary>
        /// Max launch speed of player form moving ground.
        /// </summary>
        public const float DefaultMaxLaunchVelocity = 5.0f;

        /// <summary>
        /// Height of a step that the player can climb up.
        /// </summary>
        public float stepHeight = 0.35f;

        /// <summary>
        /// Max angle the player can walk up before slipping.
        /// </summary>
        public float maxWalkAngle = 60.0f;

        /// <summary>
        /// Upwards direction for the KCC Movement engine.
        /// </summary>
        public virtual Vector3 Up => Vector3.up;

        /// <summary>
        /// Collider cast for player movement.
        /// </summary>
        public virtual IColliderCast ColliderCast => _colliderCast ??= GetComponent<IColliderCast>();

        /// <summary>
        /// Distance to ground at which player is considered grounded.
        /// </summary>
        public virtual float GroundedDistance => 0.05f;

        /// <summary>
        /// Distance to check player distance to ground.
        /// </summary>
        public virtual float GroundCheckDistance => 0.25f;

        /// <summary>
        /// Maximum angle at which the player can walk (in degrees).
        /// </summary>
        public virtual float MaxWalkAngle => maxWalkAngle;

        /// <inheritdoc/>
        public virtual int MaxBounces => 5;

        /// <inheritdoc/>
        public virtual float VerticalSnapUp => stepHeight;

        /// <inheritdoc/>
        public virtual float StepUpDepth => 0.1f;

        /// <inheritdoc/>
        public virtual float AnglePower => 2.0f;

        /// <summary>
        /// Max push speed of the player in units per second when pushing
        /// out of overlapping objects.
        /// </summary>
        public float MaxPushSpeed => 100.0f;

        /// <summary>
        /// Layermask for computing player collisions.
        /// </summary>
        public LayerMask layerMask = ~0;

        /// <inheritdoc/>
        public bool CanSnapUp => GroundedState.OnGround;

        /// <summary>
        /// Unity service for managing calls to static variables in
        /// a testable manner.
        /// </summary>
        protected IUnityService unityService = UnityService.Instance;

        /// <summary>
        /// Snap down distance for player snapping down.
        /// </summary>
        public float SnapDown => stepHeight * 2f;

        /// <summary>
        /// Max default launch velocity for the player from unlabeled
        /// surfaces.
        /// </summary>
        public float MaxDefaultLaunchVelocity => 5.0f;

        /// <summary>
        /// Maximum speed at which the player can snap down surfaces.
        /// </summary>
        public float MaxSnapDownSpeed => 5.0f;

        /// <summary>
        /// Relative parent configuration for following the ground.
        /// </summary>
        public RelativeParentConfig RelativeParentConfig { get; protected set; } = new RelativeParentConfig();

        /// <summary>
        /// Current grounded state of the character.
        /// </summary>
        public KCCGroundedState GroundedState { get; protected set; }

        /// <inheritdoc/>
        public LayerMask LayerMask => layerMask;

        /// <summary>
        /// Collider cast for player shape.
        /// </summary>
        public IColliderCast _colliderCast;

        /// <summary>
        /// Previous position of the player for calculating moving ground position.
        /// </summary>
        protected Vector3 previousPosition = Vector3.zero;

        /// <summary>
        /// Approximated world velocity of the player.
        /// </summary>
        public SmoothedVector worldVelocity = new SmoothedVector(10);

        /// <summary>
        /// Is the a movement vector is moving in the direction
        /// upwards relative to player direction.
        /// </summary>
        public bool MovingUp(Vector3 move)
        {
            return Vector3.Dot(move, Up) > 0;
        }

        public void Awake()
        {
            previousPosition = transform.position;
        }

        /// <summary>
        /// Snap the player down onto the ground
        /// </summary>
        protected void SnapPlayerDown()
        {
            Vector3 delta = KCCUtils.GetSnapDelta(
                transform.position,
                transform.rotation,
                -Up,
                SnapDown,
                ColliderCast,
                layerMask);
            transform.position += Vector3.ClampMagnitude(delta, MaxSnapDownSpeed * unityService.fixedDeltaTime);
        }

        /// <summary>
        /// Get the bounces for a KCC Utils movement action with a set default behaviour.
        /// </summary>
        /// <param name="movement">Movement to move the player.</param>
        /// <returns>Bounces that the player makes when hitting objects as part of it's movement.</returns>
        protected virtual IEnumerable<KCCBounce> GetMovement(Vector3 movement)
        {
            foreach (KCCBounce bounce in KCCUtils.GetBounces(transform.position, movement, transform.rotation, this))
            {
                if (bounce.action == KCCUtils.MovementAction.Stop)
                {
                    transform.position = bounce.finalPosition;
                }

                yield return bounce;
            }
        }

        /// <summary>
        /// Gets the velocity of the ground the player is standing on where the player is currently
        /// </summary>
        /// <returns>The velocity of the ground at the point the player is standing on</returns>
        public Vector3 GetGroundVelocity()
        {
            Vector3 groundVelocity = Vector3.zero;
            IMovingGround movingGround = GroundedState.Floor?.GetComponent<IMovingGround>();
            Rigidbody rb = GroundedState.Floor?.GetComponent<Rigidbody>();
            if (movingGround != null)
            {
                if (movingGround.AvoidTransferMomentum())
                {
                    return Vector3.zero;
                }

                // Weight movement of ground by ground movement weight
                groundVelocity = movingGround.GetVelocityAtPoint(GroundedState.GroundHitPosition);
                float velocityWeight =
                    movingGround.GetMovementWeight(GroundedState.GroundHitPosition, groundVelocity);
                float transferWeight =
                    movingGround.GetTransferMomentumWeight(GroundedState.GroundHitPosition, groundVelocity);
                groundVelocity *= velocityWeight;
                groundVelocity *= transferWeight;
            }
            else if (rb != null && !rb.isKinematic)
            {
                Vector3 groundVel = rb.GetPointVelocity(GroundedState.GroundHitPosition);
                float velocity = Mathf.Min(groundVel.magnitude, MaxDefaultLaunchVelocity);
                groundVelocity = groundVel.normalized * velocity;
            }
            else if (GroundedState.StandingOnGround)
            {
                Vector3 avgVel = worldVelocity.Average();
                float velocity = Mathf.Min(avgVel.magnitude, MaxDefaultLaunchVelocity);
                groundVelocity = avgVel.normalized * velocity;
            }

            return groundVelocity;
        }

        /// <summary>
        /// Have the player to visually move with the ground.
        /// </summary>
        public void Update()
        {
            RelativeParentConfig.FollowGround(transform);
        }

        /// <summary>
        /// Applies player movement based current configuration.
        /// Includes pushing out overlapping objects, updating grounded state, jumping,
        /// moving the player, and updating the grounded state.
        /// </summary>
        /// <param name="moves">Desired player movement in world space.</param>
        public virtual KCCBounce[] MovePlayer(params Vector3[] moves)
        {
            RelativeParentConfig.FollowGround(transform);
            Vector3 previousVelocity = (transform.position - previousPosition) / unityService.deltaTime;
            worldVelocity.AddSample(previousVelocity);

            Vector3 start = transform.position;

            // Push player out of overlapping objects
            transform.position += ColliderCast.PushOutOverlapping(
                transform.position,
                transform.rotation,
                MaxPushSpeed * unityService.fixedDeltaTime,
                layerMask,
                QueryTriggerInteraction.Ignore);

            // Allow player to move
            KCCBounce[] bounces = moves.SelectMany(move =>
            {
                KCCBounce[] bounces = GetMovement(move).ToArray();

                return bounces;
            }).ToArray();

            // Compute player relative movement state based on final pos
            bool snappedDown = false;
            bool snappedUp = bounces.Any(bounce => bounce.action == KCCUtils.MovementAction.SnapUp);

            // Only snap down if the player was grounded before they started
            // moving and are not currently trying to move upwards.
            if (!snappedUp && GroundedState.StandingOnGround && !GroundedState.Sliding && !moves.Any(move => MovingUp(move)))
            {
                snappedDown = true;
                SnapPlayerDown();
            }

            CheckGrounded(snappedUp || snappedDown);

            Vector3 delta = transform.position - start;
            transform.position += RelativeParentConfig.UpdateMovingGround(start, GroundedState, delta, unityService.fixedDeltaTime);
            previousPosition = transform.position;
            return bounces;
        }

        /// <summary>
        /// Teleport player to a given position.
        /// </summary>
        /// <param name="position">Position to teleport player to.</param>
        public void TeleportPlayer(Vector3 position)
        {
            RelativeParentConfig.Reset();
            transform.position = position;

            foreach (IOnPlayerTeleport tele in GetComponents<IOnPlayerTeleport>())
            {
                tele.OnPlayerTeleport(position, transform.rotation);
            }
        }

        /// <summary>
        /// Get the movement of the player projected onto the plane
        /// they are standing on if they are not falling.
        /// </summary>
        /// <param name="movement">How the player is attempting to move.</param>
        /// <returns>Projected movement onto the plane the player is standing on.</returns>
        public Vector3 GetProjectedMovement(Vector3 movement)
        {
            // If the player is standing on the ground, project their movement onto the ground plane
            // This allows them to walk up gradual slopes without facing a hit in movement speed
            if (GroundedState.StandingOnGround && !GroundedState.Sliding)
            {
                Vector3 projectedMovement = Vector3.ProjectOnPlane(movement, GroundedState.SurfaceNormal).normalized * movement.magnitude;
                if (projectedMovement.magnitude + KCCUtils.Epsilon >= movement.magnitude)
                {
                    movement = projectedMovement;
                }
            }

            return movement;
        }

        /// <summary>
        /// Update the current grounded state of this kinematic character controller.
        /// </summary>
        public KCCGroundedState CheckGrounded(bool snapped)
        {
            Vector3 groundCheckPos = transform.position;

            // If snapped up, use the snapped position to check grounded
            if (snapped)
            {
                Vector3 snapDelta = KCCUtils.GetSnapDelta(
                    transform.position,
                    transform.rotation,
                    -Up,
                    SnapDown,
                    ColliderCast,
                    layerMask);

                groundCheckPos += snapDelta;
            }

            bool didHit = ColliderCast.CastSelf(
                groundCheckPos,
                transform.rotation,
                -Up,
                GroundCheckDistance,
                out IRaycastHit hit,
                layerMask);

            Vector3 normal = hit.normal;

            if (snapped)
            {
                normal = GroundedState.SurfaceNormal;
            }

            GroundedState = new KCCGroundedState(
                distanceToGround: hit.distance,
                onGround: didHit,
                angle: Vector3.Angle(normal, Up),
                surfaceNormal: normal,
                groundHitPosition: hit.distance > 0 ? hit.point : GroundedState.GroundHitPosition,
                floor: hit.collider?.gameObject,
                groundedDistance: GroundedDistance,
                maxWalkAngle: MaxWalkAngle);

            return GroundedState;
        }
    }
}
