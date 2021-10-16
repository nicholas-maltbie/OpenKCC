using System.Collections.Generic;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Environment
{
    /// <summary>
    /// Script to translate a rigidbody object between two positions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MovingPlatform : MonoBehaviour
    {
        /// <summary>
        /// Velocity at which this platform should move
        /// </summary>
        public float linearSpeed = 3;

        /// <summary>
        /// Current target the platform is heading for
        /// </summary>
        private int currentTargetIndex = 0;

        /// <summary>
        /// Gets the current target we're moving towards
        /// </summary>
        public Transform CurrentTarget => targetsList[currentTargetIndex];

        /// <summary>
        /// List of targets to move between
        /// </summary>
        [SerializeField]
        public List<Transform> targetsList;

        public void FixedUpdate()
        {
            if (targetsList == null || targetsList.Count == 0)
            {
                return;
            }
            float deltaTime = Time.fixedDeltaTime;
            var direction = (CurrentTarget.position - transform.position).normalized;
            var displacement = direction * deltaTime * this.linearSpeed;
            var distanceToTarget = Vector3.Distance(transform.position, CurrentTarget.position);

            if (direction == Vector3.zero || distanceToTarget < displacement.magnitude)
            {
                displacement = CurrentTarget.position - transform.position;
                currentTargetIndex = (currentTargetIndex + 1) % targetsList.Count;
            }

            GetComponent<Rigidbody>().MovePosition(transform.position + displacement);
        }
    }
}