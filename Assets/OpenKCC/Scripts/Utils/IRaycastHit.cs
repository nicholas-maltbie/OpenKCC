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
    /// Interface to represent data of raycast hit event.
    /// </summary>
    public interface IRaycastHit
    {
        /// <summary>
        /// The Collider that was hit.
        /// </summary>
        /// <value></value>
        Collider collider { get; }

        /// <summary>
        /// The impact point in world space where the ray hit the collider.
        /// </summary>
        /// <value></value>
        Vector3 point { get; set; }

        /// <summary>
        /// The normal of the surface the ray hit.
        /// </summary>
        /// <value></value>
        Vector3 normal { get; set; }

        /// <summary>
        /// The barycentric coordinate of the triangle we hit.
        /// </summary>
        /// <value></value>
        Vector3 barycentricCoordinate { get; set; }

        /// <summary>
        /// The distance from the ray's origin to the impact point.
        /// </summary>
        /// <value></value>
        float distance { get; set; }

        /// <summary>
        /// The index of the triangle that was hit.
        /// </summary>
        /// <value></value>
        int triangleIndex { get; }

        /// <summary>
        /// The uv texture coordinate at the collision location.
        /// </summary>
        /// <value></value>
        Vector2 textureCoord { get; }

        /// <summary>
        /// The secondary uv texture coordinate at the impact point.
        /// </summary>
        /// <value></value>
        Vector2 textureCoord2 { get; }

        /// <summary>
        /// The Transform of the rigidbody or collider that was hit.
        /// </summary>
        /// <value></value>
        Transform transform { get; }

        /// <summary>
        /// The Rigidbody of the collider that was hit. If the collider is not attached to
        /// a rigidbody then it is null.
        /// </summary>
        /// <value></value>
        Rigidbody rigidbody { get; }

        /// <summary>
        /// The ArticulationBody of the collider that was hit. If the collider is not attached
        /// to an articulation body then it is null.
        /// </summary>
        /// <value></value>
        ArticulationBody articulationBody { get; }
    }
}
