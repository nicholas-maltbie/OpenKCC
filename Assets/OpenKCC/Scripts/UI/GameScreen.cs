// Copyright (C) 2022 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
//

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

            foreach (var screenComponent in gameObject.GetComponentsInChildren<IScreenComponent>())
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

            foreach (var screenComponent in gameObject.GetComponentsInChildren<IScreenComponent>())
            {
                screenComponent.OnScreenUnloaded();
            }
        }
    }
}
