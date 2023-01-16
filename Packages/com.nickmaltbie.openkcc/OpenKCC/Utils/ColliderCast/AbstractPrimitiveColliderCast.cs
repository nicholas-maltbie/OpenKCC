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

namespace nickmaltbie.OpenKCC.Utils.ColliderCast
{
    /// <summary>
    /// Abstract class to optimize operations for a primitive shape
    /// collider cast.
    /// </summary>
    public abstract class AbstractPrimitiveColliderCast : MonoBehaviour, IColliderCast
    {
        /// <inheritdoc/>
        public bool CastSelf(
            Vector3 position,
            Quaternion rotation,
            Vector3 direction,
            float distance,
            out IRaycastHit hit)
        {
            var closest = new RaycastHit() { distance = Mathf.Infinity };
            bool hitSomething = false;
            foreach (RaycastHit objHit in GetHits(position, rotation, direction, distance))
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

            hit = new RaycastHitWrapper(closest);
            return hitSomething;
        }

        /// <inheritdoc/>
        public Vector3 PushOutOverlapping(Vector3 position, Quaternion rotation, float maxDistance)
        {
            Vector3 pushed = Vector3.zero;
            foreach (Collider overlap in GetOverlapping(position, rotation))
            {
                Physics.ComputePenetration(
                    Collider, position, rotation,
                    overlap, overlap.gameObject.transform.position, overlap.gameObject.transform.rotation,
                    out Vector3 direction, out float distance
                );

                float distPush = Mathf.Min(maxDistance, distance + KCCUtils.Epsilon);
                Vector3 push = direction.normalized * distPush;
                position += push;
                pushed += push;
            }

            return pushed;
        }

        /// <inheritdoc/>
        public bool DoRaycastInDirection(Vector3 source, Vector3 direction, float distance, out IRaycastHit stepHit)
        {
            bool didHit = Physics.Raycast(new Ray(source, direction), out RaycastHit hit, distance);
            stepHit = new RaycastHitWrapper(hit);
            return didHit;
        }

        /// <summary>
        /// Primitive collider shape associated with this object.
        /// </summary>
        abstract public Collider Collider { get; }

        /// <inheritdoc/>
        abstract public Vector3 GetBottom(Vector3 position, Quaternion rotation);

        /// <inheritdoc/>
        abstract public IEnumerable<Collider> GetOverlapping(Vector3 position, Quaternion rotation);

        /// <inheritdoc/>
        abstract  public IEnumerable<RaycastHit> GetHits(Vector3 position, Quaternion rotation, Vector3 direction, float distance);
    }
}