using MLAPI;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Character
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
            this.cameraController = GetComponent<CameraController>();
            this.audioListener = GameObject.FindObjectOfType<AudioListener>();
        }

        public void LateUpdate()
        {
            // Do nothing if there is no main camera
            if (Camera.main == null)
            {
                return;
            }

            // Set main camera's parent to be this and set it's relative position and rotation to be zero
            GameObject mainCamera = Camera.main.gameObject;
            mainCamera.transform.rotation = this.cameraController.cameraTransform.rotation;
            mainCamera.transform.position = this.cameraController.cameraTransform.position;

            // If the camera has an audio listener, make sure to move that as well to the character's base 
            //  camera position (simulate sound coming from the character's head, not the camera position)
            if (audioListener != null)
            {
                this.audioListener.transform.position = this.cameraController.CameraSource;
                this.audioListener.transform.rotation = this.cameraController.cameraTransform.rotation;
            }
        }
    }
}