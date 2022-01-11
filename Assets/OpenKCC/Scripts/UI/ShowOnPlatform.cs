using UnityEngine;

namespace nickmaltbie.OpenKCC.UI
{
    /// <summary>
    /// Simple class to show an element on a specific runtime platform.
    /// </summary>
    public class ShowOnPlatform : MonoBehaviour
    {
        public RuntimePlatform showPlatform = RuntimePlatform.WebGLPlayer;

        public void Awake()
        {
            gameObject.SetActive(Application.platform == showPlatform);
        }
    }
}
