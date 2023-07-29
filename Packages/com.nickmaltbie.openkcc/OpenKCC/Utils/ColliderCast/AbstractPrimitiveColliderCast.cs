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
using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils.ColliderCast
{
    /// <summary>
    /// Abstract class to optimize operations for a primitive shape
    /// collider cast.
    /// </summary>
    public abstract class AbstractPrimitiveColliderCast : MonoBehaviour, IColliderCast
    {
        /// <summary>
        /// Cache size for raycast hit.
        /// </summary>
        public const int RaycastHitCacheSize = 25;

        /// <summary>
        /// Cache size for collider overlap.
        /// </summary>
        public const int OverlapCacheSize = 25;

        /// <summary>
        /// Overlap hit cache.
        /// </summary>
        protected static Collider[] OverlapCache = new Collider[OverlapCacheSize];

        /// <summary>
        /// Raycast hit cache.
        /// </summary>
        protected static RaycastHit[] HitCache = new RaycastHit[RaycastHitCacheSize];

        /// <summary>
        /// Collider associated with this primitive collider cast.
        /// </summary>
        protected Collider _collider;

        /// <summary>
        /// Hiding flags for the collider component.
        /// </summary>
        protected virtual HideFlags ColliderHideFlags => HideFlags.NotEditable;

        /// <summary>
        /// Configure collider component.
        /// </summary>
        public virtual void Start()
        {
            SetupCollider();
        }

        /// <inheritdoc/>
        public bool CastSelf(
            Vector3 position,
            Quaternion rotation,
            Vector3 direction,
            float distance,
            out IRaycastHit hit,
            int layerMask = RaycastHelperConstants.DefaultLayerMask,
            QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction,
            float skinWidth = 0.01f)
        {
            var closest = new RaycastHit() { distance = Mathf.Infinity };
            bool hitSomething = false;
            foreach (RaycastHit objHit in GetHits(position, rotation, direction, distance, layerMask, queryTriggerInteraction, skinWidth))
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
        public virtual Vector3 PushOutOverlapping(Vector3 position, Quaternion rotation, float maxDistance, int layerMask = RaycastHelperConstants.DefaultLayerMask, QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction, float skinWidth = 0.0f)
        {
            Vector3 pushed = Vector3.zero;
            foreach (Collider overlap in GetOverlapping(position, rotation, layerMask, queryTriggerInteraction, skinWidth))
            {
                Physics.ComputePenetration(
                    Collider, position, rotation,
                    overlap, overlap.gameObject.transform.position, overlap.gameObject.transform.rotation,
                    out Vector3 direction, out float distance
                );

                float distPush = Mathf.Min(maxDistance, distance);
                Vector3 push = direction.normalized * distPush;
                pushed += push;
                position += push;
            }

            return pushed;
        }

        /// <inheritdoc/>
        public bool DoRaycastInDirection(Vector3 source, Vector3 direction, float distance, out IRaycastHit stepHit, int layerMask = RaycastHelperConstants.DefaultLayerMask, QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction)
        {
            bool didHit = Physics.Raycast(new Ray(source, direction), out RaycastHit hit, distance, layerMask, queryTriggerInteraction);
            stepHit = new RaycastHitWrapper(hit);
            return didHit;
        }

        /// <summary>
        /// Primitive collider shape associated with this object.
        /// </summary>
        public Collider Collider => _collider = _collider ?? SetupColliderComponent();

        /// <inheritdoc/>
        public abstract Vector3 GetBottom(Vector3 position, Quaternion rotation);

        /// <inheritdoc/>
        public abstract IEnumerable<Collider> GetOverlapping(Vector3 position, Quaternion rotation, int layerMask = RaycastHelperConstants.DefaultLayerMask, QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction, float skinWidth = 0.00f);

        /// <inheritdoc/>
        public abstract IEnumerable<RaycastHit> GetHits(Vector3 position, Quaternion rotation, Vector3 direction, float distance, int layerMask = RaycastHelperConstants.DefaultLayerMask, QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction, float skinWidth = 0.01f);

        /// <summary>
        /// Update the parameters of the collider on a configuration change.
        /// </summary>
        public virtual void UpdateColliderParameters()
        {
            if (_collider != null)
            {
                _collider.hideFlags = ColliderHideFlags;
            }
        }

        /// <summary>
        /// Setup the collider component associated with this object.
        /// </summary>
        /// <returns>Collider created (or one that already exists) for this object.</returns>
        protected abstract Collider SetupColliderComponent();

        /// <summary>
        /// Setup the collider associated with this object.
        /// </summary>
        public void SetupCollider()
        {
            _collider = SetupColliderComponent();
            UpdateColliderParameters();
        }
    }
}
