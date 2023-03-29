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

using System;
using System.Collections.Generic;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils.ColliderCast
{
    /// <summary>
    /// Collider cast for selecting between different primitive collider
    /// /// shapes.
    /// </summary>
    public class PrimitiveColliderCast : AbstractPrimitiveColliderCast
    {
        [Tooltip("Collider type selected for casting.")]
        [SerializeField]
        public ColliderConfiguration colliderConfig;

        private Collider GeneratedCollider { get; set; }

        public override Collider Collider => GeneratedCollider;

        internal IEnumerable<Collider> ExtraColliders { get; private set; } = new Collider[0];

        public void Awake()
        {
            ConfigureColliders();
        }

        public void ConfigureColliders()
        {
            GeneratedCollider = colliderConfig.AttachCollider(gameObject, true);
        }

        public override Vector3 GetBottom(Vector3 position, Quaternion rotation)
        {
            return Vector3.zero;
        }

        public override IEnumerable<RaycastHit> GetHits(Vector3 position, Quaternion rotation, Vector3 direction, float distance, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            yield break;
        }

        public override IEnumerable<Collider> GetOverlapping(Vector3 position, Quaternion rotation, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            yield break;
        }
    }
}
