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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace nickmaltbie.OpenKCC.CameraControls.Config
{
    [Serializable]
    public class CameraConfig
    {
        [Header("Input Actions")]

        /// <summary>
        /// Action to rotate camera.
        /// </summary>
        [FormerlySerializedAs("lookAction")]
        [Tooltip("Player look action to rotate camera.")]
        [SerializeField]
        public InputActionReference lookActionReference;

        /// <summary>
        /// Override for the look action of the network camera controller.
        /// </summary>
        private InputAction overrideLookAction;

        /// <summary>
        /// Gest or sets the look action associated with this network camera controller.
        /// </summary>
        public InputAction LookAction
        {
            get => overrideLookAction ?? lookActionReference?.action;
            set => overrideLookAction = value;
        }

        /// <summary>
        /// Zoom action to zoom in and out.
        /// </summary>
        [Tooltip("Player zoom action with camera.")]
        [FormerlySerializedAs("zoomAction")]
        [SerializeField]
        public InputActionReference zoomActionReference;

        /// <summary>
        /// Override for the zoom action of the network camera controller.
        /// </summary>
        private InputAction overrideZoomAction;

        /// <summary>
        /// Gest or sets the zoom action associated with this network camera controller.
        /// </summary>
        public InputAction ZoomAction
        {
            get => overrideZoomAction ?? zoomActionReference?.action;
            set => overrideZoomAction = value;
        }

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
        /// Time in seconds it takes to transition between opacity states.
        /// </summary>
        public float transitionTime = 0.1f;

        /// <summary>
        /// Objects to ignore when drawing raycast for camera.
        /// </summary>
        private List<GameObject> ignoreObjects = new List<GameObject>();

        /// <summary>
        /// Ignore objects for drawing raycast for camera.
        /// </summary>
        public List<GameObject> IgnoreObjects => ignoreObjects;

        /// <summary>
        /// Get the current distance of the camera from the player camera location.
        /// </summary>
        public float CameraDistance { get; internal set; }

        /// <summary>
        /// Source camera position in real world space, this is where the head of 
        /// the player would be, where the camera zooms out from
        /// </summary>
        public Vector3 CameraSource(Transform transform) => baseCameraOffset + transform.position;

        /// <summary>
        /// Add an object to the ignore list when ray casting camera position
        /// </summary>
        /// <param name="go"></param>
        public void AddIgnoreObject(GameObject go) => ignoreObjects.Add(go);

        /// <summary>
        /// Remove an object to the ignore list when raycasting camera position
        /// </summary>
        /// <param name="go"></param>
        public bool RemoveIgnoreObject(GameObject go) => ignoreObjects.Remove(go);

        /// <summary>
        /// Setup the camera config for a camera controller.
        /// </summary>
        /// <param name="go">Game object with attached collider.</param>
        public void Setup(GameObject go)
        {
            baseCameraOffset = cameraTransform == null ? Vector3.zero : cameraTransform.localPosition;
            currentDistance = Mathf.Clamp(currentDistance, minCameraDistance, maxCameraDistance);
            ignoreObjects.Add(go);
        }
    }
}
