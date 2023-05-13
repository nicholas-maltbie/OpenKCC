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

using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Basic implementation of IRaycastHelper
    /// </summary>
    public class RaycastHelper : IRaycastHelper
    {
        /// <summary>
        /// Universal instance of RaycastHelper.
        /// </summary>
        public static RaycastHelper Instance = new RaycastHelper();

        /// <summary>
        /// Private instance of raycast helper.
        /// </summary>
        private RaycastHelper() { }

        /// <inheritdoc/>
        public bool DoRaycastInDirection(Vector3 source, Vector3 direction, float distance, out IRaycastHit stepHit, int layerMask = IRaycastHelper.DefaultLayerMask, QueryTriggerInteraction queryTriggerInteraction = IRaycastHelper.DefaultQueryTriggerInteraction)
        {
            bool didHit = Physics.Raycast(new Ray(source, direction), out RaycastHit hit, distance, layerMask, queryTriggerInteraction);
            stepHit = new RaycastHitWrapper(hit);
            return didHit;
        }
    }
}
