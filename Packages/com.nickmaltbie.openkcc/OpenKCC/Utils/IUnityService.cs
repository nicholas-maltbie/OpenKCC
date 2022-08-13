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

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Interface for managing static information from unity classes.
    /// </summary>
    public interface IUnityService
    {
        /// <summary>
        /// Get the current delta time in seconds between this and last frame
        /// </summary>
        /// <returns>The current delta time between this and the previous frame</returns>
        float deltaTime { get; }

        /// <summary>
        /// Get the current fixed delta time for physics based update
        /// </summary>
        /// <returns>the delta time shown by the fixed delta time</returns>
        float fixedDeltaTime { get; }

        /// <summary>
        /// Gets the current time in seconds since start of the game
        /// </summary>
        /// <value>Time in seconds since the start of the game</value>
        float time { get; }
    }

    /// <summary>
    /// Default implementation of unity service
    /// </summary>
    public class UnityService : IUnityService
    {
        /// <summary>
        /// Global instance of default UnityService.
        /// </summary>
        /// <returns></returns>
        public static IUnityService Instance { get; } = new UnityService();

        /// <inheritdoc/>
        public float deltaTime => Time.deltaTime;

        /// <inheritdoc/>
        public float time => Time.time;

        /// <inheritdoc/>
        public float fixedDeltaTime => Time.fixedDeltaTime;
    }
}
