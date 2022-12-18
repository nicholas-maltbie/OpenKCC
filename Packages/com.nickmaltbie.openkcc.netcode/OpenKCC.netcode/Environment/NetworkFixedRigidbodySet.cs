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
using Unity.Netcode;
using UnityEngine;

namespace nickmaltbie.OpenKCC.netcode.Environment
{
    /// <summary>
    /// Set parameters for a kinematic rigidbody
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(0)]
    public class NetworkFixedRigidbodySet : NetworkBehaviour
    {
        /// <summary>
        /// Reference to unity service for getting basic unity functions.
        /// </summary>
        public IUnityService unityService = UnityService.Instance;

        /// <summary>
        /// Angular velocity of object in degrees per second for each Euclidean axis
        /// </summary>
        [SerializeField]
        [Tooltip("Angular velocity of object in degrees per second for each Euclidean axis")]
        public Vector3 angularVelocity;

        /// <summary>
        /// Does this rotation work in local or world space. If true, will rotate in local space.
        /// If false will rotate in world space.
        /// </summary>
        [SerializeField]
        [Tooltip("Does this rotation work in local or world space")]
        public bool localRotation;

        /// <summary>
        /// Linear velocity of object in units per second for each axis
        /// </summary>
        [SerializeField]
        [Tooltip("Linear velocity of object in units per second for each axis")]
        public Vector3 linearVelocity;

        /// <summary>
        /// Does this velocity work in local or world space. If true, will translate in local space.
        /// If false will translate in world space.
        /// </summary>
        [SerializeField]
        [Tooltip("Does this translation work in local or world space.")]
        public bool localTranslation;

        public void Update()
        {
            if (!IsServer)
            {
                return;
            }

            if (linearVelocity.magnitude > 0)
            {
                // move object by velocity
                Vector3 deltaPos = unityService.deltaTime * linearVelocity;
                Vector3 targetPos = (localTranslation && transform.parent != null) ?
                    transform.parent.position + transform.localPosition + deltaPos :
                    transform.position + deltaPos;

                transform.position = targetPos;
            }

            if (angularVelocity.magnitude > 0)
            {
                // rotate object by rotation
                var deltaRotation = Quaternion.Euler(unityService.deltaTime * angularVelocity);
                Quaternion targetRot = (localRotation && transform.parent != null) ?
                    transform.parent.rotation * transform.localRotation * deltaRotation :
                    transform.rotation * deltaRotation;

                transform.rotation = targetRot;
            }
        }
    }
}
