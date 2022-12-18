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

using nickmaltbie.OpenKCC.CameraControls;
using Unity.Netcode;
using UnityEngine;

namespace nickmaltbie.OpenKCC.netcode.CameraControls
{
    /// <summary>
    /// Script to move main camera to follow the local player
    /// </summary>
    [RequireComponent(typeof(NetworkCameraController))]
    [DefaultExecutionOrder(10000)]
    public class NetworkCameraFollow : NetworkBehaviour
    {
        /// <summary>
        /// Position and rotation to control camera position and movement
        /// </summary>
        private NetworkCameraController cameraController;

        /// <summary>
        /// AudioListener for moving listening position
        /// </summary>
        private AudioListener audioListener;

        public void Start()
        {
            cameraController = GetComponent<NetworkCameraController>();
            audioListener = GameObject.FindObjectOfType<AudioListener>();
        }

        public void LateUpdate()
        {
            if (IsOwner)
            {
                CameraFollow.MoveCamera(cameraController.config.cameraTransform, audioListener);
            }
        }
    }
}
