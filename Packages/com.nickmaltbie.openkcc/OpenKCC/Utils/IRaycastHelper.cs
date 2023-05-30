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
    public static class RaycastHelperConstants
    {
        public const int DefaultLayerMask = ~0;
        public const QueryTriggerInteraction DefaultQueryTriggerInteraction = QueryTriggerInteraction.Ignore;
    }

    /// <summary>
    /// Abstraction to check for raycast commands.
    /// </summary>
    public interface IRaycastHelper
    {
        /// <summary>
        /// Do a raycast in a given direction ignoring this object.
        /// </summary>
        /// <param name="source">Source point to check from.</param>
        /// <param name="direction">Direction to search for step.</param>
        /// <param name="distance">Distance to search for step ahead of player.</param>
        /// <param name="stepHit">Information about step hit ahead.</param>
        /// <param name="layerMask">Layer mask for checking which objects to collide with.</param>
        /// <param name="queryTriggerInteraction">Configuration for QueryTriggerInteraction when solving for collisions.</param>
        /// <returns>Is something ahead hit.</returns>
        bool DoRaycastInDirection(Vector3 source, Vector3 direction, float distance, out IRaycastHit stepHit, int layerMask = RaycastHelperConstants.DefaultLayerMask, QueryTriggerInteraction queryTriggerInteraction = RaycastHelperConstants.DefaultQueryTriggerInteraction);
    }
}
