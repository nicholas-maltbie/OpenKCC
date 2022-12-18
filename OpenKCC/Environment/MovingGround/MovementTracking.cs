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

using nickmaltbie.TestUtilsUnity;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Environment.MovingGround
{
    /// <summary>
    /// Component to track an object's displacement and rotation during an update
    /// </summary>
    public class MovementTracking : MonoBehaviour, IMovingGround
    {
        /// <summary>
        /// Unity service for getting fixed delta time.
        /// </summary>
        internal IUnityService unityService = UnityService.Instance;

        /// <summary>
        /// Should momentum be transferred to players when they
        /// leave this object.
        /// </summary>
        [SerializeField]
        internal bool avoidTransferMomentum;

        /// <summary>
        /// How much of the objects velocity should a player retain when leaving the surface of the object via jump or
        /// fall.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        internal float transferMomentumWeight = 1.0f;

        [SerializeField]
        internal bool shouldAttach = true;

        /// <inheritdoc/>
        public bool AvoidTransferMomentum() => avoidTransferMomentum;

        /// <inheritdoc/>
        public bool ShouldAttach() => shouldAttach;

        /// <summary>
        /// Previously measured position of an object (previous frame)
        /// </summary>
        public Vector3 PreviousPosition { get; private set; }

        /// <summary>
        /// Previously measured attitude of an object (previous frame)
        /// </summary>
        public Quaternion PreviousAttitude { get; private set; }

        /// <summary>
        /// Finds the change in attitude (expressed as a quaternion) between
        /// the current and previous update. QFinal * Inv(QInitial)
        /// </summary>
        public Quaternion ChangeAttitude { get; private set; }

        /// <summary>
        /// Displacement between current and previous update
        /// </summary>
        public Vector3 Displacement { get; private set; }

        public void FixedUpdate()
        {
            ChangeAttitude = transform.rotation * Quaternion.Inverse(PreviousAttitude);
            Displacement = transform.position - PreviousPosition;

            // Update state
            PreviousPosition = transform.position;
            PreviousAttitude = transform.rotation;
        }

        /// <inheritdoc/>
        public virtual Vector3 GetVelocityAtPoint(Vector3 point)
        {
            return GetDisplacementAtPoint(point) / unityService.fixedDeltaTime;
        }

        /// <inheritdoc/>
        public virtual Vector3 GetDisplacementAtPoint(Vector3 point)
        {
            // Get relative position to previous start
            Vector3 relativePosition = point - PreviousPosition;
            // Rotate point around center by change in attitude
            Vector3 rotatedFinalPosition = ChangeAttitude * relativePosition;
            // Get the delta due to rotation
            Vector3 deltaRotation = rotatedFinalPosition - relativePosition;
            // Shift point by total displacement
            return deltaRotation + Displacement;
        }

        /// <inheritdoc/>
        public virtual float GetMovementWeight(Vector3 point, Vector3 playerVelocity)
        {
            return 1.0f;
        }

        /// <inheritdoc/>
        public virtual float GetTransferMomentumWeight(Vector3 point, Vector3 playerVelocity)
        {
            return transferMomentumWeight;
        }
    }
}
