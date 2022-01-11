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

using UnityEngine;

namespace nickmaltbie.OpenKCC.UI
{
    /// <summary>
    /// Simple class to abstract commands to change UI for a menu screen
    /// </summary>
    public class MenuController : MonoBehaviour
    {
        /// <summary>
        /// Can this menu controller allow for input to change screens
        /// </summary>
        public bool allowInputChanges = true;

        /// <summary>
        /// Short action delay (how fast can the screen change)
        /// </summary>
        public float actionDelay = 0.01f;

        /// <summary>
        /// Time of previous screen change
        /// </summary>
        private float previousTime;

        /// <summary>
        /// Can the screne be changed right now?
        /// </summary>
        private bool CanAct => allowInputChanges && (Time.time - previousTime > actionDelay);

        /// <summary>
        /// Requests go to the previous screen
        /// </summary>
        public void PreviousScreen()
        {
            if (!CanAct)
            {
                return;
            }

            previousTime = Time.time;
            UIManager.PreviousScreen(this);
        }

        /// <summary>
        /// Request a new screen using a prefab name
        /// </summary>
        /// <param name="screenPrefab">Screen prefab to switch to</param>
        public void SetScreen(GameObject screenPrefab)
        {
            SetScreen(screenPrefab.name);
        }

        /// <summary>
        /// Request a new screen directly through a name
        /// </summary>
        /// <param name="name">Name of new screen to display</param>
        public void SetScreen(string name)
        {
            if (!CanAct)
            {
                return;
            }

            previousTime = Time.time;
            UIManager.RequestNewScreen(this, name);
        }
    }
}
