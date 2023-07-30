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
    public class SphereColliderCast : AbstractPrimitiveColliderCast
    {
        /// <summary>
        /// Radius of the sphere.
        /// </summary>
        [SerializeField]
        [Tooltip("Radius of the sphere.")]
        public float radius = 0.5f;

        /// <summary>
        /// Center of the sphere.
        /// </summary>
        [SerializeField]
        [Tooltip("Center of the sphere.")]
        public Vector3 center = Vector3.zero;

        /// <summary>
        /// Sphere collider associated with this object.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private SphereCollider sphereCollider;

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
            return GetParams(center, radius, position, rotation, radiusMod);
        }

        /// <inheritdoc/>
        public override IEnumerable<Collider> GetOverlapping(
            Vector3 position,
            Quaternion rotation,
            int layerMask = RaycastHelperConstants.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction,
            float skinWidth = 0.0f)
        {
            (Vector3 center, float radius) = GetParams(position, rotation, -skinWidth);
            int overlap = Physics.OverlapSphereNonAlloc(center, radius, OverlapCache, layerMask, queryTriggerInteraction);
            return Enumerable.Range(0, overlap).Select(i => OverlapCache[i])
                .Where(c => c.transform != transform);
        }

        /// <inheritdoc/>
        public override IEnumerable<RaycastHit> GetHits(
            Vector3 position,
            Quaternion rotation,
            Vector3 direction,
            float distance,
            int layerMask = RaycastHelperConstants.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction,
            float skinWidth = 0.01f)
        {
            (Vector3 center, float radius) = GetParams(position, rotation, -skinWidth);
            int hits = Physics.SphereCastNonAlloc(center, radius, direction, HitCache, distance + skinWidth, layerMask, queryTriggerInteraction);
            return Enumerable.Range(0, hits).Select(i => HitCache[i])
                .Where(hit => hit.collider.transform != transform)
                .Select(hit =>
                {
                    hit.distance = Mathf.Max(hit.distance - skinWidth, 0);
                    return hit;
                });
        }

        /// <inheritdoc/>
        public override Vector3 GetBottom(Vector3 position, Quaternion rotation)
        {
            (Vector3 center, float radius) = GetParams(position, rotation);
            return center - radius * (rotation * transform.up);
        }

        /// <inheritdoc/>
        protected override Collider SetupColliderComponent()
        {
#if UNITY_EDITOR
            // Some magic to auto update the parameters from existing collider
            SphereCollider existingCollider = GetComponent<SphereCollider>();
            if (sphereCollider == null && existingCollider != null)
            {
                sphereCollider = existingCollider;
                center = existingCollider.center;
                radius = existingCollider.radius;
            }
#endif

            sphereCollider = gameObject.GetComponent<SphereCollider>();
            if (sphereCollider == null)
            {
                sphereCollider = gameObject.AddComponent<SphereCollider>();
            }

            return sphereCollider;
        }

        /// <inheritdoc/>
        public override void UpdateColliderParameters()
        {
            base.UpdateColliderParameters();
            sphereCollider.radius = radius;
            sphereCollider.center = center;
        }

        /// <summary>
        /// Debug function to reset the attached collider attribute.
        /// </summary>
        internal void ResetConfigDebug()
        {
            sphereCollider = null;
        }
    }
}
