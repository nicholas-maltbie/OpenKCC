
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.cinemachine
{
    public class DisplaySelectedCamera : MonoBehaviour
    {
        public TMP_Text text;

        public void Update()
        {
            if (CameraSelector.Instance != null)
            {
                text.text = "Selected Camera: " + CameraSelector.Instance.CurrentCamera.gameObject.name;
            }
        }
    }
}