
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

        public void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}