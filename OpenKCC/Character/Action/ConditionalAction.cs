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

namespace nickmaltbie.OpenKCC.Character.Action
{
    /// <summary>
    /// Conditional action that can be performed if a given set of conditions
    /// are met.
    /// </summary>
    [Serializable]
    public class ConditionalAction
    {
        /// <summary>
        /// Unity service for managing time inputs.
        /// </summary>
        public IUnityService unityService = UnityService.Instance;

        /// <summary>
        /// Buffer time after which conditions are no longer
        /// met that this action can still be performed.
        /// </summary>
        [SerializeField]
        [Tooltip("Buffer time after which conditions are no longer met that this action can still be performed.")]
        public float coyoteTime;

        /// <summary>
        /// Can this action be performed.
        /// </summary>
        public bool CanPerform => condition?.Invoke() is true || (coyoteTime > 0 && elapsed <= coyoteTime);

        /// <summary>
        /// Condition for configuring this conditional action.
        /// </summary>
        protected Func<bool> condition;

        /// <summary>
        /// Elapsed time since the condition for this input
        /// was last met.
        /// </summary>
        private float elapsed;

        /// <summary>
        /// Add delta time to update the conditional action state.
        /// </summary>
        public virtual void Update()
        {
            if (!(condition?.Invoke() is true))
            {
                elapsed += unityService.deltaTime;
            }
        }

        /// <summary>
        /// Configure the condition for this action.
        /// </summary>
        /// <param name="condition">Condition for the action.</param>
        public void SetCondition(Func<bool> condition)
        {
            this.condition = condition;
        }
    }
}
