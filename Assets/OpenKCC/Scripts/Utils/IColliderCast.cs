using System.Collections.Generic;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Abstraction to check for collisions and compute when this object would hit other objects.
    /// </summary>
    interface IColliderCast
    {
        /// <summary>
        /// Gets the list of objects overlapping with this object.
        /// </summary>
        /// <returns>The list of overlapping objects with this object.</returns>
        IEnumerable<Collider> GetOverlapping();

        /// <summary>
        /// Cast self and get the objects hit that exclude this object.
        /// </summary>
        /// <param name="direction">Direction to cast self collider.</param>
        /// <param name="distance">Distance to cast self collider.</param>
        /// <returns></returns>
        IEnumerable<RaycastHit> GetHits(Vector3 direction, float distance);

        /// <summary>
        /// Cast self in a given direction and get the first object hit.
        /// </summary>
        /// <param name="direction">Direction of the raycast.</param>
        /// <param name="distance">Maximum distance of raycast.</param>
        /// <param name="hit">First object hit and related information, will have a distance of Mathf.Infinity if none
        /// is found.</param>
        /// <returns>True if an object is hit within distance, false otherwise.</returns>
        bool CastSelf(Vector3 direction, float distance, out RaycastHit hit);

        /// <summary>
        /// Get the vector to push this object out of overlapping objects with a max distance.
        /// </summary>
        /// <param name="maxDistance">Maximum distance the player can be pushed</param>
        /// <returns>Direction to push the object, distance player was pushed.</returns>
        Vector3 PushOutOverlapping(float maxDistance);
    }
}
