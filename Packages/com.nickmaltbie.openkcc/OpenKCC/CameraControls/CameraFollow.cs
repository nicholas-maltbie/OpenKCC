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

using UnityEngine;

namespace nickmaltbie.OpenKCC.CameraControls
{
    /// <summary>
    /// Script to move main camera to follow the local player
    /// </summary>
    [RequireComponent(typeof(CameraController))]
    public class CameraFollow : MonoBehaviour
    {
        /// <summary>
        /// Position and rotation to control camera position and movement
        /// </summary>
        private CameraController cameraController;

        /// <summary>
        /// AudioListener for moving listening position
        /// </summary>
        private AudioListener audioListener;

        public void Start()
        {
            cameraController = GetComponent<CameraController>();
            audioListener = GameObject.FindObjectOfType<AudioListener>();
        }

        public void LateUpdate()
        {
            CameraFollow.MoveCamera(cameraController.config.cameraTransform, audioListener);
        }

        /// <summary>
        /// Move the main camera to follow a given transform.
        /// </summary>
        /// <param name="targetCameraPos">Position to move the camera to.</param>
        /// <param name="audioListener">Audio listener associated with the camera to move as well.</param>
        public static void MoveCamera(Transform targetCameraPos, AudioListener audioListener = null)
        {
            // Do nothing if there is no main camera
            if (Camera.main == null)
            {
                return;
            }

            // Set main camera's parent to be this and set it's relative position and rotation to be zero
            GameObject mainCamera = Camera.main.gameObject;
            mainCamera.transform.position = targetCameraPos.position;
            mainCamera.transform.rotation = targetCameraPos.rotation;

            // If the camera has an audio listener, make sure to move that as well to the character's base 
            //  camera position (simulate sound coming from the character's head, not the camera position)
            if (audioListener != null)
            {
                audioListener.transform.position = targetCameraPos.position;
                audioListener.transform.rotation = targetCameraPos.rotation;
            }
        }
    }
}
