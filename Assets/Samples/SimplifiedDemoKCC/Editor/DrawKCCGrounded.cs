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

using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Editor;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Demo.Editor
{
    /// <summary>
    /// Draw the Grounded state of player using the gizmos in unity. 
    /// </summary>
    [RequireComponent(typeof(KCCStateMachine))]
    public class DrawKCCGrounded : MonoBehaviour
    {
        /// <summary>
        /// Kinematic character controller reference.
        /// </summary>
        private KCCStateMachine kcc;

        /// <summary>
        /// Collider cast for checking movement and collision of the character controller.
        /// </summary>
        private IColliderCast colliderCast;

        /// <summary>
        /// How far should the downward cast be made.
        /// </summary>
        public float groundCheckDistance = 1.0f;

        /// <summary>
        /// How far is considered grounded.
        /// </summary>
        public float groundedDistance = 0.0f;

        /// <summary>
        /// Color of player when standing on the ground.
        /// </summary>
        [SerializeField]
        [Tooltip("Color of player when standing on the ground.")]
        public Color groundedColor = new Color(22 / 255f, 231 / 255f, 49 / 255f);

        /// <summary>
        /// Color of of player when floating above ground.
        /// </summary>
        [SerializeField]
        [Tooltip("Color of of player when floating above ground.")]
        public Color floatingColor = Color.red;

        /// <summary>
        /// Alpha value for drawing the character collider fill.
        /// </summary>
        [SerializeField]
        [Tooltip("Alpha value for drawing the character collider fill.")]
        [Range(0, 1)]
        public float fillAlpha = 0.5f;

        /// <summary>
        /// Alpha value for drawing the character collider outline.
        /// </summary>
        [SerializeField]
        [Tooltip("Alpha value for drawing the character collider outline.")]
        [Range(0, 1)]
        public float outlineAlpha = 1.0f;

        public void Start()
        {
            kcc = GetComponent<KCCStateMachine>();
            colliderCast = GetComponent<IColliderCast>();
        }

        public void OnDrawGizmos()
        {
            if (kcc == null)
            {
                kcc = GetComponent<KCCStateMachine>();
            }

            if (colliderCast == null)
            {
                colliderCast = GetComponent<IColliderCast>();
            }

            bool hitGround = colliderCast.CastSelf(
                transform.position,
                transform.rotation,
                Vector3.down,
                groundCheckDistance,
                out IRaycastHit hit);

            bool isGrounded = hitGround && hit.distance <= groundedDistance;

            Color castColor = isGrounded ? groundedColor : floatingColor;
            Vector3 finalPosition = transform.position + (hitGround ? hit.distance : groundCheckDistance) * Vector3.down;

            colliderCast.DrawMeshGizmo(
                new Color(castColor.r, castColor.g, castColor.b, outlineAlpha),
                new Color(castColor.r, castColor.g, castColor.b, fillAlpha),
                finalPosition,
                transform.rotation);
        }
    }
}
