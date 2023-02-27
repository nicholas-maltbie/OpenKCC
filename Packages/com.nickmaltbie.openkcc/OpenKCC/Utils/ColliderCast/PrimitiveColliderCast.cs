
using System;
using System.Collections.Generic;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils.ColliderCast
{
    public enum ColliderType
    {
        Box,
        Capsule,
        Sphere,
    }

    [Serializable]
    public struct BoxColliderConfiguration
    {
        public Vector3 centerOffset;
        public Vector3 size;
    }

    [Serializable]
    public struct SphereColliderConfiguration
    {
        public Vector3 centerOffset;
        public float radius;
    }

    [Serializable]
    public struct CapsuleColliderConfiguration
    {
        public Vector3 centerOffset;
        public float radius;
        public float height;
    }

    /// <summary>
    /// Collider cast for selecting between different primitive collider
    /// /// shapes.
    /// </summary>
    public class PrimitiveColliderCast : AbstractPrimitiveColliderCast
    {
        [Tooltip("Collider type selected for end user.")]
        [SerializeField]
        public ColliderType colliderType;

        [SerializeField]
        [HideInInspector]
        private BoxColliderConfiguration boxConfig;

        [SerializeField]
        [HideInInspector]
        private SphereColliderConfiguration sphereConfig;

        [SerializeField]
        [HideInInspector]
        private CapsuleColliderConfiguration capsuleColliderConfiguration;

        private Collider GeneratedCollider { get; set; }
        public override Collider Collider => GeneratedCollider;

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