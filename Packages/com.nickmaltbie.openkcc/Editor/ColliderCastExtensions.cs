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

using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Editor
{
    /// <summary>
    /// Collider cast extensions for debug drawing.
    /// </summary>
    public static class ColliderCastExtensions
    {
        /// <summary>
        /// Draw a given mesh with the gizmos in the world.
        /// </summary>
        /// <param name="colliderCast">Collider cast to draw debug for.</param>
        /// <param name="outlineColor">Color to draw outline of the mesh.</param>
        /// <param name="fillColor">Color to draw the fill of the mesh.</param>
        /// <param name="position">Position to draw the mesh in.</param>
        /// <param name="rotation">Rotation to draw the mesh in.</param>
        public static void DrawMeshGizmo(
            this IColliderCast colliderCast,
            Color outlineColor,
            Color fillColor,
            Vector3 position,
            Quaternion rotation)
        {
            if (colliderCast is CapsuleColliderCast)
            {
                DrawCapsuleCollider(colliderCast as CapsuleColliderCast, outlineColor, fillColor, position, rotation);
            }
        }

        /// <summary>
        /// Draw capsule collider from a CapsuleColliderCast.
        /// </summary>
        /// <param name="capsuleColliderCast">CapsuleColliderCast data.</param>
        /// <param name="outlineColor">Color to draw outline of character with.</param>
        /// <param name="fillColor">Color to fill capsule with.</param>
        /// <param name="position">Position to draw capsule collider cast.</param>
        /// <param name="rotation">Rotation to draw capsule collider with.</param>
        public static void DrawCapsuleCollider(
            CapsuleColliderCast capsuleColliderCast,
            Color outlineColor,
            Color fillColor,
            Vector3 position,
            Quaternion rotation)
        {
            Gizmos.color = fillColor;
            Gizmos.DrawMesh(
                capsuleColliderCast.DebugCapsuleMesh,
                position + capsuleColliderCast.CapsuleCollider.center,
                rotation);
            CapsuleDebug.DrawWireCapsule(
                position + capsuleColliderCast.CapsuleCollider.center,
                rotation,
                capsuleColliderCast.CapsuleCollider.radius,
                capsuleColliderCast.CapsuleCollider.height,
                outlineColor);
        }
    }
}
