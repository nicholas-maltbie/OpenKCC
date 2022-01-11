using nickmaltbie.OpenKCC.Character;

using UnityEngine;

namespace nickmaltbie.OpenKCC.UI.Events
{
    /// <summary>
    /// Simple class to set player movement state when menu loads
    /// </summary>
    public class PlayerMovementStateOnMenuLoad : MonoBehaviour, IScreenComponent
    {
        /// <summary>
        /// Player input state to set when this menu is loaded
        /// </summary>
        public PlayerInputState playerInputState = PlayerInputState.Allow;

        public void OnScreenLoaded()
        {
            PlayerInputManager.playerMovementState = this.playerInputState;
        }

        public void OnScreenUnloaded()
        {

        }
    }
}
