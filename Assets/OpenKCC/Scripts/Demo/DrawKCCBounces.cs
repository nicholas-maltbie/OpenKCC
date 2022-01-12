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
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;
using static nickmaltbie.OpenKCC.Utils.KCCUtils;

namespace nickmaltbie.OpenKCC.Demo
{
    /// <summary>
    /// Draw the bounces using the gizmos in unity. 
    /// </summary>
    [RequireComponent(typeof(KinematicCharacterController))]
    public class DrawKCCBounces : MonoBehaviour
    {
        private KinematicCharacterController kcc;

        private IColliderCast colliderCast;

        public bool drawInitialCollider = false;

        public bool drawOverlapColliders = true;

        public bool drawWhenStill = true;

        public float movementDistance = 5.0f;

        public float fillAlpha = 0.5f;

        public float outlineAlpha = 1.0f;

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

        public Color overlapColor = Color.red;

        public void Start()
        {
            kcc = GetComponent<KinematicCharacterController>();
            colliderCast = GetComponent<IColliderCast>();
        }

        public void OnDrawGizmos()
        {
            if (kcc == null)
            {
                kcc = GetComponent<KinematicCharacterController>();
            }

            if (colliderCast == null)
            {
                colliderCast = GetComponent<IColliderCast>();
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

            // Get the bounces the player's movement would make
            var bounces = new List<KCCBounce>(
                KCCUtils.GetBounces(
                    kcc.MaxBounces,
                    kcc.PushDecay,
                    kcc.VerticalSnapUp,
                    kcc.StepUpDepth,
                    kcc.AnglePower,
                    kcc.AttemptingJump,
                    kcc.CanSnapUp,
                    kcc.transform.position,
                    movement,
                    kcc.Up,
                    colliderCast,
                    null));

            int bounce = 0;
            _ = transform.position;
            ;

            foreach (KCCBounce bounceData in bounces)
            {

                Color orderColor = bounceColors[bounce % bounceColors.Length];

                if (bounceData.action == MovementAction.Move || bounceData.action == MovementAction.Bounce)
                {
                    if (bounce == 0 && !drawInitialCollider)
                    {
                        // do nothing
                    }
                    else
                    {
                        colliderCast.DrawMeshGizmo(
                            new Color(orderColor.r, orderColor.g, orderColor.b, outlineAlpha),
                            new Color(orderColor.r, orderColor.g, orderColor.b, fillAlpha),
                            bounceData.initialPosition,
                            transform.rotation);
                    }
                }

                // Draw the initial capsule
                if (bounceData.action == MovementAction.Move)
                {
                    Gizmos.color = orderColor;
                    Gizmos.DrawLine(bounceData.initialPosition, bounceData.finalPosition);
                }
                else if (bounceData.action == MovementAction.Bounce)
                {
                    Gizmos.color = orderColor;
                    Gizmos.DrawLine(bounceData.initialPosition, bounceData.finalPosition);

                    if (drawOverlapColliders)
                    {
                        colliderCast.DrawMeshGizmo(
                            new Color(overlapColor.r, overlapColor.g, overlapColor.b, outlineAlpha),
                            new Color(overlapColor.r, overlapColor.g, overlapColor.b, fillAlpha),
                            bounceData.initialPosition + bounceData.initialMomentum,
                            transform.rotation);
                        Gizmos.color = overlapColor;
                        Gizmos.DrawLine(bounceData.finalPosition, bounceData.initialPosition + bounceData.initialMomentum);
                    }
                }
                else if (bounceData.action == MovementAction.Stop)
                {
                    colliderCast.DrawMeshGizmo(
                        new Color(orderColor.r, orderColor.g, orderColor.b, outlineAlpha),
                        new Color(orderColor.r, orderColor.g, orderColor.b, fillAlpha),
                        bounceData.finalPosition,
                        transform.rotation);
                }

                // If the type is a move, draw a capsule from start to end
                bounce++;
            }
        }
    }
}
