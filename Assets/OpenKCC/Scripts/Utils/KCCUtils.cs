
using System.Collections.Generic;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Environment.Pushable;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Utility class for static functions involving kinematic character controller.
    /// </summary>
    public static class KCCUtils
    {
        /// <summary>
        /// Maximum angle between two colliding objects.
        /// </summary>
        public const float MaxAngleShoveRadians = 90.0f;

        /// <summary>
        /// Epsilon value for spacing out the KCC very small distances.
        /// </summary>
        public const float Epsilon = 0.001f;

        /// <summary>
        /// /// Attempt to snap the player up some distance. This will check if there
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
        public static bool AttemptSnapUp(
            float distanceToSnap,
            float stepUpDepth,
            RaycastHit hit,
            Vector3 up,
            Vector3 momentum,
            CapsuleColliderCast capsuleColliderCast,
            ref Vector3 position)
        {
            // If we were to snap the player up and they moved forward, would they hit something?
            Vector3 currentPosition = position;
            Vector3 snapUp = distanceToSnap * up;
            position += snapUp;

            Vector3 directionAfterSnap = Vector3.ProjectOnPlane(Vector3.Project(momentum, -hit.normal), Vector3.up).normalized * momentum.magnitude;
            bool didSnapHit = capsuleColliderCast
                .CastSelf(
                    directionAfterSnap.normalized,
                    Mathf.Max(stepUpDepth, momentum.magnitude),
                    out RaycastHit snapHit);

            // If they can move without instantly hitting something, then snap them up
            if (snapHit.distance > KCCUtils.Epsilon && (!didSnapHit || snapHit.distance > stepUpDepth))
            {
                // Project rest of movement onto plane perpendicular to gravity
                position = currentPosition;
                position += distanceToSnap * Vector3.up;
                return true;
            }
            else
            {
                // Otherwise move the player back down
                position = currentPosition;
                return false;
            }
        }

        public static IEnumerable<Vector3> GetBounces(
            int maxBounces,
            float pushDecay,
            float verticalSnapUp,
            float stepUpDepth,
            float anglePower,
            bool attemptingJump,
            bool canSnapUp,
            Vector3 position,
            Vector3 movement,
            CapsuleColliderCast capsuleColliderCast,
            CharacterPush push,
            Collider selfCollider,
            Transform transform)
        {
            // Save current momentum
            Vector3 momentum = movement;

            // current number of bounces
            int bounces = 0;

            // Continue computing while there is momentum and bounces remaining
            while (momentum.magnitude > Epsilon && bounces <= maxBounces)
            {
                yield return position;

                // Do a cast of the collider to see if an object is hit during this
                // movement bounce
                float distance = momentum.magnitude;
                if (!capsuleColliderCast.CastSelf(momentum.normalized, distance, out RaycastHit hit))
                {
                    // If there is no hit, move to desired position
                    position += momentum;
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
                position += momentum * (fraction);
                // Push slightly along normal to stop from getting caught in walls
                position += hit.normal * Epsilon;
                // Decrease remaining momentum by fraction of movement remaining
                momentum *= (1 - fraction);

                // Plane to project rest of movement onto
                Vector3 planeNormal = hit.normal;

                // Snap character vertically up if they hit something
                //  close enough to their feet
                float distanceToFeet = hit.point.y - (position - selfCollider.bounds.extents).y;
                if (canSnapUp && 
                    hit.distance > 0 &&
                    !attemptingJump &&
                    distanceToFeet < verticalSnapUp &&
                    distanceToFeet > 0)
                {
                    // Sometimes snapping up the exact distance leads to odd behaviour around steps and walls.
                    // It's good to check the maximum and minimum snap distances and take whichever one works.
                    // Attempt to snap up the maximum vertical distance
                    if (!AttemptSnapUp(
                        verticalSnapUp,
                        stepUpDepth,
                        hit,
                        transform.TransformDirection(Vector3.up),
                        momentum,
                        capsuleColliderCast,
                        ref position))
                    {
                        // If that movement doesn't work, snap them up the minimum vertical distance
                        AttemptSnapUp(
                            distanceToFeet + Epsilon * 2, 
                            stepUpDepth,
                            hit,
                            transform.TransformDirection(Vector3.up),
                            momentum,
                            capsuleColliderCast,
                            ref position);
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
                    momentum = Vector3.ProjectOnPlane(momentum, transform.TransformDirection(Vector3.up)).normalized * momentum.magnitude;
                }
                else
                {
                    momentum = projectedMomentum;
                }

                // Track number of times the character has bounced
                bounces++;
            }
            // We're done, player was moved as part of loop

            yield return position;
            yield break;
        }
        
    }
}
