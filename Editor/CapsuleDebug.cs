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
using UnityEditor;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Editor
{
    /// <summary>
    /// Debug functions for capsule drawing.
    /// </summary>
    public class CapsuleDebug
    {
        /// <summary>
        /// Draw solid capsule.
        /// </summary>
        /// <param name="_pos">Position to draw wire capsule (center of capsule)</param>
        /// <param name="_rot">Rotation to draw wire capsule</param>
        /// <param name="_radius">Radius of wire capsule</param>
        /// <param name="_height">Height of wire capsule</param>
        /// <param name="_color">Color of wire capsule</param>
        public static void DrawSolidCapsule(
            Vector3 _pos,
            Quaternion _rot,
            float _radius,
            float _height,
            Color _color = default(Color))
        {
            Gizmos.color = _color;
            Gizmos.DrawMesh(CapsuleMaker.CapsuleData(radius: _radius, depth: _height - _radius * 2), _pos, _rot);
        }

        /// <summary>
        /// Draws a wire capsule on the screen, helpful function found here on the unity stack exchange:
        /// https://answers.unity.com/questions/56063/draw-capsule-gizmo.html
        /// </summary>
        /// <param name="_pos">Position to draw wire capsule (center of capsule)</param>
        /// <param name="_rot">Rotation to draw wire capsule</param>
        /// <param name="_radius">Radius of wire capsule</param>
        /// <param name="_height">Height of wire capsule</param>
        /// <param name="_color">Color of wire capsule</param>
        public static void DrawWireCapsule(
            Vector3 _pos,
            Quaternion _rot,
            float _radius,
            float _height,
            Color _color = default(Color))
        {
            if (_color != default(Color))
            {
                Handles.color = _color;
            }

            var angleMatrix = Matrix4x4.TRS(_pos, _rot, Handles.matrix.lossyScale);
            using (new Handles.DrawingScope(angleMatrix))
            {
                float pointOffset = (_height - (_radius * 2)) / 2;

                //draw sideways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.left, Vector3.back, -180, _radius);
                Handles.DrawLine(new Vector3(0, pointOffset, -_radius), new Vector3(0, -pointOffset, -_radius));
                Handles.DrawLine(new Vector3(0, pointOffset, _radius), new Vector3(0, -pointOffset, _radius));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.left, Vector3.back, 180, _radius);
                //draw frontways
                Handles.DrawWireArc(Vector3.up * pointOffset, Vector3.back, Vector3.left, 180, _radius);
                Handles.DrawLine(new Vector3(-_radius, pointOffset, 0), new Vector3(-_radius, -pointOffset, 0));
                Handles.DrawLine(new Vector3(_radius, pointOffset, 0), new Vector3(_radius, -pointOffset, 0));
                Handles.DrawWireArc(Vector3.down * pointOffset, Vector3.back, Vector3.left, -180, _radius);
                //draw center
                Handles.DrawWireDisc(Vector3.up * pointOffset, Vector3.up, _radius);
                Handles.DrawWireDisc(Vector3.down * pointOffset, Vector3.up, _radius);
            }
        }
    }
}
