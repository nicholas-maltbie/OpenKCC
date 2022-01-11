using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace nickmaltbie.OpenKCC.UI
{
    /// <summary>
    /// Game screen that can be hidden or shown to the player
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class GameScreen : MonoBehaviour
    {
        /// <summary>
        /// Setup a screen with a given player input (if one exists)
        /// </summary>
        /// <param name="uIInputModule"></param>
        public void SetupScreen(InputSystemUIInputModule uIInputModule)
        {
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
            {
                playerInput.actions = uIInputModule.actionsAsset;
            }
        }

        /// <summary>
        /// Set a screen as visible and enable all attached components
        /// </summary>
        public void DisplayScreen()
        {
            var playerInput = GetComponent<PlayerInput>();
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1.0f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            if (playerInput != null)
            {
                playerInput.ActivateInput();
            }
            foreach (IScreenComponent screenComponent in gameObject.GetComponentsInChildren<IScreenComponent>())
            {
                screenComponent.OnScreenLoaded();
            }
        }

        /// <summary>
        /// Hide a screen and disable player action on this screen
        /// </summary>
        public void HideScreen()
        {
            var playerInput = GetComponent<PlayerInput>();
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0.0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            if (playerInput != null)
            {
                playerInput.DeactivateInput();
            }
            foreach (IScreenComponent screenComponent in gameObject.GetComponentsInChildren<IScreenComponent>())
            {
                screenComponent.OnScreenUnloaded();
            }
        }
    }
}
