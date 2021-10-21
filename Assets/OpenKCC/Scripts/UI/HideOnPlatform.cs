using UnityEngine;

namespace nickmaltbie.OpenKCC.UI
{
    /// <summary>
    /// Simple class to hide element on a specific runtime platform;
    /// </summary>
    public class HideOnPlatform : MonoBehaviour
    {
        public RuntimePlatform hidePlatform;

        public void Awake()
        {
            gameObject.SetActive(Application.platform != hidePlatform);
        }
    }
}