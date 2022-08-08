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

using nickmaltbie.OpenKCC.Utils;

using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Basic hybrid first and third person camera controller.
    /// </summary>
    public class CameraController : MonoBehaviour, ICameraControls
    {
        [Header("Input Actions")]

        /// <summary>
        /// Action to rotate camera.
        /// </summary>
        [Tooltip("Player look action to rotate camera.")]
        [SerializeField]
        private InputActionReference lookAction;

        /// <summary>
        /// Zoom action to zoom in and out.
        /// </summary>
        [Tooltip("Player zoom action with camera.")]
        [SerializeField]
        private InputActionReference zoomAction;

        [Header("Rotation Bounds")]

        /// <summary>
        /// Maximum pitch for rotating character camera in degrees
        /// </summary>
        public float maxPitch = 90;

        /// <summary>
        /// Minimum pitch for rotating character camera in degrees
        /// </summary>
        public float minPitch = -90;

        /// <summary>
        /// Rotation rate of camera in degrees per second per one unit of axis movement
        /// </summary>
        public float rotationRate = 180;

        /// <summary>
        /// How much the character rotated about the vertical axis this frame
        /// </summary>
        public float frameRotation;

        /// <summary>
        /// Transform holding camera position and rotation data
        /// </summary>
        public Transform cameraTransform;

        /// <summary>
        /// Camera offset from character center
        /// </summary>        
        public Vector3 baseCameraOffset;

        /// <summary>
        /// Minimum distance (closest zoom) of player camera
        /// </summary>
        public float minCameraDistance = 0.0f;

        /// <summary>
        /// Maximum distance (farthest zoom) of player camera
        /// </summary>
        public float maxCameraDistance = 4.0f;

        /// <summary>
        /// Current distance of the camera from the player position
        /// </summary>
        public float currentDistance;

        /// <summary>
        /// Zoom distance change in units per second
        /// </summary>
        public float zoomSpeed = 1.0f;

        /// <summary>
        /// What can the camera collide with
        /// </summary>
        public LayerMask cameraRaycastMask = ~0;

        /// <summary>
        /// Distance in which the third person character will be completely transparent and only cast shadows
        /// </summary>
        public float shadowOnlyDistance = 0.5f;

        /// <summary>
        /// Distance where the player object will dither but still be visible
        /// </summary>
        public float ditherDistance = 1.0f;

        /// <summary>
        /// Base object where all the third person character is stored.
        /// </summary>
        public GameObject thirdPersonCharacterBase;

        /// <summary>
        /// Time in seconds it takes to transition between opacity states
        /// </summary>
        public float transitionTime = 0.1f;

        /// <summary>
        /// Previous player opacity for dithering
        /// </summary>
        private float previousOpacity = 0.0f;

        /// <summary>
        /// Change in yaw from mouse movement
        /// </summary>
        private float yawChange;

        /// <summary>
        /// Change in pitch from mouse movement
        /// </summary>
        private float pitchChange;

        /// <summary>
        /// Zoom change from mousewheel.
        /// </summary>
        private float zoomAxis;

        /// <summary>
        /// Objects to ignore when drawing raycast for camera
        /// </summary>
        private List<GameObject> ignoreObjects = new List<GameObject>();

        /// <summary>
        /// Get the current distance of the camera from the player camera location
        /// </summary>
        public float CameraDistance { get; private set; }

        /// <summary>
        /// Source camera position in real world space, this is where the head of 
        /// the player would be, where the camera zooms out from
        /// </summary>
        public Vector3 CameraSource => baseCameraOffset + transform.position;

        /// <summary>
        /// Add an object to the ignore list when raycasting camera position
        /// </summary>
        /// <param name="go"></param>
        public void AddIgnoreObject(GameObject go) => ignoreObjects.Add(go);

        /// <summary>
        /// Remove an object to the ignore list when raycasting camera position
        /// </summary>
        /// <param name="go"></param>
        public bool RemoveIgnoreObject(GameObject go) => ignoreObjects.Remove(go);

        public void Start()
        {
            baseCameraOffset = cameraTransform.localPosition;
            currentDistance = Mathf.Clamp(currentDistance, minCameraDistance, maxCameraDistance);
            ignoreObjects.Add(gameObject);
        }

        /// <summary>
        /// Raycast a line from the camera's base.
        /// </summary>
        /// <param name="maxDistance">Maximum distance to draw ray.</param>
        /// <param name="layerMask">Layer mask for raycast</param>
        /// <param name="queryTriggerInteraction">Query trigger interaction for raycast</param>
        /// <param name="hit">Object the raycast hits, if any.</param>
        /// <returns>True if an object is hit, false otherwise.</returns>
        public bool RaycastFromCameraBase(float maxDistance, LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction, out RaycastHit hit)
        {
            return PhysicsUtils.RaycastFirstHitIgnore(ignoreObjects, CameraSource, cameraTransform.forward, maxDistance,
                layerMask, queryTriggerInteraction, out hit);
        }

        /// <summary>
        /// Draw a spherecase from the camera's base.
        /// </summary>
        /// <param name="maxDistance">Maximum distance to draw sphere.</param>
        /// <param sphereRadius="sphereRadius">Radius of sphere when drawing</param>
        /// <param name="layerMask">Layer mask for spherecase</param>
        /// <param name="queryTriggerInteraction">Query trigger interaction for spherecase</param>
        /// <param name="hit">Object the spherecase hits, if any.</param>
        /// <returns>True if an object is hit, false otherwise.</returns>
        public bool SpherecastFromCameraBase(float maxDistance, LayerMask layerMask, float sphereRadius, QueryTriggerInteraction queryTriggerInteraction, out RaycastHit hit)
        {
            return PhysicsUtils.SphereCastFirstHitIgnore(ignoreObjects, CameraSource, sphereRadius, cameraTransform.forward, maxDistance,
                layerMask, queryTriggerInteraction, out hit);
        }

        /// <summary>
        /// Look action changes for camera movement
        /// </summary>
        public void OnLook(InputAction.CallbackContext context)
        {
            Vector2 look = context.ReadValue<Vector2>();
            look *= PlayerInputUtils.mouseSensitivity;
            yawChange = look.x;
            pitchChange = look.y;
        }

        /// <summary>
        /// Zoom camera in and out
        /// </summary>
        /// <param name="context"></param>
        public void OnZoom(InputAction.CallbackContext context)
        {
            zoomAxis = context.ReadValue<Vector2>().y;
        }

        /// <summary>
        /// Current desired pitch of the camera.
        /// </summary>
        private float pitch;

        /// <summary>
        /// Current desired yaw of the camera.
        /// </summary>
        private float yaw;

        /// <inheritdoc/>
        public float Pitch
        {
            get => pitch;
            private set => pitch = value;
        }

        /// <inheritdoc/>
        public float Yaw
        {
            get => yaw;
            private set => yaw = value;
        }

        public void Update()
        {
            float deltaTime = Time.deltaTime;

            float zoomChange = 0;
            // bound pitch between -180 and 180
            Pitch = (Pitch % 360 + 180) % 360 - 180;
            // Only allow rotation if player is allowed to move
            if (PlayerInputUtils.playerMovementState == PlayerInputState.Allow)
            {
                yawChange = rotationRate * deltaTime * yawChange;
                Yaw += yawChange;
                Pitch += rotationRate * deltaTime * -1 * pitchChange;
                zoomChange = zoomSpeed * deltaTime * -1 * zoomAxis;
            }
            // Clamp rotation of camera between minimum and maximum specified pitch
            Pitch = Mathf.Clamp(Pitch, minPitch, maxPitch);
            frameRotation = yawChange;
            // Change camera zoom by desired level
            // Bound the current distance between minimum and maximum
            currentDistance = Mathf.Clamp(currentDistance + zoomChange, minCameraDistance, maxCameraDistance);

            // Set the player's rotation to be that of the camera's yaw
            // transform.rotation = Quaternion.Euler(0, yaw, 0);
            // Set pitch to be camera's rotation
            cameraTransform.rotation = Quaternion.Euler(Pitch, Yaw, 0);

            // Set the local position of the camera to be the current rotation projected
            //   backwards by the current distance of the camera from the player
            Vector3 cameraDirection = -cameraTransform.forward * currentDistance;
            Vector3 cameraSource = CameraSource;

            // Draw a line from our camera source in the camera direction. If the line hits anything that isn't us
            // Limit the distance by how far away that object is
            // If we hit something
            if (PhysicsUtils.SphereCastFirstHitIgnore(ignoreObjects, cameraSource, 0.01f, cameraDirection, cameraDirection.magnitude,
                cameraRaycastMask, QueryTriggerInteraction.Ignore, out RaycastHit hit))
            {
                // limit the movement by that hit
                cameraDirection = cameraDirection.normalized * hit.distance;
            }

            CameraDistance = cameraDirection.magnitude;
            cameraTransform.position = cameraSource + cameraDirection;

            bool hittingSelf = PhysicsUtils.SphereCastAllow(gameObject, cameraSource + cameraDirection, 0.01f, -cameraDirection.normalized,
                cameraDirection.magnitude, ~0, QueryTriggerInteraction.Ignore, out RaycastHit selfHit);

            float actualDistance = hittingSelf ? selfHit.distance : cameraDirection.magnitude;

            if (thirdPersonCharacterBase != null)
            {
                thirdPersonCharacterBase.transform.localRotation = Quaternion.Euler(0, Yaw, 0);
                if (actualDistance < shadowOnlyDistance)
                {
                    MaterialUtils.RecursiveSetShadowCastingMode(thirdPersonCharacterBase, UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly);
                }
                else
                {
                    MaterialUtils.RecursiveSetShadowCastingMode(thirdPersonCharacterBase, UnityEngine.Rendering.ShadowCastingMode.On);
                }

                if (actualDistance > shadowOnlyDistance && actualDistance < ditherDistance)
                {
                    float newOpacity = (actualDistance - shadowOnlyDistance) / (ditherDistance - minCameraDistance);
                    float lerpPosition = transitionTime > 0 ? deltaTime * 1 / transitionTime : 1;
                    previousOpacity = Mathf.Lerp(previousOpacity, newOpacity, lerpPosition);
                    // Set opacity of character based on how close the camera is
                    MaterialUtils.RecursiveSetFloatProperty(thirdPersonCharacterBase, "_Opacity", previousOpacity);
                }
                else
                {
                    // Set opacity of character based on how close the camera is
                    MaterialUtils.RecursiveSetFloatProperty(thirdPersonCharacterBase, "_Opacity", 1);
                    previousOpacity = actualDistance > shadowOnlyDistance ? 1 : 0;
                }
            }

            Yaw %= 360;
        }

        public void OnEnable()
        {
            zoomAction.action.performed += OnZoom;
            lookAction.action.performed += OnLook;
        }

        public void OnDisable()
        {
            zoomAction.action.performed -= OnZoom;
            lookAction.action.performed -= OnLook;
        }
    }
}
