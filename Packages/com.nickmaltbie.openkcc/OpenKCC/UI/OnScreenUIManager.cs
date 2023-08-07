// Copyright (C) 2023 Nicholas Maltbie
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

using nickmaltbie.ScreenManager.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.UI.Actions
{
    public class OnScreenUIManager : MonoBehaviour
    {
        public static bool DefaultState = false;
        public static OnScreenUIManager Instance { get; private set; }

        public InputActionReference lookAction;
        public bool EnabledState { get; private set; } = DefaultState;

        private CursorStateOnMenuLoad cursorState;
        private CanvasGroup canvasGroup;
        private InputBinding lookActionDefault;

        public void Awake()
        {
            Instance = this;
            cursorState = gameObject.GetComponentInParent<CursorStateOnMenuLoad>();
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            lookActionDefault = lookAction.action.bindings[0];
        }

        public void Start()
        {
            if (EnabledState)
            {
                EnableOnScreenInput();
            }
            else
            {
                DisableOnScreenInput();
            }
        }

        public void UpdateState(bool value)
        {
            EnabledState = value;
            if (value)
            {
                EnableOnScreenInput();
            }
            else
            {
                DisableOnScreenInput();
            }
        }

        protected void DisableOnScreenInput()
        {
            lookAction?.action.ApplyBindingOverride(0, lookActionDefault);
            canvasGroup.alpha = 0.0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            cursorState.cursorVisible = false;
            cursorState.cursorLockMode = CursorLockMode.Locked;
        }

        protected void EnableOnScreenInput()
        {
            lookAction?.action.ApplyBindingOverride(0, string.Empty);
            canvasGroup.alpha = 1.0f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            cursorState.cursorVisible = true;
            cursorState.cursorLockMode = CursorLockMode.None;
        }
    }
}
