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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils.ColliderCast
{
    /// <summary>
    /// This class is only meant for testing, no reason to include in code coverage metrics.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class MockColliderCast : MonoBehaviour, IColliderCast
    {
        /// <summary>
        /// Callback function for <see cref="nickmaltbie.OpenKCC.Utils.IRaycastHelper.DoRaycastInDirection(Vector3, Vector3, float, out IRaycastHit, int, QueryTriggerInteraction)"/>
        /// </summary>
        /// <param name="source">Source point to check from.</param>
        /// <param name="direction">Direction to search for step.</param>
        /// <param name="distance">Distance to search for step ahead of player.</param>
        /// <param name="hit">Information about hit.</param>
        /// <param name="layerMask">Layer mask for checking which objects to collide with.</param>
        /// <param name="queryTriggerInteraction">Configuration for QueryTriggerInteraction when solving for collisions.</param>
        /// <param name="skinWidth">Buffer around player when casting object.</param>
        /// <returns>Is something ahead hit.</returns>
        public delegate void DoRaycastInDirectionCallback(Vector3 source, Vector3 direction, float distance, out IRaycastHit hit, int layerMask, QueryTriggerInteraction queryTriggerInteraction, float skinWidth);

        /// <summary>
        /// Callback function for <see cref="nickmaltbie.OpenKCC.Utils.IRaycastHelper.DoRaycastInDirection(Vector3, Vector3, float, out IRaycastHit, int, QueryTriggerInteraction)"/>
        /// </summary>
        /// <param name="source">Source point to check from.</param>
        /// <param name="direction">Direction to search for step.</param>
        /// <param name="distance">Distance to search for step ahead of player.</param>
        /// <param name="hit">Information about hit.</param>
        /// <param name="layerMask">Layer mask for checking which objects to collide with.</param>
        /// <param name="queryTriggerInteraction">Configuration for QueryTriggerInteraction when solving for collisions.</param>
        /// <returns>Is something ahead hit.</returns>
        public delegate bool DoRaycastInDirectionReturns(Vector3 source, Vector3 direction, float distance, out IRaycastHit hit, int layerMask, QueryTriggerInteraction queryTriggerInteraction);

        /// <summary>
        /// Callback function for <see cref="nickmaltbie.OpenKCC.Utils.IColliderCast.CastSelf"/>
        /// </summary>
        /// <param name="position">Position of the object when it is being raycast.</param>
        /// <param name="rotation">Rotation of the objecting when it is being raycast.</param>
        /// <param name="direction">Direction of the raycast.</param>
        /// <param name="distance">Maximum distance of raycast.</param>
        /// <param name="hit">First object hit and related information, will have a distance of Infinity if none
        /// is found.</param>
        /// <param name="skinWidth">Width of skin of object to use when casting a hit. Essentially buffer
        /// space around the edge of the object.</param>
        /// <returns>True if an object is hit within distance, false otherwise.</returns>
        public delegate void CastSelfCallback(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out IRaycastHit hit, int layerMask, QueryTriggerInteraction queryTriggerInteraction, float skinWidth);

        /// <summary>
        /// Callback function for <see cref="nickmaltbie.OpenKCC.Utils.IColliderCast.CastSelf"/>
        /// </summary>
        /// <param name="position">Position of the object when it is being raycast.</param>
        /// <param name="rotation">Rotation of the objecting when it is being raycast.</param>
        /// <param name="direction">Direction of the raycast.</param>
        /// <param name="distance">Maximum distance of raycast.</param>
        /// <param name="hit">First object hit and related information, will have a distance of Infinity if none
        /// is found.</param>
        /// <param name="skinWidth">Buffer around player when casting object.</param>
        /// <returns>True if an object is hit within distance, false otherwise.</returns>
        public delegate bool CastSelfReturns(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out IRaycastHit hit, int layerMask, QueryTriggerInteraction queryTriggerInteraction, float skinWidth);

        public delegate Vector3 PushOutOverlappingReturns(Vector3 position, Quaternion rotation, float maxDistance, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore, float skinWidth = 0.0f);
        public delegate IEnumerable<Collider> GetOverlappingReturns(Vector3 position, Quaternion rotation, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore, float skinWidth = 0.0f);
        public delegate IEnumerable<RaycastHit> GetHitsReturns(Vector3 position, Quaternion rotation, Vector3 direction, float distance, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore, float skinWidth = 0.01f);
        public delegate Vector3 GetBottomReturns(Vector3 position, Quaternion rotation);

        public CastSelfReturns OnCastSelf;
        public DoRaycastInDirectionReturns OnDoRaycastInDirection;
        public GetBottomReturns OnGetBottom;
        public GetHitsReturns OnGetHits;
        public GetOverlappingReturns OnGetOverlapping;
        public PushOutOverlappingReturns OnPushOutOverlapping;

        public bool CastSelf(Vector3 position, Quaternion rotation, Vector3 direction, float distance, out IRaycastHit hit, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore, float skinWidth = 0.01f)
        {
            hit = default;
            return OnCastSelf?.Invoke(position, rotation, direction, distance, out hit, layerMask, queryTriggerInteraction, skinWidth) ?? false;
        }

        public bool DoRaycastInDirection(Vector3 source, Vector3 direction, float distance, out IRaycastHit stepHit, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore)
        {
            stepHit = default;
            return OnDoRaycastInDirection?.Invoke(source, direction, distance, out stepHit, layerMask, queryTriggerInteraction) ?? false;
        }

        public Vector3 GetBottom(Vector3 position, Quaternion rotation)
        {
            return OnGetBottom?.Invoke(position, rotation) ?? Vector3.zero;
        }

        public IEnumerable<RaycastHit> GetHits(Vector3 position, Quaternion rotation, Vector3 direction, float distance, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore, float skinWidth = 0.01f)
        {
            return OnGetHits?.Invoke(position, rotation, direction, distance, layerMask, queryTriggerInteraction, skinWidth);
        }

        public IEnumerable<Collider> GetOverlapping(Vector3 position, Quaternion rotation, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore, float skinWidth = 0.0f)
        {
            return OnGetOverlapping?.Invoke(position, rotation, layerMask, queryTriggerInteraction, skinWidth);
        }

        public Vector3 PushOutOverlapping(Vector3 position, Quaternion rotation, float maxDistance, int layerMask = -1, QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore, float skinWidth = 0.0f)
        {
            return OnPushOutOverlapping?.Invoke(position, rotation, maxDistance, layerMask, queryTriggerInteraction, skinWidth) ?? Vector3.zero;
        }
    }
}
