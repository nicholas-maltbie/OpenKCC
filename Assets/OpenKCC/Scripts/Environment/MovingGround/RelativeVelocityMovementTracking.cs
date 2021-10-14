using UnityEngine;

namespace nickmaltbie.OpenKCC.Environment.MovingGround
{
    /// <summary>
    /// Component to track an object's displacement and rotation during an update.
    /// Also have a scaling factor based on player's relative velocity.
    /// This will fully attach for relative speeds below the minimum relative velocity and fully detach players for
    /// velocities over the maximum velocity threshold. 
    /// </summary>
    public class RelativeVelocityMovementTracking : MovementTracking
    {
        /// <summary>
        /// Relative velocity under minimum velocity threshold will be fully attached.
        /// </summary>
        [SerializeField]
        [Tooltip("Relative velocity under minimum velocity threshold will be fully attached.")]
        private float minimumVelocityThreshold = 1.0f;

        /// <summary>
        /// Relative velocities above minimum velocity threshold will be fully detached.
        /// </summary>
        [SerializeField]
        [Tooltip("Relative velocities above minimum velocity threshold will be fully detached.")]
        private float maximumVelocityThreshold = 3.0f;

        /// <inheritdoc/>
        public override float GetMovementWeight(Vector3 point, Vector3 playerVelocity, float deltaTime)
        {
            float objSpeed = this.GetVelocityAtPoint(point, deltaTime).magnitude;
            float playerSpeed = playerVelocity.magnitude;

            float relativeVelocity = Mathf.Abs(objSpeed - playerSpeed);


            if (relativeVelocity <= minimumVelocityThreshold)
            {
                return 1.0f;
            }
            else if (relativeVelocity < maximumVelocityThreshold)
            {
                return 1 - (relativeVelocity - minimumVelocityThreshold) /
                    (maximumVelocityThreshold - minimumVelocityThreshold);
            }
            return 0.0f;
        }
    }
}