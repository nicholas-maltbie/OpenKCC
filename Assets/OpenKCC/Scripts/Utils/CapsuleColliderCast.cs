using System;
using System.Collections.Generic;
using System.Linq;
using nickmaltbie.OpenKCC.Character;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// ColliderCast behaviour intended to work with any capsule collider shape.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public class CapsuleColliderCast : MonoBehaviour, IColliderCast
    {
        /// <summary>
        /// Capsule Collider associated with this object.
        /// </summary>
        private CapsuleCollider capsuleCollider;

        public void Start()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();
        }

        /// <summary>
        /// Gets transformed parameters describing this capsule collider
        /// </summary>
        public (Vector3, Vector3, float, float) GetParams()
        {
            var center = transform.TransformPoint(capsuleCollider.center);
            var radius = capsuleCollider.radius;
            var height = capsuleCollider.height;

            var bottom = center + transform.TransformDirection(Vector3.down) * (height / 2 - radius);
            var top = center + transform.TransformDirection(Vector3.up) * (height / 2 - radius);

            return (top, bottom, radius, height);
        }

        /// <inheritdoc/>
        public IEnumerable<Collider> GetOverlapping()
        {
            (var top, var bottom, var radius, var height) = GetParams();
            return Physics
                .OverlapCapsule(top, bottom, radius, ~0, QueryTriggerInteraction.Ignore)
                .Where(c => c.transform != transform);
        }

        /// <inheritdoc/>
        public IEnumerable<RaycastHit> GetHits(Vector3 direction, float distance)
        {
            (var top, var bottom, var radius, _) = GetParams();
            return Physics.CapsuleCastAll(top, bottom, radius, direction, distance, ~0, QueryTriggerInteraction.Ignore)
                .Where(hit => hit.collider.transform != transform);
        }

        /// <inheritdoc/>
        public bool CastSelf(Vector3 direction, float distance, out RaycastHit hit)
        {
            RaycastHit closest = new RaycastHit() { distance = Mathf.Infinity };
            bool hitSomething = false;
            foreach (RaycastHit objHit in GetHits(direction, distance))
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

            hit = closest;
            return hitSomething;
        }

        /// <inheritdoc/>
        public Vector3 PushOutOverlapping(float maxDistance)
        {
            Vector3 pushed = Vector3.zero;
            foreach (Collider overlap in this.GetOverlapping())
            {
                Physics.ComputePenetration(
                    capsuleCollider, transform.position, transform.rotation,
                    overlap, overlap.gameObject.transform.position, overlap.gameObject.transform.rotation,
                    out Vector3 direction, out float distance
                );
                float distPush = Mathf.Min(maxDistance, distance + KinematicCharacterController.Epsilon);
                Vector3 push = direction.normalized * distPush;
                transform.position += push;
                pushed += push;
            }
            return pushed;
        }
    }
}
