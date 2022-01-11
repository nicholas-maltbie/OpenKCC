using UnityEngine;

namespace nickmaltbie.OpenKCC.Environment.Pushable
{
    /// <summary>
    /// Pushable object that can be shoved with a given force
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Pushable : MonoBehaviour, IPushable
    {
        /// <summary>
        /// Object rigidbody for pushing by players.
        /// </summary>
        private Rigidbody objRigidbody;

        public void Awake()
        {
            objRigidbody = GetComponent<Rigidbody>();
        }

        /// <inheritdoc/>
        public void PushObject(Vector3 force, Vector3 point, int forceMode)
        {
            objRigidbody.AddForce(force, (ForceMode)forceMode);
        }
    }
}
