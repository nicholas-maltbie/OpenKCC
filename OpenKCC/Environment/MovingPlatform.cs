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
using nickmaltbie.TestUtilsUnity;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Environment
{
    /// <summary>
    /// Script to translate a rigidbody object between two positions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MovingPlatform : MonoBehaviour
    {
        /// <summary>
        /// Current target the platform is heading for.
        /// </summary>
        public int currentTargetIndex { get; private set; } = 0;

        /// <summary>
        /// Unity service for managing time.
        /// </summary>
        internal IUnityService untiyService = UnityService.Instance;

        /// <summary>
        /// Should continuous movement be used to move the object
        /// or discrete steps. uses the MovePosition and MoveRotation api
        /// for continuous movement otherwise.
        /// </summary>
        internal bool isContinuous = true;

        /// <summary>
        /// Velocity at which this platform should move.
        /// </summary>
        [SerializeField]
        [Tooltip("Velocity at which this platform should move.")]
        internal float linearSpeed = 3;

        /// <summary>
        /// List of targets to move between.
        /// </summary>
        [SerializeField]
        [Tooltip("List of targets to move between.")]
        internal List<Transform> targetsList;

        /// <summary>
        /// Gets the current target we're moving towards.
        /// </summary>
        public Transform CurrentTarget => ValidTarget ? targetsList[currentTargetIndex] : null;

        /// <summary>
        /// Does the moving platform have a valid target.
        /// </summary>
        public bool ValidTarget => targetsList != null && targetsList.Count > 0;

        public void FixedUpdate()
        {
            if (!ValidTarget)
            {
                return;
            }

            Rigidbody rb = GetComponent<Rigidbody>();

            Vector3 direction = (CurrentTarget.position - rb.position).normalized;
            Vector3 displacement = direction * untiyService.fixedDeltaTime * linearSpeed;
            float distanceToTarget = Vector3.Distance(rb.position, CurrentTarget.position);

            if (direction == Vector3.zero || distanceToTarget < displacement.magnitude)
            {
                displacement = CurrentTarget.position - rb.position;
                currentTargetIndex = (currentTargetIndex + 1) % targetsList.Count;
            }

            if (isContinuous)
            {
                rb.MovePosition(rb.position + displacement);
            }
            else
            {
                rb.position += displacement;
            }
        }
    }
}
