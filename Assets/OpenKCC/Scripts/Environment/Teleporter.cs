using UnityEngine;

namespace nickmaltbie.OpenKCC.Environment
{
    /// <summary>
    /// This will teleport rigidbodies when they collide with this object
    /// </summary>
    public class Teleporter : MonoBehaviour
    {
        /// <summary>
        /// Position and rotation to which objects will be teleported when they collide with this object
        /// </summary>
        public Transform teleportLocation;

        /// <summary>
        /// Whenever something collides with this object, teleport it
        /// </summary>
        /// <param name="other">Other object that collided with this</param>
        public void OnTriggerEnter(Collider other)
        {
            // Check if the object has a character Collider
            CharacterController cc = other.gameObject.GetComponent<CharacterController>();

            // If the object has a character collider, temporarily disable it
            if (cc != null)
            {
                cc.enabled = false;
            }

            // Teleport the game object to the position and rotation of the teleport location
            other.gameObject.transform.position = teleportLocation.position;
            other.gameObject.transform.rotation = teleportLocation.rotation;

            // Re-enable the character collider if it was disabled
            if (cc != null)
            {
                cc.enabled = true;
            }
        }
    }
}
