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
        /// Upwards direction for the KCC Movement engine.
        /// </summary>
        public Vector3 Up => Vector3.up;

        /// <summary>
        /// Collider cast for player movement.
        /// </summary>
        public IColliderCast ColliderCast => _colliderCast ??= GetComponent<IColliderCast>();

        /// <summary>
        /// Distance to ground at which player is considered grounded.
        /// </summary>
        public float GroundedDistance => 0.05f;

        /// <summary>
        /// Distance to check player distance to ground.
        /// </summary>
        public float GroundCheckDistance => 0.25f;

        /// <summary>
        /// Maximum angle at which the player can walk (in degrees).
        /// </summary>
        public float MaxWalkAngle => 60f;

        /// <inheritdoc/>
        public int MaxBounces => 5;

        /// <inheritdoc/>
        public float VerticalSnapUp => StepHeight;

        /// <inheritdoc/>
        public float VerticalSnapDown => SnapDown;

        /// <inheritdoc/>
        public float StepUpDepth => 0.1f;

        /// <inheritdoc/>
        public float AnglePower => 0.5f;

        /// <inheritdoc/>
        public bool CanSnapUp => groundedState.OnGround;

        /// <summary>
        /// Unity service for managing calls to static variables in
        /// a testable manner.
        /// </summary>
        protected IUnityService unityService = UnityService.Instance;

        /// <summary>
        /// Step height the player can walk up.
        /// </summary>
        protected float StepHeight => 0.35f;

        /// <summary>
        /// Max push speed of the player in units per second when pushing
        /// out of overlapping objects.
        /// </summary>
        protected float MaxPushSpeed => 100.0f;

        /// <summary>
        /// Snap down distance for player snapping down.
        /// </summary>
        protected float SnapDown => StepHeight * 1.5f;

        /// <summary>
        /// Max default launch velocity for the player from unlabeled
        /// surfaces.
        /// </summary>
        protected float MaxDefaultLaunchVelocity => 5.0f;

        /// <summary>
        /// Relative parent configuration for following the ground.
        /// </summary>
        protected RelativeParentConfig relativeParentConfig;

        /// <summary>
        /// Current grounded state of the character.
        /// </summary>
        public KCCGroundedState groundedState { get; protected set; }

        /// <summary>
        /// Collider cast for player shape.
        /// </summary>
        private IColliderCast _colliderCast;

        /// <summary>
        /// Is the a movement vector is moving in the direction
        /// upwards relative to player direction.
        /// </summary>
        public bool MovingUp(Vector3 move)
        {
            return Vector3.Dot(move, Up) > 0;
        }

        /// <summary>
        /// Snap the player down onto the ground
        /// </summary>
        protected void SnapPlayerDown()
        {
            transform.position = KCCUtils.SnapPlayerDown(
                transform.position,
                transform.rotation,
                -Up,
                SnapDown,
                KCCUtils.Epsilon,
                ColliderCast);
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
        public Vector3 GetGroundVelocity(Vector3 playerVel)
        {
            Vector3 groundVelocity = Vector3.zero;
            IMovingGround movingGround = groundedState.Floor?.GetComponent<IMovingGround>();
            Rigidbody rb = groundedState.Floor?.GetComponent<Rigidbody>();
            if (movingGround != null)
            {
                if (movingGround.AvoidTransferMomentum())
                {
                    return Vector3.zero;
                }

                // Weight movement of ground by ground movement weight
                groundVelocity = movingGround.GetVelocityAtPoint(groundedState.GroundHitPosition);
                float velocityWeight =
                    movingGround.GetMovementWeight(groundedState.GroundHitPosition, groundVelocity);
                float transferWeight =
                    movingGround.GetTransferMomentumWeight(groundedState.GroundHitPosition, groundVelocity);
                groundVelocity *= velocityWeight;
                groundVelocity *= transferWeight;
            }
            else if (rb != null && !rb.isKinematic)
            {
                Vector3 groundVel = rb.GetPointVelocity(groundedState.GroundHitPosition);
                float velocity = Mathf.Min(groundVel.magnitude, MaxDefaultLaunchVelocity);
                groundVelocity = groundVel.normalized * velocity;
            }
            else if (groundedState.StandingOnGround)
            {
                float velocity = Mathf.Min(playerVel.magnitude, MaxDefaultLaunchVelocity);
                groundVelocity = playerVel.normalized * velocity;
            }

            return groundVelocity;
        }

        /// <summary>
        /// Have the player to visually move with the ground.
        /// </summary>
        public void Update()
        {
            relativeParentConfig.FollowGround(transform);
        }

        /// <summary>
        /// Applies player movement based current configuration.
        /// Includes pushing out overlapping objects, updating grounded state, jumping,
        /// moving the player, and updating the grounded state.
        /// </summary>
        /// <param name="move">Desired player movement in world space.</param>
        public virtual IEnumerable<KCCBounce> MovePlayer(Vector3 move)
        {
            relativeParentConfig.FollowGround(transform);
            Vector3 start = transform.position;

            // Push player out of overlapping objects
            transform.position += ColliderCast.PushOutOverlapping(
                transform.position,
                transform.rotation,
                MaxPushSpeed * unityService.fixedDeltaTime);

            // Allow player to move
            KCCBounce[] bounces = GetMovement(move).ToArray();

            // Only snap down if the player was grounded before they started
            // moving and are not currently trying to move upwards.
            if (groundedState.StandingOnGround && !MovingUp(move))
            {
                SnapPlayerDown();
            }

            // Compute player relative movement state based on final pos
            bool snappedUp = bounces.Any(bounce => bounce.action == KCCUtils.MovementAction.SnapUp);
            CheckGrounded(snappedUp);

            Vector3 delta = transform.position - start;
            transform.position += relativeParentConfig.UpdateMovingGround(transform.position, groundedState, delta, unityService.fixedDeltaTime);
            relativeParentConfig.FollowGround(transform);

            return bounces;
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
        /// Get the movement of the player projected onto the plane
        /// they are standing on if they are not falling.
        /// </summary>
        /// <param name="movement">How the player is attempting to move.</param>
        /// <returns>Projected movement onto the plane the player is standing on.</returns>
        public Vector3 GetProjectedMovement(Vector3 movement)
        {
            // If the player is standing on the ground, project their movement onto the ground plane
            // This allows them to walk up gradual slopes without facing a hit in movement speed
            if (groundedState.StandingOnGround && !groundedState.Sliding)
            {
                Vector3 projectedMovement = Vector3.ProjectOnPlane(movement, groundedState.SurfaceNormal).normalized * movement.magnitude;
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
        protected KCCGroundedState CheckGrounded(bool snappedUp)
        {
            bool didHit = ColliderCast.CastSelf(
                transform.position,
                transform.rotation,
                -Up,
                GroundCheckDistance,
                out IRaycastHit hit);

            Vector3 normal = hit.normal;

            if (snappedUp)
            {
                normal = groundedState.SurfaceNormal;
            }

            groundedState = new KCCGroundedState(
                distanceToGround : hit.distance,
                onGround : didHit,
                angle : Vector3.Angle(normal, Up),
                surfaceNormal : normal,
                groundHitPosition : hit.distance > 0 ? hit.point : groundedState.GroundHitPosition,
                floor : hit.collider?.gameObject,
                groundedDistance : GroundedDistance,
                maxWalkAngle: MaxWalkAngle);

            return groundedState;
        }
    }
}
