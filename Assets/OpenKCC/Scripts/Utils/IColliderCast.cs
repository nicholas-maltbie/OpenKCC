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
        /// Gets the bounds of the collider.
        /// </summary>
        /// <returns>Bounds of this collider object.</returns>
        Bounds GetBounds();

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

        /// <summary>
        /// Draw a given mesh with the gizmos in the world.
        /// </summary>
        /// <param name="outlineColor">Color to draw outline of the mesh.</param>
        /// <param name="fillColor">Color to draw the fill of the mesh.</param>
        /// <param name="position">Position to draw the mesh in.</param>
        /// <param name="rotation">Rotation to draw the mesh in.</param>
        void DrawMeshGizmo(Color outlineColor, Color fillColor, Vector3 position, Quaternion rotation);
    }
}
