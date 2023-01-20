﻿// Copyright (C) 2023 Nicholas Maltbie
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
using nickmaltbie.OpenKCC.Utils;

namespace nickmaltbie.OpenKCC.Character.Attributes
{
    /// <summary>
    /// Attribute to represent player movement settings for a given state.
    /// </summary>
    public class MovementSettingsAttribute : Attribute
    {
        /// <summary>
        /// Allow movement by normal velocity.
        /// </summary>
        [Obsolete("Replaced by KCCConfig.AllowVelocity")]
        public bool AllowVelocity = false;

        /// <summary>
        /// Allow movement by player input movement.
        /// </summary>
        [Obsolete("Natural extension of letting speed config be unset, so not needed.")]
        public bool AllowWalk = false;

        /// <summary>
        /// Should the player be snapped down after moving.
        /// </summary>
        [Obsolete("Replaced by KCCConfig.SnapPlayerDown")]
        public bool SnapPlayerDown = false;

        /// <summary>
        /// Function to read speed value.
        /// </summary>
        public string SpeedConfig;

        /// <summary>
        /// Get player speed from a movement settings attribute.
        /// </summary>
        /// <param name="state">State to lookup speed for.</param>
        /// <param name="source">Object to lookup speed from.</param>
        public static float GetSpeed(Type state, object source)
        {
            if (Attribute.GetCustomAttribute(state, typeof(MovementSettingsAttribute)) is MovementSettingsAttribute settings && !string.IsNullOrWhiteSpace(settings.SpeedConfig))
            {
                return (float)source.EvaluateMember(settings.SpeedConfig);
            }

            return 0.0f;
        }
    }
}
