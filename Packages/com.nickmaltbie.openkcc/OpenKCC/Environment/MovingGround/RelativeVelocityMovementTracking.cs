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

namespace nickmaltbie.OpenKCC.Environment.MovingGround
{
    /// <summary>
    /// Component to track an object's displacement and rotation during an update.
    /// Also have a scaling factor based on player's relative velocity.
    /// This will fully attach for relative speeds below the minimum relative velocity and fully detach players for
    /// velocities over the maximum velocity threshold. 
    /// </summary>
    public class RelativeVelocityMovementTracking : MovementTracking
    {
        /// <summary>
        /// Relative velocity under minimum velocity threshold will be fully attached.
        /// </summary>
        [SerializeField]
        [Tooltip("Relative velocity under minimum velocity threshold will be fully attached.")]
        internal float minimumVelocityThreshold = 1.0f;

        /// <summary>
        /// Relative velocities above minimum velocity threshold will be fully detached.
        /// </summary>
        [SerializeField]
        [Tooltip("Relative velocities above minimum velocity threshold will be fully detached.")]
        internal float maximumVelocityThreshold = 3.0f;

        /// <inheritdoc/>
        public override float GetMovementWeight(Vector3 point, Vector3 playerVelocity)
        {
            float objSpeed = GetVelocityAtPoint(point).magnitude;
            float playerSpeed = playerVelocity.magnitude;

            float relativeVelocity = Mathf.Abs(objSpeed - playerSpeed);

            if (relativeVelocity <= minimumVelocityThreshold)
            {
                return 1.0f;
            }
            else if (relativeVelocity < maximumVelocityThreshold)
            {
                return 1 - (relativeVelocity - minimumVelocityThreshold) /
                    (maximumVelocityThreshold - minimumVelocityThreshold);
            }

            return 0.0f;
        }
    }
}
