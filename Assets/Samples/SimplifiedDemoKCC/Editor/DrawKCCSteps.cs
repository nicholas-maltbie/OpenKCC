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

using System.Linq;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Editor;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;
using static nickmaltbie.OpenKCC.Utils.KCCUtils;

namespace nickmaltbie.OpenKCC.Demo.Editor
{
    /// <summary>
    /// Draw the step based Character Movement.
    /// </summary>
    [RequireComponent(typeof(KCCStateMachine))]
    public class DrawKCCSteps : MonoBehaviour
    {
        /// <summary>
        /// Kinematic character controller reference.
        /// </summary>
        private KCCStateMachine kcc;

        /// <summary>
        /// Collider cast for checking movement and collision of the character controller.
        /// </summary>
        private CapsuleColliderCast colliderCast;

        /// <summary>
        /// Should a collider at the position of the character be drawn?
        /// </summary>
        [SerializeField]
        [Tooltip("Should the initial collider at the current position be drawn.")]
        public bool drawInitialCollider = false;

        /// <summary>
        /// Should colliders for overlapping objects be drawn?
        /// </summary>
        [SerializeField]
        [Tooltip("Should the overlapping colliders be drawn.")]
        public bool drawOverlapColliders = true;

        /// <summary>
        /// Should the colliders be drawn when the player is not moving
        /// </summary>
        [SerializeField]
        [Tooltip("Should the colliders be drawn when the player is not moving.")]
        public bool drawWhenStill = true;

        /// <summary>
        /// Total distance to move player when drawing colliders.
        /// </summary>
        [SerializeField]
        [Tooltip("How far of a movement should be drawn.")]
        public float movementDistance = 5.0f;

        /// <summary>
        /// Number of steps in the step based movement.
        /// </summary>
        [SerializeField]
        [Tooltip("Number of steps to highlight in step based movement.")]
        public int steps = 4;

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

        /// <summary>
        /// Colors for various iterations of drawing, will loop back to start if this runs out.
        /// </summary>
        [SerializeField]
        [Tooltip("Colors for various iterations of drawing, will loop back to start if this runs out.")]
        public Color[] bounceColors = new Color[]{
            new Color( 22 / 255f, 231 / 255f,  49 / 255f),
            new Color(  0 / 255f, 233 / 255f, 110 / 255f),
            new Color(  0 / 255f, 232 / 255f, 156 / 255f),
            new Color(  0 / 255f, 231 / 255f, 195 / 255f),
            new Color(  0 / 255f, 227 / 255f, 225 / 255f),
            new Color(  0 / 255f, 222 / 255f, 245 / 255f),
            new Color(  0 / 255f, 216 / 255f, 255 / 255f),
            new Color( 59 / 255f, 209 / 255f, 255 / 255f),
            new Color(125 / 255f, 201 / 255f, 248 / 255f),
            new Color(160 / 255f, 195 / 255f, 234 / 255f),
        };

        /// <summary>
        /// Color of overlapping objects for the character collider gizmo.
        /// </summary>
        [SerializeField]
        [Tooltip("Color of overlapping objects for the character collider gizmo.")]
        public Color overlapColor = Color.red;

        /// <summary>
        /// Color of steps not used.
        /// </summary>
        [SerializeField]
        [Tooltip("Color of steps not used.")]
        public Color ghostColor = Color.white;

        public void Start()
        {
            kcc = GetComponent<KCCStateMachine>();
            colliderCast = GetComponent<CapsuleColliderCast>();
        }

        public void OnDrawGizmos()
        {
            if (kcc == null)
            {
                kcc = GetComponent<KCCStateMachine>();
            }

            if (colliderCast == null)
            {
                colliderCast = GetComponent<CapsuleColliderCast>();
            }

            /*Vector3 movement = kcc.GetProjectedMovement().normalized * movementDistance;

            if (movement.magnitude == 0)
            {
                if (!drawWhenStill)
                {
                    return;
                }

                movement = kcc.GetProjectedMovement(Vector3.forward).normalized * movementDistance;
            }*/

            Vector3 movement = kcc.GetProjectedMovement(Vector3.forward).normalized * movementDistance;

            // Get Check each step forward
            Vector3 stepOffset = movement / steps;

            int step = drawInitialCollider ? 0 : 1;
            bool overlap = false;

            while (step <= steps)
            {
                Color orderColor = bounceColors[step % bounceColors.Length];

                // Get the position of character after they would move this step
                Vector3 target = transform.position + stepOffset * step + Epsilon * kcc.config.Up;

                if (overlap)
                {
                    if (drawOverlapColliders)
                    {
                        colliderCast.DrawMeshGizmo(
                            new Color(ghostColor.r, ghostColor.g, ghostColor.b, outlineAlpha),
                            new Color(ghostColor.r, ghostColor.g, ghostColor.b, fillAlpha),
                            target,
                            transform.rotation);
                    }
                }
                else
                {
                    overlap = colliderCast.GetOverlapping(target, transform.rotation).Any();

                    if (!overlap)
                    {
                        colliderCast.DrawMeshGizmo(
                            new Color(orderColor.r, orderColor.g, orderColor.b, outlineAlpha),
                            new Color(orderColor.r, orderColor.g, orderColor.b, fillAlpha),
                            target,
                            transform.rotation);
                    }
                    else if (overlap && drawOverlapColliders)
                    {
                        colliderCast.DrawMeshGizmo(
                            new Color(overlapColor.r, overlapColor.g, overlapColor.b, outlineAlpha),
                            new Color(overlapColor.r, overlapColor.g, overlapColor.b, fillAlpha),
                            target,
                            transform.rotation);
                    }
                }

                step++;
            }
        }
    }
}
