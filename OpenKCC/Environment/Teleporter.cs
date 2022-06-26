// Copyright (C) 2022 Nicholas Maltbie
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
