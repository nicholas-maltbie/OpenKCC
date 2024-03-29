﻿// Copyright (C) 2023 Nicholas Maltbie
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

using nickmaltbie.OpenKCC.CameraControls.Config;
using nickmaltbie.TestUtilsUnity;
using UnityEngine;

namespace nickmaltbie.OpenKCC.CameraControls
{
    /// <summary>
    /// Basic hybrid first and third person camera controller.
    /// </summary>
    public class CameraController : MonoBehaviour, ICameraControls, IManagedCamera
    {
        /// <summary>
        /// Unity service associated with this game object for testing.
        /// </summary>
        public IUnityService unityService = UnityService.Instance;

        /// <summary>
        /// Camera config associated wit this camera controller.
        /// </summary>
        [SerializeField]
        public CameraConfig config = new CameraConfig();

        /// <inheritdoc/>
        public float PreviousOpacity { get; set; }

        /// <inheritdoc/>
        public float Pitch { get; set; }

        /// <inheritdoc/>
        public float Yaw { get; set; }

        /// <inheritdoc/>
        public Quaternion PlayerHeading => Quaternion.Euler(0, Yaw, 0);

        /// <inheritdoc/>
        public Transform CameraBase => config.cameraTransform;

        /// <inheritdoc/>
        public Transform PlayerBase => transform;

        public void Start()
        {
            config.Setup(gameObject);
        }

        public void Update()
        {
            CameraUtils.UpdateCameraController(config, gameObject, this, unityService.deltaTime);
        }
    }
}
