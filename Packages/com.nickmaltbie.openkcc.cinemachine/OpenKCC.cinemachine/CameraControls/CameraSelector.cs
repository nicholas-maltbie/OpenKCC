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

using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.cinemachine.CameraControls
{
    public class CameraSelector : MonoBehaviour
    {
        public static CameraSelector Instance { get; private set; }

        public CinemachineVirtualCamera[] cameras;

        public CinemachineVirtualCamera CurrentCamera => cameras[current];
        private int current = 0;

        /// <summary>
        /// Action to toggle selected camera.
        /// </summary>
        [Tooltip("Action to toggle selected camera")]
        [SerializeField]
        public InputActionReference toggleCameraActionReference;

        /// <summary>
        /// Override for the toggle camera action of the player.
        /// </summary>
        private InputAction overrideToggleCamera;

        /// <summary>
        /// Gest or sets the toggle camera action associated with this player.
        /// </summary>
        public InputAction ToggleCameraAction
        {
            get => overrideToggleCamera ?? toggleCameraActionReference?.action;
            set => overrideToggleCamera = value;
        }

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            foreach (CinemachineVirtualCamera camera in cameras)
            {
                camera.gameObject.SetActive(false);
            }

            cameras[current].gameObject.SetActive(true);
            ToggleCameraAction.Enable();

            ToggleCameraAction.performed += _ =>
            {
                cameras[current].gameObject.SetActive(false);
                current = (current + 1) % cameras.Length;
                cameras[current].gameObject.SetActive(true);
            };
        }

        public void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
