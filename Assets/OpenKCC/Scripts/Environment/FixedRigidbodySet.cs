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

using UnityEngine;

namespace PropHunt.Environment
{
    /// <summary>
    /// Set parameters for a kinematic rigidbody
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class FixedRigidbodySet : MonoBehaviour
    {

        /// <summary>
        /// Angular velocity of object in degrees per second for each euclidian axis
        /// </summary>
        [SerializeField]
        [Tooltip("Angular velocity of object in degrees per second for each euclidian axis")]
        protected Vector3 angularVelocity;

        /// <summary>
        /// Does this rotation work in local or world space. If true, will rotate in local space.
        /// If false will rotate in world space.
        /// </summary>
        [SerializeField]
        [Tooltip("Does this rotation work in local or world space")]
        protected bool localRotation;

        /// <summary>
        /// Linear velocity of object in units per second for each axis
        /// </summary>
        [SerializeField]
        [Tooltip("Linear velocity of object in units per second for each axis")]
        protected Vector3 linearVelocity;

        /// <summary>
        /// Does this velocity work in local or world space. If true, will translate in local space.
        /// If false will translate in world space.
        /// </summary>
        [SerializeField]
        [Tooltip("Does this translation work in local or world space.")]
        protected bool localTranslation;

        /// <summary>
        /// Rigidbody for this object
        /// </summary>
        protected new Rigidbody rigidbody;

        public void Start()
        {
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;
        }

        public void FixedUpdate()
        {
            if (linearVelocity.magnitude > 0)
            {
                // move object by velocity
                var deltaPos = Time.fixedDeltaTime * linearVelocity;
                if (localTranslation && transform.parent != null)
                {
                    rigidbody.MovePosition(transform.parent.position + transform.localPosition + deltaPos);
                }
                else
                {
                    rigidbody.MovePosition(transform.position + deltaPos);
                }
            }

            if (angularVelocity.magnitude > 0)
            {
                // rotate object by rotation
                var deltaRotation = Quaternion.Euler(Time.fixedDeltaTime * angularVelocity);
                if (localRotation && transform.parent != null)
                {
                    rigidbody.MoveRotation(transform.parent.rotation * transform.localRotation * deltaRotation);
                }
                else
                {
                    rigidbody.MoveRotation(transform.rotation * deltaRotation);
                }
            }
        }
    }
}
