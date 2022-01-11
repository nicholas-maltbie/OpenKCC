using UnityEngine;

namespace nickmaltbie.OpenKCC.UI.Events
{
    /// <summary>
    /// Simple class to set cursor state when the menu loads
    /// </summary>
    public class CursorStateOnMenuLoad : MonoBehaviour, IScreenComponent
    {
        /// <summary>
        /// Lock state of cursor when loading this screen
        /// </summary>
        public CursorLockMode cursorLockMode = CursorLockMode.None;

        /// <summary>
        /// Visible state of the cursor when loading this screen
        /// </summary>
        public bool cursorVisible = true;

        public void OnScreenLoaded()
        {
            Cursor.lockState = cursorLockMode;
            Cursor.visible = cursorVisible;
        }

        public void OnScreenUnloaded()
        {

        }
    }
}
