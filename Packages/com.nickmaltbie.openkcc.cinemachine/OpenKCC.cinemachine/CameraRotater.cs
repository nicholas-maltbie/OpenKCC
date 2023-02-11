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

using nickmaltbie.OpenKCC.Character;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.cinemachine
{
    public class CameraRotater : MonoBehaviour
    {
        /// <summary>
        /// Object to rotate for camera to follow.
        /// </summary>
        public GameObject followTarget;

        /// <summary>
        /// Action to rotate camera.
        /// </summary>
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
        /// Current pitch of the camera.
        /// </summary>
        private float Pitch { get; set; }

        /// <summary>
        /// Current Yaw of the camera.
        /// </summary>
        private float Yaw { get; set; }

        public void Awake()
        {
            Pitch = followTarget.transform.rotation.eulerAngles.x;
            Yaw = followTarget.transform.rotation.eulerAngles.y;
        }

        public void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;

            Vector2 look = LookAction?.ReadValue<Vector2>() ?? Vector2.zero;
            look *= PlayerInputUtils.mouseSensitivity;
            float yawChange = look.x;
            float pitchChange = look.y;

            // bound pitch between -180 and 180
            Pitch = (Pitch % 360 + 180) % 360 - 180;

            // Only allow rotation if player is allowed to move
            if (PlayerInputUtils.playerMovementState == PlayerInputState.Allow)
            {
                yawChange = rotationRate * deltaTime * yawChange;
                Yaw += yawChange;
                Pitch += rotationRate * deltaTime * -1 * pitchChange;
            }

            // Clamp rotation of camera between minimum and maximum specified pitch
            Pitch = Mathf.Clamp(Pitch, minPitch, maxPitch);

            // Set the player's rotation to be that of the camera's yaw
            // transform.rotation = Quaternion.Euler(0, yaw, 0);
            // Set pitch to be camera's rotation
            followTarget.transform.rotation = Quaternion.Euler(Pitch, Yaw, 0);
        }
    }
}
