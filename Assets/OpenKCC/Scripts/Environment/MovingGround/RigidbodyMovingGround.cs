using UnityEngine;

namespace nickmaltbie.OpenKCC.Environment.MovingGround
{
    /// <summary>
    /// Moving ground defined by a rigidbody
    /// </summary>
    public class RigidbodyMovingGround : MonoBehaviour, IMovingGround
    {
        [SerializeField]
        protected Rigidbody attachedRigidbody;

        [SerializeField]
        protected NetworkRigidbody attachedNetworkRigidbody;

        /// <summary>
        /// How much of the objects velocity should a player retain when leaving the surface of the object via jump or
        /// fall.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        private float transferMomentumWeight = 1.0f;

        /// <summary>
        /// Should momentum be transferred to players when they
        /// leave this object.
        /// </summary>
        public bool avoidTransferMomentum;

        /// <inheritdoc/>
        public bool AvoidTransferMomentum() => avoidTransferMomentum;

        /// <inheritdoc/>
        public bool ShouldAttach() => true;

        public void Start()
        {
            if (attachedRigidbody == null)
            {
                attachedRigidbody = GetComponent<Rigidbody>();
            }
            if (attachedNetworkRigidbody == null)
            {
                attachedNetworkRigidbody = GetComponent<NetworkRigidbody>();
            }
        }

        /// <inheritdoc/>
        public Vector3 GetVelocityAtPoint(Vector3 point, float deltaTime)
        {
            if (attachedNetworkRigidbody != null)
            {
                return attachedNetworkRigidbody.GetVelocityAtPoint(point);
            }
            return attachedRigidbody.GetPointVelocity(point);
        }

        /// <inheritdoc/>
        public Vector3 GetDisplacementAtPoint(Vector3 point, float deltaTime)
        {
            return attachedRigidbody.GetPointVelocity(point) * deltaTime;
        }

        /// <inheritdoc/>
        public virtual float GetMovementWeight(Vector3 point, Vector3 playerVelocity, float deltaTime)
        {
            return 1.0f;
        }

        public float GetTransferMomentumWeight(Vector3 point, Vector3 playerVelocity, float deltaTime)
        {
            return transferMomentumWeight;
        }
    }
}