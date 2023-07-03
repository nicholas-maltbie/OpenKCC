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
    /// Collider cast for selecting between different primitive collider
    /// /// shapes.
    /// </summary>
    public class PrimitiveColliderCast : AbstractPrimitiveColliderCast
    {
        /// <summary>
        /// Selected collider configuration.
        /// </summary>
        [Tooltip("Collider type selected for casting.")]
        [SerializeField]
        public ColliderConfiguration config = new ColliderConfiguration();

        /// <summary>
        /// Generated collider for this primitive collider cast.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        private Collider generatedCollider;

        /// <summary>
        /// Create the collider for this primitive collider cast.
        /// </summary>
        public override void UpdateColliderParameters()
        {
            generatedCollider = config.AttachCollider(gameObject, true);
            base._collider = generatedCollider;
            if (config.type == ColliderType.Point)
            {
                SphereCollider sphere = gameObject.AddComponent<SphereCollider>();
                sphere.radius = KCCUtils.Epsilon / 2;
                sphere.center = config.pointCenter;
                generatedCollider = sphere;
            }
        }

        /// <inheritdoc/>
        public override Vector3 GetBottom(Vector3 position, Quaternion rotation)
        {
            switch (config.type)
            {
                case ColliderType.Box:
                    (Vector3 boxCenter, Vector3 boxSize) = BoxColliderCast.GetParams(config.Center, config.Size, position, rotation, -KCCUtils.Epsilon);
                    return boxCenter + rotation * Vector3.down * boxSize.y / 2;
                case ColliderType.Sphere:
                    (Vector3 sphereCenter, float sphereRadius) = SphereColliderCast.GetParams(config.Center, config.Radius, position, rotation);
                    return sphereCenter - sphereRadius * (rotation * transform.up);
                case ColliderType.Capsule:
                    (_, Vector3 capsuleBottom, float capsuleRadius, _) = CapsuleColliderCast.GetParams(config.Center, config.Radius, config.Height, position, rotation);
                    return capsuleBottom + capsuleRadius * (rotation * Vector3.down);
                case ColliderType.Point:
                default:
                    return position + rotation * (config.Center + Vector3.down * KCCUtils.Epsilon);
            }
        }

        /// <inheritdoc/>
        public override IEnumerable<RaycastHit> GetHits(Vector3 position, Quaternion rotation, Vector3 direction, float distance, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            IEnumerable<RaycastHit> hits;
            switch (config.type)
            {
                case ColliderType.Box:
                    (Vector3 boxCenter, Vector3 boxSize) = BoxColliderCast.GetParams(config.Center, config.Size, position, rotation, -KCCUtils.Epsilon);
                    hits = Physics.BoxCastAll(boxCenter, boxSize / 2, direction, rotation, distance, layerMask, queryTriggerInteraction);
                    break;
                case ColliderType.Sphere:
                    (Vector3 sphereCenter, float sphereRadius) = SphereColliderCast.GetParams(config.Center, config.Radius, position, rotation);
                    hits = Physics.SphereCastAll(sphereCenter, sphereRadius, direction, distance, layerMask, queryTriggerInteraction);
                    break;
                case ColliderType.Capsule:
                    (Vector3 capsuleTop, Vector3 capsuleBottom, float capsuleRadius, float capsuleHeight) = CapsuleColliderCast.GetParams(config.Center, config.Radius, config.Height, position, rotation);
                    hits = Physics.CapsuleCastAll(capsuleTop, capsuleBottom, capsuleRadius, direction, distance, layerMask, queryTriggerInteraction);
                    break;
                case ColliderType.Point:
                default:
                    Vector3 origin = position + rotation * config.Center;
                    hits = Physics.RaycastAll(origin, direction, distance, layerMask, queryTriggerInteraction);
                    break;
            }

            return hits.Where(hit => hit.collider.transform != transform);
        }

        /// <inheritdoc/>
        public override IEnumerable<Collider> GetOverlapping(Vector3 position, Quaternion rotation, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            IEnumerable<Collider> overlap;
            switch (config.type)
            {
                case ColliderType.Box:
                    (Vector3 boxCenter, Vector3 boxSize) = BoxColliderCast.GetParams(config.Center, config.Size, position, rotation, -KCCUtils.Epsilon);
                    overlap = Physics.OverlapBox(boxCenter, boxSize / 2, rotation, layerMask, queryTriggerInteraction);
                    break;
                case ColliderType.Sphere:
                    (Vector3 sphereCenter, float sphereRadius) = SphereColliderCast.GetParams(config.Center, config.Radius, position, rotation);
                    overlap = Physics.OverlapSphere(sphereCenter, sphereRadius, layerMask, queryTriggerInteraction);
                    break;
                case ColliderType.Capsule:
                    (Vector3 capsuleTop, Vector3 capsuleBottom, float capsuleRadius, float capsuleHeight) = CapsuleColliderCast.GetParams(config.Center, config.Radius, config.Height, position, rotation);
                    overlap = Physics.OverlapCapsule(capsuleTop, capsuleBottom, capsuleRadius, layerMask, queryTriggerInteraction);
                    break;
                case ColliderType.Point:
                default:
                    Vector3 origin = position + rotation * config.Center;
                    overlap = Physics.OverlapSphere(origin, KCCUtils.Epsilon / 2, layerMask, queryTriggerInteraction);
                    break;
            }

            return overlap.Where(collider => collider.transform != transform);
        }

        /// <inheritdoc/>
        protected override Collider SetupColliderComponent()
        {
            UpdateColliderParameters();
            return generatedCollider;
        }

        /// <summary>
        /// Do nothing, this is handled by editor script
        /// </summary>
        public override void OnValidate()
        {
        }
    }
}
