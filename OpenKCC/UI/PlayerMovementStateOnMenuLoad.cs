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

using nickmaltbie.OpenKCC.Character;
using nickmaltbie.ScreenManager;
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

        /// <inheritdoc/>
        public void OnScreenLoaded()
        {
            PlayerInputUtils.playerMovementState = playerInputState;
        }

        /// <inheritdoc/>
        public void OnScreenUnloaded()
        {

        }
    }
}
