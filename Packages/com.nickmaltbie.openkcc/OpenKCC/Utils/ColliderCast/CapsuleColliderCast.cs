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
    /// ColliderCast behaviour intended to work with any capsule collider shape.
    /// </summary>
    public class CapsuleColliderCast : AbstractPrimitiveColliderCast
    {
        /// <summary>
        /// Default center for capsule collider.
        /// </summary>
        /// <returns></returns>
        private static readonly Vector3 DefaultCenter = new Vector3(0, 1, 0);

        /// <summary>
        /// Radius of the capsule.
        /// </summary>
        [SerializeField]
        [Tooltip("Radius of the capsule.")]
        public float radius = 0.5f;

        /// <summary>
        /// Height of the capsule.
        /// </summary>
        [SerializeField]
        [Tooltip("Height of the capsule.")]
        public float height = 2.0f;

        /// <summary>
        /// Center of the capsule.
        /// </summary>
        [SerializeField]
        [Tooltip("Center of the capsule.")]
        public Vector3 center = DefaultCenter;

        /// <summary>
        /// Capsule collider associated with this object.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private CapsuleCollider capsuleCollider = null;

        /// <summary>
        /// Mesh of capsule for debug drawing.
        /// </summary>
        private Mesh _debugCapsuleMesh;

        /// <summary>
        /// Debug mesh associated with capsule collider.
        /// </summary>
        public Mesh DebugCapsuleMesh => _debugCapsuleMesh = _debugCapsuleMesh ??
            CapsuleMaker.CapsuleData(radius: radius, depth: height - radius * 2);

        /// <summary>
        /// Gets transformed parameters describing this capsule collider for a given position and rotation
        /// </summary>
        /// <param name="capsuleCenter">Center of the capsule collider in relative position.</param>
        /// <param name="capsuleRadius">Radius of the capsule</param>
        /// <param name="capsuleHeight">height of the capsule.</param>
        /// <param name="position">Position of the object.</param>
        /// <param name="rotation">Rotation of the object.</param>
        /// <param name="radiusMod">Modifier to add to radius when computing shape of collider.</param>
        /// <returns>The top, bottom, radius, and height of the capsule collider</returns>
        public static (Vector3, Vector3, float, float) GetParams(Vector3 capsuleCenter, float capsuleRadius, float capsuleHeight, Vector3 position, Quaternion rotation, float radiusMod = 0.0f)
        {
            Vector3 center = rotation * capsuleCenter + position;
            float radius = capsuleRadius + radiusMod;
            float height = capsuleHeight + radiusMod * 2;

            Vector3 bottom = center + rotation * Vector3.down * (height / 2 - radius);
            Vector3 top = center + rotation * Vector3.up * (height / 2 - radius);

            return (top, bottom, radius, height);
        }

        /// <summary>
        /// Gets transformed parameters describing this capsule collider for a given position and rotation
        /// </summary>
        /// <param name="position">Position of the object.</param>
        /// <param name="rotation">Rotation of the object.</param>
        /// <param name="radiusMod">Modifier to add to radius when computing shape of collider.</param>
        /// <returns>The top, bottom, radius, and height of the capsule collider</returns>
        public (Vector3, Vector3, float, float) GetParams(Vector3 position, Quaternion rotation, float radiusMod = 0.0f)
        {
            return GetParams(center, radius, height, position, rotation, radiusMod);
        }

        /// <inheritdoc/>
        public override IEnumerable<Collider> GetOverlapping(
            Vector3 position,
            Quaternion rotation,
            int layerMask = RaycastHelperConstants.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction)
        {
            (Vector3 top, Vector3 bottom, float radius, float height) = GetParams(position, rotation);
            return Physics
                .OverlapCapsule(top, bottom, radius, layerMask, queryTriggerInteraction)
                .Where(c => c.transform != transform);
        }

        /// <inheritdoc/>
        public override IEnumerable<RaycastHit> GetHits(
            Vector3 position,
            Quaternion rotation,
            Vector3 direction,
            float distance,
            int layerMask = RaycastHelperConstants.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction)
        {
            (Vector3 top, Vector3 bottom, float radius, float height) = GetParams(position, rotation);
            return Physics.CapsuleCastAll(top, bottom, radius, direction, distance, layerMask, queryTriggerInteraction)
                .Where(hit => hit.collider.transform != transform);
        }

        /// <inheritdoc/>
        public override Vector3 GetBottom(Vector3 position, Quaternion rotation)
        {
            (_, Vector3 bottom, float radius, _) = GetParams(position, rotation);
            return bottom + radius * (rotation * Vector3.down);
        }

        /// <inheritdoc/>
        protected override Collider SetupColliderComponent()
        {
#if UNITY_EDITOR
            // Some magic to auto update the parameters from existing collider
            CapsuleCollider existingCollider = GetComponent<CapsuleCollider>();
            if (capsuleCollider == null && existingCollider != null)
            {
                capsuleCollider = existingCollider;
                radius = existingCollider.radius;
                center = existingCollider.center;
                height = existingCollider.height;
            }
#endif

            capsuleCollider = gameObject.GetComponent<CapsuleCollider>();
            if (capsuleCollider == null)
            {
                capsuleCollider = gameObject.AddComponent<CapsuleCollider>();
            }

            return capsuleCollider;
        }

        /// <inheritdoc/>
        public override void UpdateColliderParameters()
        {
            capsuleCollider.radius = radius;
            capsuleCollider.height = height;
            capsuleCollider.center = center;
        }
    }
}
