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
        /// <param name="colliderCast">Collider cast component associated with the KCC.</param>
        /// <returns></returns>
        public static Vector3 SnapPlayerDown(
            Vector3 position,
            Quaternion rotation,
            Vector3 dir,
            float dist,
            IColliderCast colliderCast)
        {
            bool didHit = colliderCast.CastSelf(
                position,
                rotation,
                dir,
                dist,
                out IRaycastHit hit);

            if (didHit && hit.distance > 0)
            {
                return position + dir * (hit.distance - KCCUtils.Epsilon * 2);
            }

            return position;
        }

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
        /// <param name="rotation">Rotation of the player.</param>
        /// <param name="position">Position of the player.</param>
        /// <returns>True if the player had space on the ledge and was able to move, false if
        /// there was not enough room the player is moved back to their original position</returns>
        public static bool AttemptSnapUp(
            float distanceToSnap,
            float stepUpDepth,
            Vector3 up,
            Vector3 momentum,
            IColliderCast colliderCast,
            Quaternion rotation,
            ref Vector3 position)
        {
            // If we were to snap the player up and they moved forward, would they hit something?
            Vector3 snapPos = position + distanceToSnap * up;
            bool didSnapHit = colliderCast
                .CastSelf(
                    snapPos,
                    rotation,
                    momentum.normalized,
                    stepUpDepth + Epsilon,
                    out IRaycastHit snapHit);

            // UnityEngine.Debug.Log($"AttemptingSnapUp snapHit.distance:{snapHit.distance} > {Epsilon} && (!didSnapHit:{!didSnapHit} || snapHit.distance{snapHit.distance} > stepUpDepth:{stepUpDepth})");

            // If they can move without instantly hitting something, then snap them up
            if (snapHit.distance > Epsilon && (!didSnapHit || snapHit.distance > stepUpDepth))
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
            IColliderCast colliderCast,
            IRaycastHit hit,
            Vector3 up,
            Vector3 momentum,
            ref Vector3 position,
            Quaternion rotation,
            float verticalSnapUp,
            float stepUpDepth)
        {
            // Snap character vertically up if they hit something
            //  close enough to their feet
            Vector3 bottom = colliderCast.GetBottom(position, rotation);
            Vector3 footVector = Vector3.Project(hit.point, up) - Vector3.Project(bottom, up);
            bool isAbove = Vector3.Dot(footVector, up) > 0;
            float distanceToFeet = footVector.magnitude * (isAbove ? 1 : -1);

            bool hitStep = colliderCast.CheckVerticalStepAhead(
                hit.point - up * Epsilon + hit.normal * Epsilon,
                momentum.normalized,
                momentum.magnitude,
                out IRaycastHit stepHit);
            bool perpendicularStep = hitStep && Vector3.Dot(stepHit.normal, up) <= Epsilon;

            bool snappedUp = false;

            if (hitStep &&
                perpendicularStep &&
                hit.distance > 0 &&
                distanceToFeet < verticalSnapUp &&
                distanceToFeet > 0)
            {
                // Sometimes snapping up the exact distance leads to odd behaviour around steps and walls.
                // It's good to check the maximum and minimum snap distances and take whichever one works.
                // snap them up the minimum vertical distance
                snappedUp = AttemptSnapUp(
                    distanceToFeet + Epsilon * 2,
                    stepUpDepth,
                    up,
                    momentum,
                    colliderCast,
                    rotation,
                    ref position);

                if (!snappedUp)
                {
                    // If that movement doesn't work, Attempt to snap up the maximum vertical distance
                    snappedUp = AttemptSnapUp(verticalSnapUp,
                        stepUpDepth,
                        up,
                        momentum,
                        colliderCast,
                        rotation,
                        ref position);
                }
            }

            return snappedUp;
        }

        /// <summary>
        /// Get player's projected movement along a surface.
        /// </summary>
        /// <param name="snappedUp">Did the player snap up the current movement.</param>
        /// <param name="momentum">Remaining player momentum.</param>
        /// <param name="planeNormal">Plane normal that the player is bouncing off of.</param>
        /// <param name="up">Upwards direction relative to player.</param>
        /// <returns>Remaining momentum of the player.</returns>
        public static Vector3 GetProjectedMomentumSave(bool snappedUp, Vector3 momentum, Vector3 planeNormal, Vector3 up)
        {
            Vector3 projectedMomentum = snappedUp ? momentum : Vector3.ProjectOnPlane(
                momentum,
                planeNormal).normalized * momentum.magnitude;

            // If projected momentum is less than original momentum (so if the projection broke due to float
            // operations), then change this to just project along the vertical.
            if (projectedMomentum.magnitude + Epsilon < momentum.magnitude)
            {
                return Vector3.ProjectOnPlane(momentum, up).normalized * momentum.magnitude;
            }
            else
            {
                return projectedMomentum;
            }
        }

        public static IEnumerable<KCCBounce> GetBounces(
            int maxBounces,
            float pushDecay,
            float verticalSnapUp,
            float stepUpDepth,
            float anglePower,
            bool attemptingJump,
            bool canSnapUp,
            Vector3 position,
            Vector3 movement,
            Quaternion rotation,
            Vector3 up,
            IColliderCast colliderCast,
            ICharacterPush push)
        {
            // Save current momentum
            Vector3 momentum = movement;

            // current number of bounces
            int bounces = 0;

            bool didSnapUp = false;

            // Continue computing while there is momentum and bounces remaining
            while (momentum.magnitude > Epsilon && bounces <= maxBounces)
            {
                // Position of character at start of bounce
                Vector3 initialPosition = position;
                Vector3 initialMomentum = momentum;

                // Do a cast of the collider to see if an object is hit during this
                // movement bounce
                float distance = momentum.magnitude;
                if (!colliderCast.CastSelf(position, rotation, momentum.normalized, distance, out IRaycastHit hit))
                {
                    // If there is no hit, move to desired position
                    position += momentum;

                    yield return new KCCBounce
                    {
                        initialPosition = initialPosition,
                        finalPosition = position,
                        initialMomentum = initialMomentum,
                        remainingMomentum = Vector3.zero,
                        action = MovementAction.Move,
                    };

                    // Exit as we are done bouncing
                    break;
                }

                // If we are overlapping, just exit
                if (hit.distance == 0)
                {
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
                position += hit.normal * Epsilon * 2;
                // Decrease remaining momentum by fraction of movement remaining
                momentum *= (1 - fraction);

                // Plane to project rest of movement onto
                Vector3 planeNormal = hit.normal;

                bool snappedUp =
                    canSnapUp &&
                    !attemptingJump &&
                    AttemptSnapUp(colliderCast, hit, up, momentum, ref position, rotation, verticalSnapUp, stepUpDepth);

                if (snappedUp)
                {
                    didSnapUp = true;
                    yield return new KCCBounce
                    {
                        initialPosition = initialPosition,
                        finalPosition = position,
                        initialMomentum = initialMomentum,
                        remainingMomentum = Vector3.zero,
                        action = MovementAction.SnapUp,
                    };
                }

                // If we did snap up, don't decay momentum
                if (!snappedUp)
                {
                    // If we didn't snap up:
                    // Only apply angular change if hitting something
                    // Get angle between surface normal and remaining movement
                    float angleBetween = Vector3.Angle(hit.normal, momentum) - 90.0f;

                    // Normalize angle between to be between 0 and 1
                    // 0 means no angle, 1 means 90 degree angle
                    angleBetween = Mathf.Min(MaxAngleShoveRadians, Mathf.Abs(angleBetween));
                    float normalizedAngle = angleBetween / MaxAngleShoveRadians;

                    // Reduce the momentum by the remaining movement that ocurred
                    momentum *= Mathf.Pow(1 - normalizedAngle, anglePower) * 0.9f + 0.1f;
                }
                // Rotate the remaining remaining movement to be projected along the plane 
                // of the surface hit (emulate pushing against the object)
                momentum = GetProjectedMomentumSave(snappedUp, momentum, planeNormal, up);

                // Track number of times the character has bounced
                bounces++;
                yield return new KCCBounce
                {
                    initialPosition = initialPosition,
                    finalPosition = position,
                    initialMomentum = initialMomentum,
                    remainingMomentum = Vector3.zero,
                    action = MovementAction.Bounce,
                };
            }

            if (didSnapUp)
            {
                position = SnapPlayerDown(position, rotation, -up, verticalSnapUp, colliderCast);
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
    }
}
