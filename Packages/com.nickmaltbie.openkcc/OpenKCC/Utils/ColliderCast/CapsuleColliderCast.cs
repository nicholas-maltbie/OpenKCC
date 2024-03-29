﻿// Copyright (C) 2023 Nicholas Maltbie
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
        /// Direction of the capsule.
        /// </summary>
        [SerializeField]
        [Tooltip("Direction of the capsule.")]
        private CapsuleDirection capsuleDirection = CapsuleDirection.Y;

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
        /// <param name="capsuleHeight">Height of the capsule.</param>
        /// <param name="capsuleDirection">Direction of the capsule.</param>
        /// <param name="position">Position of the object.</param>
        /// <param name="rotation">Rotation of the object.</param>
        /// <param name="radiusMod">Modifier to add to radius when computing shape of collider.</param>
        /// <returns>The top, bottom, radius, and height of the capsule collider</returns>
        public static (Vector3, Vector3, float, float) GetParams(Vector3 capsuleCenter, float capsuleRadius, float capsuleHeight, CapsuleDirection capsuleDirection, Vector3 position, Quaternion rotation, float radiusMod = 0.0f)
        {
            Vector3 center = rotation * capsuleCenter + position;
            float radius = capsuleRadius + radiusMod;
            float height = capsuleHeight + radiusMod * 2;

            Vector3 up = GetCapsuleRelativeUp(capsuleDirection);
            Vector3 bottom = center + rotation * -up * (height / 2 - radius);
            Vector3 top = center + rotation * up * (height / 2 - radius);

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
            return GetParams(center, radius, height, capsuleDirection, position, rotation, radiusMod);
        }

        /// <inheritdoc/>
        public override IEnumerable<Collider> GetOverlapping(
            Vector3 position,
            Quaternion rotation,
            int layerMask = RaycastHelperConstants.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction,
            float skinWidth = 0.0f)
        {
            (Vector3 top, Vector3 bottom, float radius, float height) = GetParams(position, rotation, -skinWidth);
            int overlap = Physics.OverlapCapsuleNonAlloc(top, bottom, radius, OverlapCache, layerMask, queryTriggerInteraction);
            return Enumerable.Range(0, overlap).Select(i => OverlapCache[i]).Where(c => c.transform != transform);
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
            (Vector3 top, Vector3 bottom, float radius, float height) = GetParams(position, rotation, -skinWidth);
            int hits = Physics.CapsuleCastNonAlloc(top, bottom, radius, direction, HitCache, distance + skinWidth, layerMask, queryTriggerInteraction);
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
            (_, Vector3 bottom, float radius, _) = GetParams(position, rotation);
            return bottom + radius * (rotation * -GetCapsuleRelativeUp(capsuleDirection));
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
                capsuleDirection = GetCapsuleDirection(existingCollider.direction);
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
            base.UpdateColliderParameters();
            capsuleCollider.radius = radius;
            capsuleCollider.height = height;
            capsuleCollider.center = center;
            capsuleCollider.direction = (int)capsuleDirection;
        }

        /// <summary>
        /// Get relative up direction for the capsule based on capsule direction.
        /// </summary>
        /// <param name="dir">Direction from a Capsule Collider.</param>
        /// <returns></returns>
        public static Vector3 GetCapsuleRelativeUp(CapsuleDirection capsuleDirection)
        {
            switch (capsuleDirection)
            {
                default:
                case CapsuleDirection.Y:
                    return Vector3.up;
                case CapsuleDirection.X:
                    return Vector3.right;
                case CapsuleDirection.Z:
                    return Vector3.forward;
            }
        }

        /// <summary>
        /// See https://docs.unity3d.com/ScriptReference/CapsuleCollider-direction.html
        /// </summary>
        /// <param name="dir">Direction from a Capsule Collider.</param>
        /// <returns>CapsuleCollider direction from int to CapsuleDirection</returns>
        private static CapsuleDirection GetCapsuleDirection(int dir)
        {
            return (CapsuleDirection)dir;
        }

        /// <summary>
        /// Debug function to reset the attached collider attribute.
        /// </summary>
        internal void ResetConfigDebug()
        {
            capsuleCollider = null;
        }
    }
}
