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

using System.Collections.Generic;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Abstraction to check for collisions and compute when this object would hit other objects.
    /// </summary>
    public interface IColliderCast
    {
        /// <summary>
        /// Gets the bottom of the bounds of the collider.
        /// </summary>
        /// <param name="position">Position of the object when it is being checked.</param>
        /// <param name="rotation">Rotation of the objecting when it is being checked.</param>
        /// <returns>Bottom of the bounds of the collider.</returns>
        Vector3 GetBottom(Vector3 position, Quaternion rotation);

        /// <summary>
        /// Gets the list of objects overlapping with this object.
        /// </summary>
        /// <param name="position">Position of the object when it is being checked.</param>
        /// <param name="rotation">Rotation of the objecting when it is being checked.</param>
        /// <returns>The list of overlapping objects with this object.</returns>
        IEnumerable<Collider> GetOverlapping(Vector3 position, Quaternion rotation);

        /// <summary>
        /// Cast self and get the objects hit that exclude this object.
        /// </summary>
        /// <param name="direction">Direction to cast self collider.</param>
        /// <param name="distance">Distance to cast self collider.</param>
        /// <param name="position">Position of the object when it is being raycast.</param>
        /// <param name="rotation">Rotation of the objecting when it is being raycast.</param>
        /// <returns>List of objects this hits when it is being raycast</returns>
        IEnumerable<RaycastHit> GetHits(Vector3 position, Quaternion rotation, Vector3 direction, float distance);

        /// <summary>
        /// Cast self in a given direction and get the first object hit.
        /// </summary>
        /// <param name="position">Position of the object when it is being raycast.</param>
        /// <param name="rotation">Rotation of the objecting when it is being raycast.</param>
        /// <param name="direction">Direction of the raycast.</param>
        /// <param name="distance">Maximum distance of raycast.</param>
        /// <param name="hit">First object hit and related information, will have a distance of Infinity if none
        /// is found.</param>
        /// <param name="skinWidth">Width of skin of object to use when casting a hit. Essentially buffer
        /// space around the edge of the object.</param>
        /// <returns>True if an object is hit within distance, false otherwise.</returns>
        bool CastSelf(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out IRaycastHit hit);

        /// <summary>
        /// Get the vector to push this object out of overlapping objects with a max distance.
        /// </summary>
        /// <param name="position">Position of the object when it is being raycast.</param>
        /// <param name="rotation">Rotation of the objecting when it is being raycast.</param>
        /// <param name="maxDistance">Maximum distance the player can be pushed</param>
        /// <returns>Direction to push the object, distance player was pushed.</returns>
        Vector3 PushOutOverlapping(Vector3 position, Quaternion rotation, float maxDistance);

        /// <summary>
        /// Do a raycast in a given direction ignoring this object.
        /// </summary>
        /// <param name="source">Source point to check from.</param>
        /// <param name="direction">Direction to search for step.</param>
        /// <param name="distance">Distance to search for step ahead of player.</param>
        /// <param name="stepHit">Information bout step hit ahead.</param>
        /// <returns>Is something ahead hit.</returns>
        bool DoRaycastInDirection(Vector3 source, Vector3 direction, float distance, out IRaycastHit stepHit);
    }
}
