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
        /// Different actions associated with each update from the KCC Utils.
        /// </summary>
        public enum MovementAction
        {
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
        /// /// Attempt to snap the player up some distance. This will check if there
        /// is available space on the ledge above the point that the player collided with.
        /// If there is space, the player will be teleported up some distance. If
        /// there is not enough space on the ledge above, then this will move the player back to where
        /// they were before the attempt was made.
        /// </summary>
        /// <param name="distanceToSnap">Distance that the player is teleported up</param>
        /// <param name="hit">Wall/step that the player ran into</param>
        /// <param name="momentum">The remaining momentum of the player</param>
        /// <param name="up">Direction of Up relative to the player</param>
        /// <param name="colliderCast">Collider cast associated with the player</param>
        /// <param name="momentum">reference to moving position of the player</param>
        /// <returns>True if the player had space on the ledge and was able to move, false if
        /// there was not enough room the player is moved back to their original position</returns>
        public static bool AttemptSnapUp(
            float distanceToSnap,
            float stepUpDepth,
            RaycastHit hit,
            Vector3 up,
            Vector3 momentum,
            IColliderCast colliderCast,
            ref Vector3 position)
        {
            // If we were to snap the player up and they moved forward, would they hit something?
            Vector3 currentPosition = position;
            Vector3 snapUp = distanceToSnap * up;
            Vector3 directionAfterSnap = Vector3.ProjectOnPlane(Vector3.Project(momentum, -hit.normal), Vector3.up).normalized * momentum.magnitude;
            bool didSnapHit = colliderCast
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

        public static IEnumerable<(Vector3, Ray, MovementAction)> GetBounces(
            int maxBounces,
            float pushDecay,
            float verticalSnapUp,
            float stepUpDepth,
            float anglePower,
            bool attemptingJump,
            bool canSnapUp,
            Vector3 position,
            Vector3 movement,
            Vector3 up,
            IColliderCast colliderCast,
            CharacterPush push)
        {
            // Save current momentum
            Vector3 momentum = movement;

            // current number of bounces
            int bounces = 0;

            // Continue computing while there is momentum and bounces remaining
            while (momentum.magnitude > Epsilon && bounces <= maxBounces)
            {
                // Position of character at start of bounce
                Vector3 initialPosition = position;

                // Do a cast of the collider to see if an object is hit during this
                // movement bounce
                float distance = momentum.magnitude;
                if (!colliderCast.CastSelf(momentum.normalized, distance, out RaycastHit hit))
                {
                    // If there is no hit, move to desired position
                    position += momentum;
                    yield return (initialPosition, new Ray(position, Vector3.zero), MovementAction.Move);

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
                float distanceToFeet = hit.point.y - (position - colliderCast.GetBounds().extents).y;
                if (canSnapUp && 
                    hit.distance > 0 &&
                    !attemptingJump &&
                    distanceToFeet < verticalSnapUp &&
                    distanceToFeet > 0)
                {
                    // Sometimes snapping up the exact distance leads to odd behaviour around steps and walls.
                    // It's good to check the maximum and minimum snap distances and take whichever one works.
                    // Attempt to snap up the maximum vertical distance
                    bool snappedUp = AttemptSnapUp(
                        verticalSnapUp,
                        stepUpDepth,
                        hit,
                        up,
                        momentum,
                        colliderCast,
                        ref position);

                    if (!snappedUp)
                    {
                        // If that movement doesn't work, snap them up the minimum vertical distance
                        snappedUp = AttemptSnapUp(
                            distanceToFeet + Epsilon * 2, 
                            stepUpDepth,
                            hit,
                            up,
                            momentum,
                            colliderCast,
                            ref position);
                    }

                    if (snappedUp)
                    {
                        yield return (initialPosition, new Ray(position, momentum), MovementAction.SnapUp);
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
                Vector3 projectedMomentum = Vector3.ProjectOnPlane(
                    momentum,
                    planeNormal).normalized * momentum.magnitude;
                
                // If projected momentum is less than original momentum (so if the projection broke due to float
                // operations), then change this to just project along the vertical.
                if (projectedMomentum.magnitude + Epsilon < momentum.magnitude)
                {
                    momentum = Vector3.ProjectOnPlane(momentum, up).normalized * momentum.magnitude;
                }
                else
                {
                    momentum = projectedMomentum;
                }

                // Track number of times the character has bounced
                bounces++;
                yield return (initialPosition, new Ray(position, momentum), MovementAction.Bounce);
            }
            // We're done, player was moved as part of loop

            yield return (position, new Ray(position, Vector3.zero), MovementAction.Stop);
            yield break;
        }
    }
}
