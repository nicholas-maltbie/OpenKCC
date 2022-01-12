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
using System.Linq;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// ColliderCast behaviour intended to work with any capsule collider shape.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public class CapsuleColliderCast : MonoBehaviour, IColliderCast
    {
        /// <summary>
        /// Capsule Collider associated with this object.
        /// </summary>
        private CapsuleCollider capsuleCollider;

        public void Start()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

        /// <summary>
        /// Gets transformed parameters describing this capsule collider
        /// </summary>
        public (Vector3, Vector3, float, float) GetParams()
        {
            Vector3 center = transform.TransformPoint(capsuleCollider.center);
            float radius = capsuleCollider.radius;
            float height = capsuleCollider.height;

            Vector3 bottom = center + transform.TransformDirection(Vector3.down) * (height / 2 - radius);
            Vector3 top = center + transform.TransformDirection(Vector3.up) * (height / 2 - radius);

            return (top, bottom, radius, height);
        }

        /// <inheritdoc/>
        public IEnumerable<Collider> GetOverlapping()
        {
            (Vector3 top, Vector3 bottom, float radius, float height) = GetParams();
            return Physics
                .OverlapCapsule(top, bottom, radius, ~0, QueryTriggerInteraction.Ignore)
                .Where(c => c.transform != transform);
        }

        /// <inheritdoc/>
        public IEnumerable<RaycastHit> GetHits(Vector3 direction, float distance)
        {
            (Vector3 top, Vector3 bottom, float radius, _) = GetParams();
            return Physics.CapsuleCastAll(top, bottom, radius, direction, distance, ~0, QueryTriggerInteraction.Ignore)
                .Where(hit => hit.collider.transform != transform);
        }

        /// <inheritdoc/>
        public bool CastSelf(Vector3 direction, float distance, out RaycastHit hit)
        {
            var closest = new RaycastHit() { distance = Mathf.Infinity };
            bool hitSomething = false;
            foreach (RaycastHit objHit in GetHits(direction, distance))
            {
                if (objHit.collider.gameObject.transform != gameObject.transform)
                {
                    if (objHit.distance < closest.distance)
                    {
                        closest = objHit;
                    }

                    hitSomething = true;
                }
            }

            hit = closest;
            return hitSomething;
        }

        /// <inheritdoc/>
        public Vector3 PushOutOverlapping(float maxDistance)
        {
            Vector3 pushed = Vector3.zero;
            foreach (Collider overlap in GetOverlapping())
            {
                Physics.ComputePenetration(
                    capsuleCollider, transform.position, transform.rotation,
                    overlap, overlap.gameObject.transform.position, overlap.gameObject.transform.rotation,
                    out Vector3 direction, out float distance
                );
                float distPush = Mathf.Min(maxDistance, distance + KCCUtils.Epsilon);
                Vector3 push = direction.normalized * distPush;
                transform.position += push;
                pushed += push;
            }

            return pushed;
        }

        /// <inheritdoc/>
        public Bounds GetBounds()
        {
            return capsuleCollider.bounds;
        }
    }
}
