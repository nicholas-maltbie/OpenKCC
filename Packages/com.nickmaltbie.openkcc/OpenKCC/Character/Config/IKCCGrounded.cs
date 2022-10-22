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

using UnityEngine;

namespace nickmaltbie.OpenKCC.Character.Config
{
    /// <summary>
    /// interface to represent the current grounded state of
    /// a given kinematic character controller.
    /// </summary>
    public interface IKCCGrounded
    {
        /// <summary>
        /// Current distance the player is from the ground.
        /// </summary>
        public float DistanceToGround { get; }

        /// <summary>
        /// Was the player grounded this frame
        /// </summary>
        public bool OnGround { get; }

        /// <summary>
        /// Angle between the ground and the player.
        /// </summary>
        public float Angle { get; }

        /// <summary>
        /// The surface normal vector of the ground the player is standing on.
        /// </summary>
        public Vector3 SurfaceNormal { get; }

        /// <summary>
        /// The point in which the player is hitting the ground.
        /// </summary>
        public Vector3 GroundHitPosition { get; }

        /// <summary>
        /// What is the player standing on.
        /// </summary>
        public GameObject Floor { get; }

        /// <summary>
        /// Is the player currently standing on the ground?
        /// </summary>
        public bool StandingOnGround { get; }

        /// <summary>
        /// Is the player currently falling?
        /// </summary>
        public bool Falling { get; }

        /// <summary>
        /// Check if a player is sliding for a given max walk angle.
        /// </summary>
        public bool Sliding { get; }
    }
}
