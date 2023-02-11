
using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.cinemachine
{
    public class CameraSelector : MonoBehaviour
    {
        public CinemachineVirtualCamera[] cameras;
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
            foreach (var camera in cameras)
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
    }
}