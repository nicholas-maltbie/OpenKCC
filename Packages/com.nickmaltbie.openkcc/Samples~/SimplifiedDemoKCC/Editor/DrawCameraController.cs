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
using nickmaltbie.OpenKCC.CameraControls;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Demo.Editor
{
    /// <summary>
    /// Draw the DrawCameraController state of player using the gizmos in unity. 
    /// </summary>
    [RequireComponent(typeof(CameraController))]
    public class DrawCameraController : MonoBehaviour
    {
        /// <summary>
        /// Camera controller component
        /// </summary>
        private CameraController cameraController;

        private List<GameObject> selfCollider;

        /// <summary>
        /// Should offset from player position be drawn.
        /// </summary>
        [SerializeField]
        [Tooltip("Should offset from player position be drawn.")]
        private bool drawOffset = false;

        /// <summary>
        /// Color to draw offset from player position.
        /// </summary>
        [SerializeField]
        [Tooltip("Color to draw offset from player position.")]
        private Color offsetColor = Color.cyan;

        /// <summary>
        /// Should raycast from camera head position to camera final position be shown.
        /// </summary>
        [SerializeField]
        [Tooltip("Should raycast from camera head position to camera final position be shown.")]
        private bool drawCameraRaycast = true;

        /// <summary>
        /// When drawing raycast, show overlap be shown.
        /// </summary>
        [SerializeField]
        [Tooltip("When drawing raycast, show overlap be shown.")]
        private bool showRaycastOverlap = true;

        /// <summary>
        /// Color to draw raycast line for camera checks.
        /// </summary>
        [SerializeField]
        [Tooltip("Color to draw raycast line for camera checks.")]
        private Color raycastColor = Color.blue;

        /// <summary>
        /// Color to draw overlap space for camera raycast checks.
        /// </summary>
        [SerializeField]
        [Tooltip("Color to draw overlap space for camera raycast checks.")]
        private Color overlapColor = Color.yellow;

        /// <summary>
        /// Radius of sphere to show camera position.
        /// </summary>
        [SerializeField]
        [Tooltip("Radius of sphere to show camera position.")]
        private float sphereRadius = 0.05f;

        /// <summary>
        /// Draw frustrum to show camera view.
        /// </summary>
        [SerializeField]
        [Tooltip("Draw frustrum to show camera view.")]
        private bool drawFrustrum = true;

        /// <summary>
        /// Draw raycast back from camera to player checking hitting collider.
        /// </summary>
        [SerializeField]
        [Tooltip("Draw raycast back from camera to player checking hitting collider.")]
        private bool drawBackRaycast = false;

        public void Start()
        {
            cameraController = GetComponent<CameraController>();
        }

        public void OnDrawGizmos()
        {
            if (cameraController == null)
            {
                cameraController = GetComponent<CameraController>();
            }

            if (selfCollider == null)
            {
                selfCollider = new List<GameObject>(new[] { gameObject });
            }

            Vector3 cameraSource = transform.position + cameraController.config.baseCameraOffset;

            if (drawOffset)
            {
                Gizmos.color = offsetColor;
                Gizmos.DrawLine(transform.position, cameraSource);
            }

            var rotation = Quaternion.Euler(cameraController.Pitch, cameraController.Yaw, 0);
            _ = cameraController.config.currentDistance;

            Vector3 cameraDirection = rotation * Vector3.back * cameraController.config.currentDistance;

            // Draw a line from our camera source in the camera direction. If the line hits anything that isn't us
            // Limit the distance by how far away that object is
            // If we hit something
            if (PhysicsUtils.SphereCastFirstHitIgnore(selfCollider, cameraSource, 0.01f, cameraDirection, cameraDirection.magnitude,
                ~0, QueryTriggerInteraction.Ignore, out RaycastHit hit))
            {
                // limit the movement by that hit
                cameraDirection = cameraDirection.normalized * hit.distance;
            }

            bool hittingSelf = PhysicsUtils.SphereCastAllow(gameObject, cameraSource + cameraDirection, 0.01f, -cameraDirection.normalized,
                cameraDirection.magnitude, ~0, QueryTriggerInteraction.Ignore, out RaycastHit selfHit);
            _ = hittingSelf ? selfHit.distance : cameraDirection.magnitude;

            Vector3 cameraPos = cameraSource + cameraDirection;

            if (drawCameraRaycast)
            {
                Gizmos.color = raycastColor;
                Gizmos.DrawRay(cameraSource, cameraDirection);

                if (showRaycastOverlap)
                {
                    Gizmos.color = overlapColor;
                    Vector3 overlap = (rotation * Vector3.back * cameraController.config.currentDistance) - cameraDirection;
                    Gizmos.DrawRay(cameraPos, overlap);
                }
            }

            if (drawBackRaycast)
            {
                Vector3 hitPoint = selfHit.point;
                if (hitPoint == Vector3.zero)
                {
                    hitPoint = cameraPos;
                }

                if (hittingSelf)
                {
                    Gizmos.color = raycastColor;
                    Gizmos.DrawLine(cameraPos, hitPoint);
                }

                if (showRaycastOverlap)
                {
                    Vector3 startPoint = hitPoint;
                    Gizmos.color = overlapColor;
                    Gizmos.DrawLine(startPoint, cameraSource);
                }
            }

            if (drawBackRaycast || drawCameraRaycast)
            {
                Gizmos.DrawSphere(cameraPos, sphereRadius);
            }

            if (drawFrustrum)
            {
                Gizmos.DrawFrustum(Camera.main.transform.position, Camera.main.fieldOfView, Camera.main.farClipPlane, Camera.main.nearClipPlane, Camera.main.aspect);
            }
        }
    }
}
