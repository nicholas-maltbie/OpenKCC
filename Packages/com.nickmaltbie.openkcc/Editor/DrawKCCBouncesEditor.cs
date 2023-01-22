// Copyright (C) 2023 Nicholas Maltbie
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
using com.nickmaltbie.OpenKCC.Debug;
using nickmaltbie.OpenKCC.Utils;
using UnityEditor;
using UnityEngine;
using static nickmaltbie.OpenKCC.Utils.KCCUtils;

namespace nickmaltbie.OpenKCC.Editor
{
    /// <summary>
    /// Draw the bounces using the gizmos in unity. 
    /// </summary>
    public static class DrawKCCBouncesGizmos
    {
        [DrawGizmo(GizmoType.Selected | GizmoType.Active)]
        public static void DrawGizmoForMyScript(DrawKCCBounces source, GizmoType gizmoType)
        {
            Transform transform = source.gameObject.transform;

            Vector3 movement = transform.forward * source.movementDistance;
            IColliderCast colliderCast = source.GetComponent<IColliderCast>();

            // Get the bounces the player's movement would make
            var bounces = KCCUtils.GetBounces(
                    transform.position,
                    movement,
                    transform.rotation,
                    new KCCConfig
                    {
                        maxBounces = source.maxBounces,
                        pushDecay = 0,
                        verticalSnapUp = source.stepHeight,
                        stepUpDepth = source.stepDepth,
                        anglePower = 0,
                        canSnapUp = true,
                        up = Vector3.up,
                        colliderCast = colliderCast,
                        push = null
                    }).ToList();

            int bounce = 0;
            _ = transform.position;

            foreach (KCCBounce bounceData in bounces)
            {
                Color orderColor = source.bounceColors[bounce > 0 ? 0 : bounce % source.bounceColors.Length];

                if (bounceData.action == MovementAction.Move ||
                    bounceData.action == MovementAction.Bounce ||
                    bounceData.action == MovementAction.SnapUp)
                {
                    if (bounce == 0 && !source.drawInitialCollider)
                    {
                        // do nothing
                    }
                    else
                    {
                        colliderCast.DrawMeshGizmo(
                            new Color(orderColor.r, orderColor.g, orderColor.b, source.outlineAlpha),
                            new Color(orderColor.r, orderColor.g, orderColor.b, source.fillAlpha),
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
                else if (bounceData.action == MovementAction.Bounce ||
                    bounceData.action == MovementAction.SnapUp)
                {
                    Gizmos.color = orderColor;
                    Gizmos.DrawLine(bounceData.initialPosition, bounceData.finalPosition);

                    if (source.drawOverlapColliders)
                    {
                        colliderCast.DrawMeshGizmo(
                            new Color(source.overlapColor.r, source.overlapColor.g, source.overlapColor.b, source.outlineAlpha),
                            new Color(source.overlapColor.r, source.overlapColor.g, source.overlapColor.b, source.fillAlpha),
                            bounceData.initialPosition + bounceData.initialMomentum,
                            transform.rotation);
                        Gizmos.color = source.overlapColor;
                        Gizmos.DrawLine(bounceData.finalPosition, bounceData.initialPosition + bounceData.initialMomentum);
                    }
                }
                else if (bounceData.action == MovementAction.Stop)
                {
                    colliderCast.DrawMeshGizmo(
                        new Color(orderColor.r, orderColor.g, orderColor.b, source.outlineAlpha),
                        new Color(orderColor.r, orderColor.g, orderColor.b, source.fillAlpha),
                        bounceData.finalPosition,
                        transform.rotation);
                }

                // If the type is a move, draw a capsule from start to end
                bounce++;
            }
        }
    }
}
