﻿// Copyright (C) 2022 Nicholas Maltbie
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
using nickmaltbie.TestUtilsUnity;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Basic hybrid first and third person camera controller.
    /// </summary>
    public class NetworkCameraController : NetworkBehaviour, ICameraControls
    {
        public IUnityService unityService = UnityService.Instance;

        [Header("Input Actions")]

        /// <summary>
        /// Action to rotate camera.
        /// </summary>
        [Tooltip("Player look action to rotate camera.")]
        [SerializeField]
        public InputActionReference lookAction;

        /// <summary>
        /// Zoom action to zoom in and out.
        /// </summary>
        [Tooltip("Player zoom action with camera.")]
        [SerializeField]
        public InputActionReference zoomAction;

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
        public float CameraDistance { get; internal set; }

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
        /// Current desired pitch of the camera.
        /// </summary>
        private NetworkVariable<float> pitch = new NetworkVariable<float>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner);

        /// <summary>
        /// Current desired yaw of the camera.
        /// </summary>
        private NetworkVariable<float> yaw = new NetworkVariable<float>(
            readPerm: NetworkVariableReadPermission.Everyone,
            writePerm: NetworkVariableWritePermission.Owner);

        /// <inheritdoc/>
        public float Pitch
        {
            get => pitch.Value;
            private set => pitch.Value = value;
        }

        /// <inheritdoc/>
        public float Yaw
        {
            get => yaw.Value;
            private set => yaw.Value = value;
        }

        public void Update()
        {
            if (IsOwner)
            {
                Vector2 look = lookAction.action.ReadValue<Vector2>();
                look *= PlayerInputUtils.mouseSensitivity;
                zoomAxis = zoomAction.action.ReadValue<Vector2>().y;
                yawChange = look.x;
                pitchChange = look.y;

                // bound pitch between -180 and 180
                float zoomChange = 0;
                Pitch = (Pitch % 360 + 180) % 360 - 180;

                // Only allow rotation if player is allowed to move
                if (PlayerInputUtils.playerMovementState == PlayerInputState.Allow)
                {
                    yawChange = rotationRate * unityService.deltaTime * yawChange;
                    Yaw += yawChange;
                    Pitch += rotationRate * unityService.deltaTime * -1 * pitchChange;
                    zoomChange = zoomSpeed * unityService.deltaTime * -1 * zoomAxis;
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

                if (thirdPersonCharacterBase != null)
                {
                    UpdateThirdPersonCamera(cameraDirection);
                }

                Yaw %= 360;
            }
            else if (thirdPersonCharacterBase != null)
            {
                thirdPersonCharacterBase.transform.localRotation = Quaternion.Euler(0, Yaw, 0);
            }
        }

        public void UpdateThirdPersonCamera(Vector3 cameraDirection)
        {
            bool hittingSelf = PhysicsUtils.SphereCastAllow(gameObject, CameraSource + cameraDirection, 0.01f, -cameraDirection.normalized,
                cameraDirection.magnitude, ~0, QueryTriggerInteraction.Ignore, out RaycastHit selfHit);
            float actualDistance = hittingSelf ? selfHit.distance : cameraDirection.magnitude;
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
                float lerpPosition = transitionTime > 0 ? unityService.deltaTime * 1 / transitionTime : 1;
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
    }
}
