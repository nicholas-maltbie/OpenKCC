// Copyright (C) 2023 Nicholas Maltbie
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
    /// ColliderCast behaviour intended to work with any sphere collider shape.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class SphereColliderCast : AbstractPrimitiveColliderCast
    {
        /// <summary>
        /// Sphere collider associated with this object.
        /// </summary>
        private SphereCollider _sphereCollider;

        /// <summary>
        /// Sphere Collider associated with this object.
        /// </summary>
        internal SphereCollider SphereCollider => _sphereCollider ??= GetComponent<SphereCollider>();

        /// <inheritdoc/>
        public override Collider Collider => SphereCollider;

        /// <summary>
        /// Gets transformed parameters describing this sphere collider for a given position and rotation
        /// </summary>
        /// <param name="sphereCenter">Relative position of sphere center.</param>
        /// <param name="sphereRadius">Radius of the sphere.</param>
        /// <param name="position">Position of the object.</param>
        /// <param name="rotation">Rotation of the object.</param>
        /// <param name="radiusMod">Modifier to add to radius when computing shape of collider.</param>
        /// <returns>Returns the center of the sphere in world space and the modified radius.</returns>
        public static (Vector3, float) GetParams(Vector3 sphereCenter, float sphereRadius, Vector3 position, Quaternion rotation, float radiusMod = 0.0f)
        {
            Vector3 center = rotation * sphereCenter + position;
            float radius = sphereRadius + radiusMod;

            return (center, radius);
        }

        /// <summary>
        /// Gets transformed parameters describing this sphere collider for a given position and rotation
        /// </summary>
        /// <param name="position">Position of the object.</param>
        /// <param name="rotation">Rotation of the object.</param>
        /// <param name="radiusMod">Modifier to add to radius when computing shape of collider.</param>
        /// <returns>Returns the center of the collider in world space
        /// and the modified radius.</returns>
        public (Vector3, float) GetParams(Vector3 position, Quaternion rotation, float radiusMod = 0.0f)
        {
            return GetParams(SphereCollider.center, SphereCollider.radius, position, rotation, radiusMod);
        }

        /// <inheritdoc/>
        public override IEnumerable<Collider> GetOverlapping(
            Vector3 position,
            Quaternion rotation,
            int layerMask = IColliderCast.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = IColliderCast.DefaultQueryTriggerInteraction)
        {
            (Vector3 center, float radius) = GetParams(position, rotation);
            return Physics
                .OverlapSphere(center, radius, layerMask, queryTriggerInteraction)
                .Where(c => c.transform != transform);
        }

        /// <inheritdoc/>
        public override IEnumerable<RaycastHit> GetHits(
            Vector3 position,
            Quaternion rotation,
            Vector3 direction,
            float distance,
            int layerMask = IColliderCast.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = IColliderCast.DefaultQueryTriggerInteraction)
        {
            (Vector3 center, float radius) = GetParams(position, rotation);
            return Physics.SphereCastAll(center, radius, direction, distance, layerMask, queryTriggerInteraction)
                .Where(hit => hit.collider.transform != transform);
        }

        /// <inheritdoc/>
        public override Vector3 GetBottom(Vector3 position, Quaternion rotation)
        {
            (Vector3 center, float radius) = GetParams(position, rotation);
            return center - radius * (rotation * transform.up);
        }
    }
}
