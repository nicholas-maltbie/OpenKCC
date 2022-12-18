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

using System;
using nickmaltbie.TestUtilsUnity;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace nickmaltbie.OpenKCC.Input
{
    /// <summary>
    /// Buffered input that supports a delta of time between when the button
    /// is pressed and when the action is considered performed. Also supports
    /// a cooldown between action resets.
    /// </summary>
    [Serializable]
    public class BufferedInput
    {
        /// <summary>
        /// Unity service for getting unity inputs in testable manner.
        /// </summary>
        public IUnityService unityService = UnityService.Instance;

        /// <summary>
        /// Action associated with this input.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("inputAction")]
        [Tooltip("Input action to control this buffered input.")]
        public InputActionReference inputActionReference;

        /// <summary>
        /// Override input action for testing.
        /// </summary>
        private InputAction overrideInputAction;

        /// <summary>
        /// Gets the input action associated with this buffered input.
        /// </summary>
        public InputAction InputAction
        {
            get => overrideInputAction ?? inputActionReference;
            set => overrideInputAction = value;
        }

        /// <summary>
        /// Buffer time for configuring the player input.
        /// </summary>
        [SerializeField]
        [Tooltip("Total buffer time for player input.")]
        public float bufferTime;

        /// <summary>
        /// Cooldown time between input actions.
        /// </summary>
        [SerializeField]
        [Tooltip("Total buffer time for player input.")]
        public float cooldown = 0.1f;

        /// <summary>
        /// Amount of time elapsed since the player pressed the input.
        /// </summary>
        private float elapsedSincePressed = Mathf.Infinity;

        /// <summary>
        /// Amount fo elapsed time since the input was reset.
        /// </summary>
        private float elapsedSinceReset = Mathf.Infinity;

        /// <summary>
        /// Is the input currently active.
        /// </summary>
        public bool Pressed => elapsedSinceReset > cooldown && elapsedSincePressed <= bufferTime;

        /// <summary>
        /// Add delta time to update the buffered input state.
        /// </summary>
        /// <param name="deltaTime">Delta time to add in seconds.</param>
        public void Update()
        {
            elapsedSincePressed += unityService.deltaTime;
            elapsedSinceReset += unityService.deltaTime;

            if (InputAction?.IsPressed() ?? false)
            {
                elapsedSincePressed = 0.0f;
            }
        }

        /// <summary>
        /// Reset the active state for this button.
        /// </summary>
        public void Reset()
        {
            elapsedSincePressed = Mathf.Infinity;
            elapsedSinceReset = 0;
        }
    }
}
