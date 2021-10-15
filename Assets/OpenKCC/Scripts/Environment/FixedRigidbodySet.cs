using UnityEngine;

namespace PropHunt.Environment
{
    /// <summary>
    /// Set parameters for a kinematic rigidbody
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class FixedRigidbodySet : MonoBehaviour
    {

        /// <summary>
        /// Angular velocity of object in degrees per second for each euclidian axis
        /// </summary>
        [SerializeField]
        [Tooltip("Angular velocity of object in degrees per second for each euclidian axis")]
        protected Vector3 angularVelocity;

        /// <summary>
        /// Does this rotation work in local or world space. If true, will rotate in local space.
        /// If false will rotate in world space.
        /// </summary>
        [SerializeField]
        [Tooltip("Does this rotation work in local or world space")]
        protected bool localRotation;

        /// <summary>
        /// Linear velocity of object in units per second for each axis
        /// </summary>
        [SerializeField]
        [Tooltip("Linear velocity of object in units per second for each axis")]
        protected Vector3 linearVelocity;

        /// <summary>
        /// Does this velocity work in local or world space. If true, will translate in local space.
        /// If false will translate in world space.
        /// </summary>
        [SerializeField]
        [Tooltip("Does this translation work in local or world space.")]
        protected bool localTranslation;

        /// <summary>
        /// Rigidbody for this object
        /// </summary>
        protected new Rigidbody rigidbody;

        public void Start()
        {
            this.rigidbody = GetComponent<Rigidbody>();
            this.rigidbody.isKinematic = true;
        }

        public void FixedUpdate()
        {
            if (linearVelocity.magnitude > 0)
            {
                // move object by velocity
                Vector3 deltaPos = Time.fixedDeltaTime * linearVelocity;
                if (localTranslation && transform.parent != null)
                {
                    this.rigidbody.MovePosition(transform.parent.position + transform.localPosition + deltaPos);
                }
                else
                {
                    this.rigidbody.MovePosition(transform.position + deltaPos);
                }
            }

            if (angularVelocity.magnitude > 0)
            {
                // rotate object by rotation
                Quaternion deltaRotation = Quaternion.Euler(Time.fixedDeltaTime * angularVelocity);
                if (localRotation && transform.parent != null)
                {
                    this.rigidbody.MoveRotation(transform.parent.rotation * transform.localRotation * deltaRotation);
                }
                else
                {
                    this.rigidbody.MoveRotation(transform.rotation * deltaRotation);
                }
            }
        }
    }
}