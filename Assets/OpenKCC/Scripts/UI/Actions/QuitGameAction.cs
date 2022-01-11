using UnityEngine;

namespace nickmaltbie.OpenKCC.UI.Actions
{
    /// <summary>
    /// Class with behaviour to quit game
    /// </summary>
    public class QuitGameAction : MonoBehaviour
    {
        /// <summary>
        /// Quit the game
        /// </summary>
        public void QuitGame()
        {
            // save any game data here
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
