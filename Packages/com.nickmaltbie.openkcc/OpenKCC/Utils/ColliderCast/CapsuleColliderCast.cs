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
    [RequireComponent(typeof(CapsuleCollider))]
    public class CapsuleColliderCast : AbstractPrimitiveColliderCast
    {
        /// <summary>
        /// Mesh of capsule for debug drawing.
        /// </summary>
        private Mesh _debugCapsuleMesh;

        /// <summary>
        /// Capsule collider associated with this object.
        /// </summary>
        private CapsuleCollider _capsuleCollider;

        /// <summary>
        /// Capsule Collider associated with this object.
        /// </summary>
        public CapsuleCollider CapsuleCollider => _capsuleCollider ??= GetComponent<CapsuleCollider>();

        /// <summary>
        /// Debug mesh associated with capsule collider.
        /// </summary>
        public Mesh DebugCapsuleMesh => _debugCapsuleMesh ??=
            CapsuleMaker.CapsuleData(radius: CapsuleCollider.radius, depth: CapsuleCollider.height - CapsuleCollider.radius * 2);

        /// <inheritdoc/>
        public override Collider Collider => CapsuleCollider;

        /// <summary>
        /// Gets transformed parameters describing this capsule collider for a given position and rotation
        /// </summary>
        /// <param name="position">Position of the object.</param>
        /// <param name="rotation">Rotation of the object.</param>
        /// <param name="radiusMod">Modifier to add to radius when computing shape of collider.</param>
        /// <returns>The top, bottom, radius, and height of the capsule collider</returns>
        public (Vector3, Vector3, float, float) GetParams(Vector3 position, Quaternion rotation, float radiusMod = 0.0f)
        {
            Vector3 center = rotation * CapsuleCollider.center + position;
            float radius = CapsuleCollider.radius + radiusMod;
            float height = CapsuleCollider.height + radiusMod * 2;

            Vector3 bottom = center + rotation * Vector3.down * (height / 2 - radius);
            Vector3 top = center + rotation * Vector3.up * (height / 2 - radius);

            return (top, bottom, radius, height);
        }

        /// <inheritdoc/>
        public override IEnumerable<Collider> GetOverlapping(
            Vector3 position,
            Quaternion rotation,
            int layerMask = IColliderCast.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = IColliderCast.DefaultQueryTriggerInteraction)
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
            int layerMask = IColliderCast.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = IColliderCast.DefaultQueryTriggerInteraction)
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
    }
}
