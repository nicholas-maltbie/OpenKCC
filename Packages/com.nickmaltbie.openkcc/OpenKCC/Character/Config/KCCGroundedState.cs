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

using System;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Character.Config
{
    /// <summary>
    /// Data structure to represent the current grounded state of
    /// a given kinematic character controller.
    /// </summary>
    [Serializable]
    public readonly struct KCCGroundedState : IKCCGrounded
    {
        /// <summary>
        /// Current distance the player is from the ground.
        /// </summary>
        public readonly float DistanceToGround { get; }

        /// <summary>
        /// Is the player currently standing on the ground.
        /// </summary>
        public readonly bool OnGround { get; }

        /// <summary>
        /// Angle between the ground and the player.
        /// </summary>
        public readonly float Angle { get; }

        /// <summary>
        /// The surface normal vector of the ground the player is standing on.
        /// </summary>
        public readonly Vector3 SurfaceNormal { get; }

        /// <summary>
        /// The point in which the player is hitting the ground.
        /// </summary>
        public readonly Vector3 GroundHitPosition { get; }

        /// <summary>
        /// What is the player standing on.
        /// </summary>
        public readonly GameObject Floor { get; }

        /// <summary>
        /// Is the player currently standing on the ground?
        /// Will be true if the hit the ground and the distance to the ground is less than
        /// the grounded threshold. NOTE, will be false if the player is overlapping with the
        /// ground or another object as it is difficult to tell whether they are stuck in a wall
        /// (and would therefore not be on the ground) versus when they are stuck in the floor.
        /// </summary>
        public readonly bool StandingOnGround { get; }

        /// <summary>
        /// Is the player currently standing on or overlapping with the ground.
        /// </summary>
        public readonly bool StandingOnGroundOrOverlap { get; }

        /// <summary>
        /// Is the player currently falling? this is true if they are either not standing on 
        /// the ground or if the angle between them and the ground is grater than the player's
        /// ability to walk.
        /// </summary>
        public readonly bool Falling { get; }

        /// <summary>
        /// Check if a player is sliding for a given max walk angle.
        /// </summary>
        /// <returns>True if the player is slipping/falling on the slope they are currently standing on.</returns>
        public readonly bool Sliding { get; }

        /// <summary>
        /// Setup a structure of the grounded
        /// state for a kinematic character controller.
        /// </summary>
        /// <param name="distanceToGround">Current distance the player is from the ground.</param>
        /// <param name="onGround">Is the player currently standing on the ground.</param>
        /// <param name="angle">Angle between the ground and the player.</param>
        /// <param name="surfaceNormal">The surface normal vector of the ground the player is standing on.</param>
        /// <param name="groundHitPosition">The point in which the player is hitting the ground.</param>
        /// <param name="floor">What is the player standing on.</param>
        /// <param name="groundedDistance">Distance at which the player is considered grounded.</param>
        /// <param name="maxWalkAngle">The maximum surface angle the player can walk at before sliding.</param>
        public KCCGroundedState(
            float distanceToGround,
            bool onGround,
            float angle,
            Vector3 surfaceNormal,
            Vector3 groundHitPosition,
            GameObject floor,
            float groundedDistance = 0.01f,
            float maxWalkAngle = 60.0f)
        {
            DistanceToGround = distanceToGround;
            OnGround = onGround;
            Angle = angle;
            SurfaceNormal = surfaceNormal;
            GroundHitPosition = groundHitPosition;
            Floor = floor;

            StandingOnGround = OnGround && DistanceToGround <= groundedDistance && DistanceToGround > 0;
            StandingOnGroundOrOverlap = OnGround && DistanceToGround <= groundedDistance;
            Falling = !StandingOnGround;
            Sliding = StandingOnGround && maxWalkAngle < Angle;
        }
    }
}
