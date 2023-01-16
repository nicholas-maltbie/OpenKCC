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
using Unity.Netcode;
using UnityEngine;

namespace nickmaltbie.OpenKCC.netcode.Environment
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(0)]
    public class NetworkMovingPlatform : NetworkBehaviour
    {
        /// <summary>
        /// Moving platform with network configuration.
        /// </summary>
        private NetworkVariable<int> _currentTarget = new NetworkVariable<int>(
            value: 0,
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Server);

        /// <summary>
        /// Current target the platform is heading for.
        /// </summary>
        public int CurrentTargetIdx
        {
            get => _currentTarget.Value;
            private set => _currentTarget.Value = value;
        }

        /// <summary>
        /// Unity service for managing time.
        /// </summary>
        internal IUnityService untiyService = UnityService.Instance;

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
        internal List<Transform> targetsList = new List<Transform>();

        /// <summary>
        /// Gets the current target we're moving towards.
        /// </summary>
        public Transform CurrentTarget => ValidTarget ? targetsList[CurrentTargetIdx] : null;

        /// <summary>
        /// Does the moving platform have a valid target.
        /// </summary>
        public bool ValidTarget => targetsList != null && targetsList.Count > 0;

        public void Update()
        {
            if (!ValidTarget || !IsServer)
            {
                return;
            }

            Vector3 direction = (CurrentTarget.position - transform.position).normalized;
            Vector3 displacement = direction * untiyService.deltaTime * linearSpeed;
            float distanceToTarget = Vector3.Distance(transform.position, CurrentTarget.position);

            if (direction == Vector3.zero || distanceToTarget < displacement.magnitude)
            {
                displacement = CurrentTarget.position - transform.position;

                if (IsServer)
                {
                    CurrentTargetIdx = (CurrentTargetIdx + 1) % targetsList.Count;
                }
            }

            transform.position += displacement;
        }
    }
}
