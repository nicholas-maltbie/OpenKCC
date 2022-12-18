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
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Data structure describing a bounce of the KCC when moving throughout a scene.
    /// </summary>
    public struct KCCBounce
    {
        /// <summary>
        /// Initial position before moving.
        /// </summary>
        public Vector3 initialPosition;

        /// <summary>
        /// Final position once finishing this bounce.
        /// </summary>
        public Vector3 finalPosition;

        /// <summary>
        /// Initial momentum when starting the move.
        /// </summary>
        public Vector3 initialMomentum;

        /// <summary>
        /// Remaining momentum after this bounce.
        /// </summary>
        public Vector3 remainingMomentum;

        /// <summary>
        /// Action that ocurred during this bounce.
        /// </summary>
        public KCCUtils.MovementAction action;

        /// <summary>
        /// Get the movement of a vector (from initial position to final position).
        /// </summary>
        public Vector3 Movement => finalPosition - initialPosition;

        public override string ToString()
        {
            return string.Join(
                ", ",
                $"{nameof(initialPosition)}:{initialPosition.ToString("F3")}",
                $"{nameof(finalPosition)}:{finalPosition.ToString("F3")}",
                $"{nameof(initialMomentum)}:{initialMomentum.ToString("F3")}",
                $"{nameof(remainingMomentum)}:{remainingMomentum.ToString("F3")}",
                $"{nameof(action)}:{action.ToString()}"
            );
        }
    }

    /// <summary>
    /// Utility class for static functions involving kinematic character controller.
    /// </summary>
    public static class KCCUtils
    {
        /// <summary>
        /// Different actions associated with each update from the KCC Utils.
        /// </summary>
        public enum MovementAction
        {
            Invalid,
            Move,
            Bounce,
            SnapUp,
            Stop,
        }

        /// <summary>
        /// Maximum angle between two colliding objects.
        /// </summary>
        public const float MaxAngleShoveRadians = 90.0f;

        /// <summary>
        /// Epsilon value for spacing out the KCC very small distances.
        /// </summary>
        public const float Epsilon = 0.001f;

        /// <summary>
        /// Snap the player down onto the ground
        /// </summary>
        /// <param name="position">Position of the kcc</param>
        /// <param name="rotation">Rotation of the kcc.</param>
        /// <param name="dir">Direction to snap the kcc down.</param>
        /// <param name="dist">Maximum distance the kcc can snap.</param>
        /// <param name="minSnapThreshold">Minimum snap threshold for snapping down.</param>
        /// <param name="colliderCast">Collider cast component associated with the KCC.</param>
        /// <returns></returns>
        public static Vector3 GetSnapDelta(
            Vector3 position,
            Quaternion rotation,
            Vector3 dir,
            float dist,
            float minSnapThreshold,
            IColliderCast colliderCast)
        {
            bool didHit = colliderCast.CastSelf(
                position + dir * Epsilon,
                rotation,
                dir,
                dist,
                out IRaycastHit hit);

            if (didHit && hit.distance > minSnapThreshold)
            {
                return dir * (hit.distance - Epsilon);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Snap the player down onto the ground
        /// </summary>
        /// <param name="position">Position of the kcc</param>
        /// <param name="rotation">Rotation of the kcc.</param>
        /// <param name="dir">Direction to snap the kcc down.</param>
        /// <param name="dist">Maximum distance the kcc can snap.</param>
        /// <param name="minSnapThreshold">Minimum snap threshold for snapping down.</param>
        /// <param name="colliderCast">Collider cast component associated with the KCC.</param>
        /// <returns></returns>
        public static Vector3 SnapPlayerDown(
            Vector3 position,
            Quaternion rotation,
            Vector3 dir,
            float dist,
            float minSnapThreshold,
            IColliderCast colliderCast)
        {
            return position + GetSnapDelta(position, rotation, dir, dist, minSnapThreshold, colliderCast);
        }

        /// <summary>
        /// Attempt to snap the player up some distance. This will check if there
        /// is available space on the ledge above the point that the player collided with.
        /// If there is space, the player will be teleported up some distance. If
        /// there is not enough space on the ledge above, then this will move the player back to where
        /// they were before the attempt was made.
        /// </summary>
        /// <param name="distanceToSnap">Distance that the player is teleported up.</param>
        /// <param name="momentum">The remaining momentum of the player.</param>
        /// <param name="rotation">Rotation of the player.</param>
        /// <param name="position">Position of the player.</param>
        /// <param name="config">Config for controlling player movement.</param>
        /// <returns>True if the player had space on the ledge and was able to move, false if
        /// there was not enough room the player is moved back to their original position</returns>
        public static bool AttemptSnapUp(
            float distanceToSnap,
            Vector3 momentum,
            Quaternion rotation,
            ref Vector3 position,
            IKCCConfig config)
        {
            // If we were to snap the player up and they moved forward, would they hit something?
            Vector3 snapPos = position + distanceToSnap * config.Up;
            bool didSnapHit = config.ColliderCast
                .CastSelf(
                    snapPos,
                    rotation,
                    momentum.normalized,
                    config.StepUpDepth + Epsilon,
                    out IRaycastHit snapHit);

            // If they can move without instantly hitting something, then snap them up
            if (!didSnapHit || (snapHit.distance >= Epsilon && snapHit.distance > config.StepUpDepth))
            {
                position = snapPos;
                return true;
            }
            // Otherwise move the player back down
            return false;
        }

        /// <summary>
        /// Attempt to snap up at current height or maximum height of the player's snap up height.
        /// </summary>
        /// <param name="colliderCast">Collider cast associated with the player.</param>
        /// <param name="hit">Hit event where player collided with the step.</param>
        /// <param name="up">Up direction for the player.</param>
        /// <param name="momentum">Player's remaining movement.</param>
        /// <param name="position">Player's current position, modify if the player moves while walking up the step.</param>
        /// <param name="rotation">Player's current rotation.</param>
        /// <param name="verticalSnapUp">Maximum distance player can snap up.</param>
        /// <param name="stepUpDepth">Minimum depth required to have the player step forward.</param>
        /// <returns>True if the player snapped up, false otherwise.</returns>
        public static bool AttemptSnapUp(
            IRaycastHit hit,
            Vector3 momentum,
            ref Vector3 position,
            Quaternion rotation,
            IKCCConfig config)
        {
            // Snap character vertically up if they hit something
            //  close enough to their feet
            Vector3 bottom = config.ColliderCast.GetBottom(position, rotation);
            Vector3 footVector = Vector3.Project(hit.point, config.Up) - Vector3.Project(bottom, config.Up);
            bool isAbove = Vector3.Dot(footVector, config.Up) > 0;
            float distanceToFeet = footVector.magnitude * (isAbove ? 1 : -1);

            bool hitStep = config.ColliderCast.DoRaycastInDirection(
                hit.point - config.Up * Epsilon + hit.normal * Epsilon,
                momentum.normalized,
                momentum.magnitude,
                out IRaycastHit stepHit);
            bool perpendicularStep = hitStep && Vector3.Dot(stepHit.normal, config.Up) <= Epsilon;

            bool snappedUp = false;

            if (hitStep &&
                perpendicularStep &&
                hit.distance > 0 &&
                distanceToFeet < config.VerticalSnapUp &&
                distanceToFeet > 0)
            {
                // Sometimes snapping up the exact distance leads to odd behaviour around steps and walls.
                // It's good to check the maximum and minimum snap distances and take whichever one works.
                // snap them up the minimum vertical distance
                snappedUp = AttemptSnapUp(
                    distanceToFeet + Epsilon,
                    momentum,
                    rotation,
                    ref position,
                    config);

                if (!snappedUp)
                {
                    // If that movement doesn't work, Attempt to snap up the maximum vertical distance
                    snappedUp = AttemptSnapUp(
                        config.VerticalSnapUp,
                        momentum,
                        rotation,
                        ref position,
                        config);
                }
            }

            return snappedUp;
        }

        /// <summary>
        /// Get player's projected movement along a surface.
        /// </summary>
        /// <param name="momentum">Remaining player momentum.</param>
        /// <param name="planeNormal">Plane normal that the player is bouncing off of.</param>
        /// <param name="up">Upwards direction relative to player.</param>
        /// <returns>Remaining momentum of the player.</returns>
        public static Vector3 GetProjectedMomentumSafe(Vector3 momentum, Vector3 planeNormal, Vector3 up)
        {
            Vector3 projectedMomentum = Vector3.ProjectOnPlane(momentum, planeNormal).normalized * momentum.magnitude;

            // If projected momentum is less than original momentum (so if the projection broke due to float
            // operations), then change this to just project along the vertical.
            if (projectedMomentum.magnitude + Epsilon < momentum.magnitude)
            {
                return Vector3.ProjectOnPlane(momentum, up).normalized * momentum.magnitude;
            }

            return projectedMomentum;
        }

        /// <summary>
        /// Compute a single KCC Bounce from a given position.
        /// </summary>
        /// <param name="position">Starting position for bounce.</param>
        /// <param name="remainingMomentum">Remaining momentum of the bounce.</param>
        /// <param name="movement">Initial player input movement.</param>
        /// <param name="rotation">Rotation of the player.</param>
        /// <param name="config">Configuration of movement.</param>
        /// <returns>Returns KCC Bounce information for current movement.</returns>
        public static KCCBounce SingleKCCBounce(
            Vector3 position,
            Vector3 remainingMomentum,
            Vector3 movement,
            Quaternion rotation,
            IKCCConfig config)
        {
            Vector3 initialPosition = position;
            Vector3 initialMomentum = remainingMomentum;

            // Do a cast of the collider to see if an object is hit during this
            // movement bounce
            float distance = remainingMomentum.magnitude;
            if (!config.ColliderCast.CastSelf(position, rotation, remainingMomentum.normalized, distance, out IRaycastHit hit))
            {
                // If there is no hit, move to desired position
                return new KCCBounce
                {
                    initialPosition = initialPosition,
                    finalPosition = remainingMomentum + initialPosition,
                    initialMomentum = initialMomentum,
                    remainingMomentum = Vector3.zero,
                    action = MovementAction.Move,
                };
            }

            // If we are overlapping, just exit
            if (hit.distance == 0)
            {
                return new KCCBounce
                {
                    initialPosition = initialPosition,
                    finalPosition = initialPosition,
                    initialMomentum = initialMomentum,
                    remainingMomentum = Vector3.zero,
                    action = MovementAction.Invalid,
                };
            }

            // Apply some force to the object hit if it is moveable, Apply force on entity hit
            if (config.Push != null &&
                config.Push.CanPushObject(hit.collider))
            {
                config.Push.PushObject(new KinematicCharacterControllerHit(
                    hit.collider, hit.collider?.attachedRigidbody, hit.collider?.gameObject,
                    hit.collider?.transform, hit.point, hit.normal, remainingMomentum.normalized, movement.magnitude
                ));
                // If pushing something, reduce remaining force significantly
                remainingMomentum *= config.PushDecay;
            }

            float fraction = hit.distance / distance;
            // Set the fraction of remaining movement (minus some small value)
            Vector3 deltaBounce = remainingMomentum * fraction;
            deltaBounce = deltaBounce.normalized * Mathf.Max(0, deltaBounce.magnitude - Epsilon);
            position += deltaBounce;

            // Decrease remaining momentum by fraction of movement remaining
            remainingMomentum *= (1 - Mathf.Max(0, deltaBounce.magnitude / distance));

            if (config.CanSnapUp && AttemptSnapUp(hit, remainingMomentum, ref position, rotation, config))
            {
                return new KCCBounce
                {
                    initialPosition = initialPosition,
                    finalPosition = position,
                    initialMomentum = initialMomentum,
                    remainingMomentum = remainingMomentum,
                    action = MovementAction.SnapUp,
                };
            }

            // If we didn't snap up:
            // Only apply angular change if hitting something
            // Get angle between surface normal and remaining movement
            float angleBetween = Vector3.Angle(hit.normal, remainingMomentum) - 90.0f;

            // Normalize angle between to be between 0 and 1
            // 0 means no angle, 1 means 90 degree angle
            angleBetween = Mathf.Min(MaxAngleShoveRadians, Mathf.Abs(angleBetween));
            float normalizedAngle = angleBetween / MaxAngleShoveRadians;

            // Reduce the momentum by the remaining movement that ocurred
            remainingMomentum *= Mathf.Pow(1 - normalizedAngle, config.AnglePower) * 0.9f + 0.1f;
            // Rotate the remaining remaining movement to be projected along the plane 
            // of the surface hit (emulate pushing against the object)
            remainingMomentum = GetProjectedMomentumSafe(remainingMomentum, hit.normal, config.Up);

            return new KCCBounce
            {
                initialPosition = initialPosition,
                finalPosition = position,
                initialMomentum = initialMomentum,
                remainingMomentum = remainingMomentum,
                action = MovementAction.Bounce,
            };
        }

        /// <summary>
        /// Get the bounces for a KCC Utils movement action with a set default behaviour.
        /// </summary>
        /// <param name="position">Position to start player movement from.</param>
        /// <param name="movement">Movement to move the player.</param>
        /// <param name="rotation">Rotation of the player during movement.</param>
        /// <param name="config">Configuration settings for player movement.</param>
        /// <returns>Bounces that the player makes when hitting objects as part of it's movement.</returns>
        public static Vector3 GetMovement(
            Vector3 position,
            Vector3 movement,
            Quaternion rotation,
            IKCCConfig config)
        {
            Vector3 finalPos = position;
            foreach (KCCBounce bounce in GetBounces(position, movement, rotation, config))
            {
                if (bounce.action == MovementAction.Stop)
                {
                    finalPos = bounce.finalPosition;
                }
            }

            return finalPos - position;
        }

        /// <summary>
        /// Get the bounces for a KCC Utils movement action with a set default behaviour.
        /// </summary>
        /// <param name="position">Position to start player movement from.</param>
        /// <param name="movement">Movement to move the player.</param>
        /// <param name="rotation">Rotation of the player during movement.</param>
        /// <param name="config">Configuration settings for player movement.</param>
        /// <returns>Bounces that the player makes when hitting objects as part of it's movement.</returns>
        public static IEnumerable<KCCBounce> GetBounces(
            Vector3 position,
            Vector3 movement,
            Quaternion rotation,
            IKCCConfig config)
        {
            // Save current momentum
            Vector3 momentum = movement;

            // current number of bounces
            int bounces = 0;

            bool didSnapUp = false;

            // Continue computing while there is momentum and bounces remaining
            while (momentum.magnitude >= Epsilon && bounces <= config.MaxBounces)
            {
                KCCBounce bounce = SingleKCCBounce(position, momentum, movement, rotation, config);

                if (bounce.action == MovementAction.Invalid)
                {
                    yield return bounce;
                    break;
                }
                else if (bounce.action == MovementAction.SnapUp)
                {
                    didSnapUp = true;
                }
                else if (Vector3.Dot(bounce.Movement, movement) < 0)
                {
                    break;
                }

                yield return bounce;

                momentum = bounce.remainingMomentum;
                position = bounce.finalPosition;

                // Track number of times the character has bounced
                bounces++;
            }

            if (didSnapUp)
            {
                position = SnapPlayerDown(position, rotation, -config.Up, config.VerticalSnapUp + Epsilon * 2, Epsilon * 2, config.ColliderCast);
            }

            // We're done, player was moved as part of loop
            yield return new KCCBounce
            {
                initialPosition = position,
                finalPosition = position,
                initialMomentum = Vector3.zero,
                remainingMomentum = Vector3.zero,
                action = MovementAction.Stop,
            };
            yield break;
        }

        /// <summary>
        /// Gets the velocity of the ground the player is standing on where the player is currently
        /// </summary>
        /// <returns>The velocity of the ground at the point the player is standing on</returns>
        public static Vector3 GetGroundVelocity(IKCCGrounded groundedState, IKCCConfig kccConfig, Vector3 playerVel)
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
                float velocity = Mathf.Min(groundVel.magnitude, kccConfig.MaxDefaultLaunchVelocity);
                groundVelocity = groundVel.normalized * velocity;
            }
            else if (groundedState.StandingOnGround)
            {
                float velocity = Mathf.Min(playerVel.magnitude, kccConfig.MaxDefaultLaunchVelocity);
                groundVelocity = playerVel.normalized * velocity;
            }

            return groundVelocity;
        }
    }
}
