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
    /// Wrapper for RaycastHit to IRaycastHit.
    /// </summary>
    public class RaycastHitWrapper : IRaycastHit
    {
        /// <summary>
        /// Wrapper for raycast hit object.
        /// </summary>
        private RaycastHit hit;

        /// <summary>
        /// Initialize a raycast hit wrapper for a given raycast hit event.
        /// </summary>
        /// <param name="hit">Raycast hit event.</param>
        public RaycastHitWrapper(RaycastHit hit)
        {
            this.hit = hit;
        }

        /// <inheritdoc/>
        public Collider collider => hit.collider;

        /// <inheritdoc/>
        public Vector3 point { get => hit.point; set => hit.point = value; }

        /// <inheritdoc/>
        public Vector3 normal { get => hit.normal; set => hit.normal = value; }

        /// <inheritdoc/>
        public Vector3 barycentricCoordinate { get => hit.barycentricCoordinate; set => hit.barycentricCoordinate = value; }

        /// <inheritdoc/>
        public float distance { get => hit.distance; set => hit.distance = value; }

        /// <inheritdoc/>
        public int triangleIndex => hit.triangleIndex;

        /// <inheritdoc/>
        public Vector2 textureCoord => hit.textureCoord;

        /// <inheritdoc/>
        public Vector2 textureCoord2 => hit.textureCoord2;

        /// <inheritdoc/>
        public Transform transform => hit.transform;

        /// <inheritdoc/>
        public Rigidbody rigidbody => hit.rigidbody;

        /// <inheritdoc/>
        public ArticulationBody articulationBody => hit.articulationBody;
    }
}
