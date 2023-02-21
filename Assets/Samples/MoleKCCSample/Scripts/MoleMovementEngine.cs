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
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.MoleKCCSample
{
    /// <summary>
    /// MovementEngine for the mole character to allow the player to
    /// walk up and down walls directly.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MoleMovementEngine : KCCMovementEngine
    {
        /// <summary>
        /// Have no angle decay in movement up walls.
        /// </summary>
        public override float AnglePower => 1.0f;

        /// <summary>
        /// Surface normal for overriding up direction.
        /// </summary>
        protected Vector3 groundNormal = Vector3.up;

        /// <summary>
        /// Should the grounded angle be overwritten.
        /// </summary>
        protected bool overrideGrounded = false;

        /// <summary>
        /// Max speed that the player can snap down at.
        /// </summary>
        public override float MaxSnapDownSpeed => Mathf.Infinity;

        /// <summary>
        /// Set the normal vector for the ground the mole is standing on.
        /// </summary>
        /// <param name="normal"></param>
        public void SetNormal(Vector3 normal)
        {
            groundNormal = normal;
            overrideGrounded = true;
        }

        /// <summary>
        /// Override the up direction for the mole.
        /// </summary>
        public override Vector3 Up => groundNormal;

        /// <inheritdoc/>
        protected override IEnumerable<KCCBounce> GetMovement(Vector3 movement)
        {
            foreach (KCCBounce bounce in base.GetMovement(movement))
            {
                if (bounce.action == KCCUtils.MovementAction.Bounce)
                {
                    if (bounce.hit == null)
                    {
                        yield return bounce;
                        continue;
                    }

                    // If we bounce off a wall perpendicular to the current surface
                    Vector3 normal = bounce.hit.normal;
                    float remainingSpeed = bounce.initialMomentum.magnitude - bounce.Movement.magnitude;
                    if (normal == Vector3.zero)
                    {
                        normal = groundNormal;
                    }

                    // Rotate the remaining movement
                    bounce.remainingMomentum = Quaternion.LookRotation(bounce.hit.normal) *
                        bounce.initialMomentum.normalized * remainingSpeed;

                    SetNormal(normal);
                }

                yield return bounce;
            }

            // Compute the new grounded state
            CheckGrounded(false);
            SetNormal(GroundedState.SurfaceNormal);
        }

        /// <summary>
        /// Snap the player down onto the ground
        /// </summary>
        protected override void SnapPlayerDown()
        {
            transform.position = KCCUtils.SnapPlayerDown(
                transform.position,
                transform.rotation,
                -Up,
                SnapDown,
                ColliderCast);
        }

        /// <summary>
        /// Applies player movement based current configuration.
        /// Includes pushing out overlapping objects, updating grounded state, jumping,
        /// moving the player, and updating the grounded state.
        /// </summary>
        /// <param name="moves">Desired player movement in world space.</param>
        public override KCCBounce[] MovePlayer(params Vector3[] moves)
        {
            RelativeParentConfig.FollowGround(transform);
            Vector3 previousVelocity = (transform.position - previousPosition) / unityService.deltaTime;
            worldVelocity.AddSample(previousVelocity);

            Vector3 start = transform.position;

            // Push player out of overlapping objects
            transform.position += ColliderCast.PushOutOverlapping(
                transform.position,
                transform.rotation,
                MaxPushSpeed * unityService.fixedDeltaTime);

            // Allow player to move
            KCCBounce[] bounces = moves.SelectMany(move =>
            {
                KCCBounce[] bounces = GetMovement(move).ToArray();

                // Only snap down if the player was grounded before they started
                // moving and are not currently trying to move upwards.
                if (GroundedState.StandingOnGround && !GroundedState.Sliding && !MovingUp(move))
                {
                    SnapPlayerDown();
                }

                return bounces;
            }).ToArray();

            // Compute player relative movement state based on final pos
            bool snappedUp = bounces.Any(bounce => bounce.action == KCCUtils.MovementAction.SnapUp);
            CheckGrounded(snappedUp);

            Vector3 delta = transform.position - start;
            transform.position += RelativeParentConfig.UpdateMovingGround(start, GroundedState, delta, unityService.fixedDeltaTime);
            RelativeParentConfig.FollowGround(transform);

            previousPosition = transform.position;
            return bounces;
        }

        /// <summary>
        /// Update the current grounded state of this kinematic character controller.
        /// </summary>
        public override KCCGroundedState CheckGrounded(bool snapped)
        {
            bool didHit = ColliderCast.CastSelf(
                transform.position,
                transform.rotation,
                -Up,
                GroundCheckDistance,
                out IRaycastHit hit);

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
