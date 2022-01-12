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

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Interface to hold the data for a controller collider hit event.
    /// Wrapping this in an interface makes the testing of a controller collider hit
    /// easier to manage.
    /// </summary>
    public interface IControllerColliderHit
    {
        /// <summary>
        /// The collider that was hit by the controller.
        /// </summary>
        Collider collider { get; }

        /// <summary>
        /// The rigidbody that was hit by the controller.
        /// </summary>
        Rigidbody rigidbody { get; }

        /// <summary>
        /// The game object that was hit by the controller.
        /// </summary>
        GameObject gameObject { get; }

        /// <summary>
        /// The transform that was hit by the controller.
        /// </summary>
        Transform transform { get; }

        /// <summary>
        /// The impact point in world space.
        /// </summary>
        Vector3 point { get; }

        /// <summary>
        /// The normal of the surface we collided with in world space.
        /// </summary>
        Vector3 normal { get; }

        /// <summary>
        /// The direction the CharacterController was moving in when the collision occurred.
        /// </summary>
        Vector3 moveDirection { get; }

        /// <summary>
        /// How far the character has travelled until it hit the collider.
        /// </summary>
        float moveLength { get; }
    }

    public class KinematicCharacterControllerHit : IControllerColliderHit
    {

        /// <summary>
        /// Create a hit event for a kinematic character controller
        /// </summary>
        /// <param name="hit">the collider hit by this character controller</param>
        /// <param name="rigidbody">the rigidbody hit by this character controller</param>
        /// <param name="gameObject">the game object that was hit by this character controller</param>
        /// <param name="transform">the transform of the game object hit by this character controller</param>
        /// <param name="point">the point that this character controller collided with the object</param>
        /// <param name="normal">the normal vector of the hit between the character controller and the hit object</param>
        /// <param name="moveDirection">the direction the player was moving</param>
        /// <param name="moveLength">the distance the player was moving</param>
        public KinematicCharacterControllerHit(
            Collider hit, Rigidbody rigidbody, GameObject gameObject,
            Transform transform, Vector3 point, Vector3 normal, Vector3 moveDirection,
            float moveLength)
        {
            collider = hit;
            this.rigidbody = rigidbody;
            this.gameObject = gameObject;
            this.transform = transform;
            this.point = point;
            this.normal = normal;
            this.moveDirection = moveDirection;
            this.moveLength = moveLength;
        }

        public Collider collider { get; private set; }
        public Rigidbody rigidbody { get; private set; }
        public GameObject gameObject { get; private set; }
        public Transform transform { get; private set; }
        public Vector3 point { get; private set; }
        public Vector3 normal { get; private set; }
        public Vector3 moveDirection { get; private set; }
        public float moveLength { get; private set; }
    }

    /// <summary>
    /// Wrapper for the data in a ControllerColliderHit
    /// </summary>
    public class ControllerColliderHitWrapper : IControllerColliderHit
    {
        private ControllerColliderHit colliderHit;
        private Vector3 moveVector;

        public ControllerColliderHitWrapper(ControllerColliderHit colliderHit, Vector3 moveVector)
        {
            this.colliderHit = colliderHit;
            this.moveVector = moveVector;
        }

        public Collider collider => colliderHit.collider;
        public Rigidbody rigidbody => colliderHit.rigidbody;
        public GameObject gameObject => colliderHit.gameObject;
        public Transform transform => colliderHit.transform;
        public Vector3 point => colliderHit.point;
        public Vector3 normal => colliderHit.normal;
        public Vector3 moveDirection => moveVector.normalized;
        public float moveLength => moveVector.magnitude;
    }
}
