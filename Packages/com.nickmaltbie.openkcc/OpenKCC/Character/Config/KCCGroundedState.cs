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

using System;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Character.Config
{
    /// <summary>
    /// Class to represent the current grounded state of
    /// a given kinematic character controller.
    /// </summary>
    [Serializable]
    public class KCCGroundedState : IKCCGrounded, IEquatable<KCCGroundedState>
    {
        /// <summary>
        /// Distance to ground at which player is considered grounded.
        /// </summary>
        [Tooltip("Distance from ground at which a player is considered standing on the ground")]
        [SerializeField]
        public float groundedDistance = 0.05f;

        /// <summary>
        /// Distance to check player distance to ground.
        /// </summary>
        [Tooltip("Distance to draw rays down when checking if player is grounded")]
        [SerializeField]
        public float groundCheckDistance = 0.25f;

        /// <summary>
        /// Maximum angle at which the player can walk (in degrees).
        /// </summary>
        [Tooltip("Maximum angle at which the player can walk")]
        [SerializeField]
        [Range(0, 90)]
        public float maxWalkAngle = 60f;

        /// <summary>
        /// Current distance the player is from the ground.
        /// </summary>
        public float DistanceToGround { get; private set; }

        /// <summary>
        /// Was the player grounded this frame
        /// </summary>
        public bool OnGround { get; private set; }

        /// <summary>
        /// Angle between the ground and the player.
        /// </summary>
        public float Angle { get; private set; }

        /// <summary>
        /// The surface normal vector of the ground the player is standing on.
        /// </summary>
        public Vector3 SurfaceNormal { get; private set; }

        /// <summary>
        /// The point in which the player is hitting the ground.
        /// </summary>
        public Vector3 GroundHitPosition { get; private set; }

        /// <summary>
        /// What is the player standing on.
        /// </summary>
        public GameObject Floor { get; private set; }

        /// <summary>
        /// Is the player currently standing on the ground?
        /// Will be true if the hit the ground and the distance to the ground is less than
        /// the grounded threshold. NOTE, will be false if the player is overlapping with the
        /// ground or another object as it is difficult to tell whether they are stuck in a wall
        /// (and would therefore not be on the ground) versus when they are stuck in the floor.
        /// </summary>
        public bool StandingOnGround => OnGround && DistanceToGround <= groundedDistance && DistanceToGround > 0;

        /// <summary>
        /// Is the player currently standing on or overlapping with the ground.
        /// </summary>
        public bool StandingOnGroundOrOverlap => OnGround && DistanceToGround <= groundedDistance;

        /// <summary>
        /// Is the player currently falling? this is true if they are either not standing on 
        /// the ground or if the angle between them and the ground is grater than the player's
        /// ability to walk.
        /// </summary>
        public bool Falling => !StandingOnGround;

        /// <summary>
        /// Check if a player is sliding for a given max walk angle.
        /// </summary>
        /// <returns>True if the player is slipping/falling on the slope they are currently standing on.</returns>
        public bool Sliding => StandingOnGround && Angle >= maxWalkAngle;

        /// <summary>
        /// Update the current grounded state of this kinematic character controller.
        /// </summary>
        /// <param name="kccConfig">Configuration of the character controller.</param>
        /// <param name="position">Character position in the world.</param>
        /// <param name="rotation">Character rotation in the world.</param>
        public void CheckGrounded(IKCCConfig kccConfig, Vector3 position, Quaternion rotation)
        {
            bool didHit = kccConfig.ColliderCast.CastSelf(
                position,
                rotation,
                -kccConfig.Up,
                groundCheckDistance,
                out IRaycastHit hit);

            Angle = Vector3.Angle(hit.normal, kccConfig.Up);
            DistanceToGround = hit.distance;
            OnGround = didHit;
            SurfaceNormal = hit.normal;
            GroundHitPosition = hit.distance > 0 ? hit.point : GroundHitPosition;
            Floor = hit.collider != null ? hit.collider.gameObject : null;
        }

        /// <summary>
        /// Get the movement of the player projected onto the plane
        /// they are standing on if they are not falling.
        /// </summary>
        /// <param name="movement">How the player is attempting to move</param>
        /// <returns>Projected movement onto the plane the player is standing on.</returns>
        public Vector3 GetProjectedMovement(Vector3 movement)
        {
            // If the player is standing on the ground, project their movement onto the ground plane
            // This allows them to walk up gradual slopes without facing a hit in movement speed
            if (StandingOnGround && Angle <= maxWalkAngle)
            {
                Vector3 projectedMovement = Vector3.ProjectOnPlane(movement, SurfaceNormal).normalized *
                    movement.magnitude;
                if (projectedMovement.magnitude + KCCUtils.Epsilon >= movement.magnitude)
                {
                    movement = projectedMovement;
                }
            }

            return movement;
        }

        public bool Equals(KCCGroundedState other)
        {
            return
                groundedDistance.Equals(other.groundedDistance) &&
                groundCheckDistance.Equals(other.groundCheckDistance) &&
                maxWalkAngle.Equals(other.maxWalkAngle);
        }
    }
}
