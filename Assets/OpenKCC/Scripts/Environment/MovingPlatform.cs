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
//

using System.Collections.Generic;

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
        /// Velocity at which this platform should move
        /// </summary>
        public float linearSpeed = 3;

        /// <summary>
        /// Current target the platform is heading for
        /// </summary>
        private int currentTargetIndex = 0;

        /// <summary>
        /// Gets the current target we're moving towards
        /// </summary>
        public Transform CurrentTarget => targetsList[currentTargetIndex];

        /// <summary>
        /// List of targets to move between
        /// </summary>
        [SerializeField]
        public List<Transform> targetsList;

        public void FixedUpdate()
        {
            if (targetsList == null || targetsList.Count == 0)
            {
                return;
            }

            var deltaTime = Time.fixedDeltaTime;
            var direction = (CurrentTarget.position - transform.position).normalized;
            var displacement = direction * deltaTime * linearSpeed;
            var distanceToTarget = Vector3.Distance(transform.position, CurrentTarget.position);

            if (direction == Vector3.zero || distanceToTarget < displacement.magnitude)
            {
                displacement = CurrentTarget.position - transform.position;
                currentTargetIndex = (currentTargetIndex + 1) % targetsList.Count;
            }

            GetComponent<Rigidbody>().MovePosition(transform.position + displacement);
        }
    }
}
