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

namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Current player input state, is the player allowed to give input or not
    /// </summary>
    public enum PlayerInputState
    {
        Allow,
        Deny
    }

    /// <summary>
    /// Manager for player input information as a static class
    /// </summary>
    public static class PlayerInputUtils
    {
        /// <summary>
        /// Current state of the player movement action
        /// </summary>
        public static PlayerInputState playerMovementState = PlayerInputState.Allow;

        /// <summary>
        /// Minimum mouse sensitivity value multiplier
        /// </summary>
        public static readonly float minimumMouseSensitivity = 0.05f;
        /// <summary>
        /// Maximum mouse sensitivity value multiplier
        /// </summary>
        public static readonly float maximumMouseSensitivity = 1.0f;

        /// <summary>
        /// Mouse sensitivity multiplier (should be between 0.05 and 1.0 hopefully)
        /// </summary>
        public static float mouseSensitivity = 0.25f;

        /// <summary>
        /// Default mouse sensitivity
        /// </summary>
        public const float DefaultMouseSensitivity = 0.25f;
    }
}
