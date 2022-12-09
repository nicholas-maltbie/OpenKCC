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
using System.Diagnostics.CodeAnalysis;
using nickmaltbie.OpenKCC.CameraControls.Config;
using nickmaltbie.TestUtilsUnity;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.CameraControls
{
    /// <summary>
    /// Basic hybrid first and third person camera controller.
    /// </summary>
    public class CameraController : MonoBehaviour, ICameraControls, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Unity service associated with this game object for testing.
        /// </summary>
        public IUnityService unityService = UnityService.Instance;

        /// <summary>
        /// Current version fo the serialization.
        /// </summary>
        public const string CurrentSerializationVersion = "v1.0.0";

        /// <summary>
        /// Camera config associated wit this camera controller.
        /// </summary>
        [SerializeField]
        public CameraConfig config = new CameraConfig();

        /// <summary>
        /// Version of serialization for the KCC state machine.
        /// Used to migrate to newer version if using older version.
        /// </summary>
        [HideInInspector]
        [SerializeField]
        internal string serializationVersion;

        /// <inheritdoc/>
        public float PreviousOpacity { get; set; }

        /// <inheritdoc/>
        public float Pitch { get; set; }

        /// <inheritdoc/>
        public float Yaw { get; set; }

        public void Start()
        {
            config.Setup(gameObject);
        }

        public void Update()
        {
            CameraUtils.UpdateCameraController(config, gameObject, this, unityService.deltaTime);
        }

        #region Obsolete fields

        public const string ObselteMessage = "Obselte camera field, going to be removed and replaced with CameraConfig in future update";

        [Header("Input Actions")]

        /// <summary>
        /// Action to rotate camera.
        /// </summary>
        [Tooltip("Player look action to rotate camera.")]
        [SerializeField]
        [HideInInspector]
        [Obsolete(ObselteMessage)]
        public InputActionReference lookAction;

        /// <summary>
        /// Zoom action to zoom in and out.
        /// </summary>
        [Tooltip("Player zoom action with camera.")]
        [SerializeField]
        [HideInInspector]
        [Obsolete(ObselteMessage)]
        public InputActionReference zoomAction;

        [Header("Rotation Bounds")]

        /// <summary>
        /// Maximum pitch for rotating character camera in degrees
        /// </summary>
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public float maxPitch = 90;

        /// <summary>
        /// Minimum pitch for rotating character camera in degrees
        /// </summary>
        [Obsolete(ObselteMessage)]
        public float minPitch = -90;

        /// <summary>
        /// Rotation rate of camera in degrees per second per one unit of axis movement
        /// </summary>
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public float rotationRate = 180;

        /// <summary>
        /// Transform holding camera position and rotation data
        /// </summary>
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public Transform cameraTransform;

        /// <summary>
        /// Camera offset from character center
        /// </summary>   
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public Vector3 baseCameraOffset;

        /// <summary>
        /// Minimum distance (closest zoom) of player camera
        /// </summary>
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public float minCameraDistance = 0.0f;

        /// <summary>
        /// Maximum distance (farthest zoom) of player camera
        /// </summary>
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public float maxCameraDistance = 4.0f;

        /// <summary>
        /// Current distance of the camera from the player position
        /// </summary>
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public float currentDistance;

        /// <summary>
        /// Zoom distance change in units per second
        /// </summary>
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public float zoomSpeed = 1.0f;

        /// <summary>
        /// What can the camera collide with
        /// </summary>
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public LayerMask cameraRaycastMask = ~0;

        /// <summary>
        /// Distance in which the third person character will be completely transparent and only cast shadows
        /// </summary>
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public float shadowOnlyDistance = 0.5f;

        /// <summary>
        /// Distance where the player object will dither but still be visible
        /// </summary>
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public float ditherDistance = 1.0f;

        /// <summary>
        /// Base object where all the third person character is stored.
        /// </summary>
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public GameObject thirdPersonCharacterBase;

        /// <summary>
        /// Time in seconds it takes to transition between opacity states
        /// </summary>
        [Obsolete(ObselteMessage)]
        [HideInInspector]
        public float transitionTime = 0.1f;

        /// <summary>
        /// Get the current distance of the camera from the player camera location
        /// </summary>
        [Obsolete(ObselteMessage)]
        [ExcludeFromCodeCoverage]
        public float CameraDistance { get; }

        /// <summary>
        /// Source camera position in real world space, this is where the head of 
        /// the player would be, where the camera zooms out from
        /// </summary>
        [Obsolete(ObselteMessage)]
        [ExcludeFromCodeCoverage]
        public Vector3 CameraSource => baseCameraOffset + transform.position;

        #endregion Obsolete fields

        #region Parse Depreciated Configurations of KCC StateMachine.
        /// <summary>
        /// Persist volatile fields to serialized data before
        /// unity serializes the object.
        /// </summary>
        public void OnBeforeSerialize()
        {
            // Do nothing for now...
        }

        /// <summary>
        /// Allow for parsing depreciated fields into the newer
        /// format.
        /// </summary>
        public void OnAfterDeserialize()
        {
            // If CameraConfig config is uninitialized, read in values from previous
            // depreciated fields
            if (string.IsNullOrEmpty(serializationVersion))
            {
                // Disabling warnings because this method is to convert from obsolete
                // fields to current structure.
#pragma warning disable CS0618 // Type or member is obsolete
                config.lookActionReference = lookAction;
                config.zoomActionReference = zoomAction;
                config.maxPitch = maxPitch;
                config.minPitch = minPitch;
                config.rotationRate = rotationRate;
                config.cameraTransform = cameraTransform;
                config.baseCameraOffset = baseCameraOffset;
                config.minCameraDistance = minCameraDistance;
                config.maxCameraDistance = maxCameraDistance;
                config.currentDistance = currentDistance;
                config.zoomSpeed = zoomSpeed;
                config.cameraRaycastMask = cameraRaycastMask;
                config.shadowOnlyDistance = shadowOnlyDistance;
                config.ditherDistance = ditherDistance;
                config.thirdPersonCharacterBase = thirdPersonCharacterBase;
                config.transitionTime = transitionTime;
                serializationVersion = CurrentSerializationVersion;
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }

        #endregion
    }
}
