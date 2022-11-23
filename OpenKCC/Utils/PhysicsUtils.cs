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

using System.Collections.Generic;

using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Collection of functions for abstracting more complex physics operations.
    /// </summary>
    public static class PhysicsUtils
    {
        /// <summary>
        /// Filters a set of raycast hits for the first hit that is of a collider attached ot a specific object
        /// </summary>
        /// <param name="allow">Only game object that will be searched for</param>
        /// <param name="hits">All of the hits to process</param>
        /// <param name="closest">Closest hit that is attached to the ignored object</param>
        /// <returns>True if a hit was detected, false otherwise</returns>
        public static bool FilterForFirstHitAllow(GameObject allow, IEnumerable<RaycastHit> hits, out RaycastHit closest)
        {
            bool hitSomething = false;
            closest = new RaycastHit { distance = Mathf.Infinity };
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject == allow && hit.distance < closest.distance)
                {
                    hitSomething = true;
                    closest = hit;
                }
            }

            return hitSomething;
        }

        /// <summary>
        /// Filters a set of raycast hits for the first hit that is of a collider attached ot a specific object
        /// </summary>
        /// <param name="target">Only game object that will be searched for</param>
        /// <param name="hits">All of the hits to process</param>
        /// <param name="closest">Closest hit that is attached to the ignored object</param>
        /// <returns>True if a hit was detected, false otherwise</returns>
        public static bool FilterForFirstHitAllow(ICollection<GameObject> allowList, IEnumerable<RaycastHit> hits, out RaycastHit closest)
        {
            bool hitSomething = false;
            closest = new RaycastHit { distance = Mathf.Infinity };
            foreach (RaycastHit hit in hits)
            {
                if (allowList.Contains(hit.collider.gameObject) && hit.distance < closest.distance)
                {
                    hitSomething = true;
                    closest = hit;
                }
            }

            return hitSomething;
        }

        /// <summary>
        /// Filters a set of raycast hits for the first hit that is not 
        /// of a collider attached to a given game object
        /// </summary>
        /// <param name="ignoreList">Game objects to ignore when checking for collider hit</param>
        /// <param name="hits">All of the hits to process</param>
        /// <param name="closest">Closest hit that is attached to the ignored object</param>
        /// <returns>True if a hit was detected, false otherwise</returns>
        public static bool FilterForFirstHitIgnore(ICollection<GameObject> ignoreList, IEnumerable<RaycastHit> hits, out RaycastHit closest)
        {
            bool hitSomething = false;
            closest = new RaycastHit { distance = Mathf.Infinity };
            foreach (RaycastHit hit in hits)
            {
                if (!ignoreList.Contains(hit.collider.gameObject) && hit.distance < closest.distance)
                {
                    hitSomething = true;
                    closest = hit;
                }
            }

            return hitSomething;
        }

        /// <summary>
        /// Filters a set of raycast hits for the first hit that is not 
        /// of a collider attached to a given game object
        /// </summary>
        /// <param name="ignore">Game object to ignore when checking for collider hit</param>
        /// <param name="hits">All of the hits to process</param>
        /// <param name="closest">Closest hit that is attached to the ignored object</param>
        /// <returns>True if a hit was detected, false otherwise</returns>
        public static bool FilterForFirstHitIgnore(GameObject ignore, IEnumerable<RaycastHit> hits, out RaycastHit closest)
        {
            bool hitSomething = false;
            closest = new RaycastHit { distance = Mathf.Infinity };
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject != ignore && hit.distance < closest.distance)
                {
                    hitSomething = true;
                    closest = hit;
                }
            }

            return hitSomething;
        }

        /// <summary>
        /// Compute the first object hit while only looking for colliders attached to a specific object
        /// </summary>
        /// <param name="target">Object search for given raycast</param>
        /// <param name="source">Source position of raycast</param>
        /// <param name="direction">Direction of raycast</param>
        /// <param name="distance">Distance of raycast</param>
        /// <param name="layerMask">Laymask for raycast</param>
        /// <param name="queryTriggerInteraction">Query trigger interaction for raycast</param>
        /// <param name="closest">The closest raycast hit event</param>
        /// <returns>True if a hit was detected, false otherwise.</returns>
        public static bool RaycastHitAllow(ICollection<GameObject> allowList, Vector3 source, Vector3 direction, float distance,
            LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction, out RaycastHit closest)
        {
            return FilterForFirstHitAllow(allowList, Physics.RaycastAll(source, direction, distance, layerMask, queryTriggerInteraction), out closest);
        }

        /// <summary>
        /// Compute the first object hit while only looking for colliders attached to a specific object
        /// </summary>
        /// <param name="target">Object search for given raycast</param>
        /// <param name="source">Source position of raycast</param>
        /// <param name="direction">Direction of raycast</param>
        /// <param name="distance">Distance of raycast</param>
        /// <param name="layerMask">Laymask for raycast</param>
        /// <param name="queryTriggerInteraction">Query trigger interaction for raycast</param>
        /// <param name="closest">The closest raycast hit event</param>
        /// <returns>True if a hit was detected, false otherwise.</returns>
        public static bool RaycastHitAllow(GameObject target, Vector3 source, Vector3 direction, float distance,
            LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction, out RaycastHit closest)
        {
            return FilterForFirstHitAllow(target, Physics.RaycastAll(source, direction, distance, layerMask, queryTriggerInteraction), out closest);
        }

        /// <summary>
        /// Compute the first object hit while only looking for colliders attached to a specific object
        /// </summary>
        /// <param name="target">Object search for given raycast</param>
        /// <param name="source">Source position of raycast</param>
        /// <param name="radius">Radius spherecast</param>
        /// <param name="direction">Direction of raycast</param>
        /// <param name="distance">Distance of raycast</param>
        /// <param name="layerMask">Laymask for raycast</param>
        /// <param name="queryTriggerInteraction">Query trigger interaction for raycast</param>
        /// <param name="closest">The closest raycast hit event</param>
        /// <returns>True if a hit was detected, false otherwise.</returns>
        public static bool SphereCastAllow(GameObject target, Vector3 source, float radius, Vector3 direction, float distance,
            LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction, out RaycastHit closest)
        {
            return FilterForFirstHitAllow(target, Physics.SphereCastAll(source, radius, direction, distance, layerMask, queryTriggerInteraction), out closest);
        }

        /// <summary>
        /// Compute the first object hit while ignoring a given object for a raycast.
        /// Will include overlapping objects if objects overlap.
        /// </summary>
        /// <param name="ignoreList">Object to ignore from raycast</param>
        /// <param name="source">Source position of raycast</param>
        /// <param name="direction">Direction of raycast</param>
        /// <param name="distance">Distance of raycast</param>
        /// <param name="layerMask">Laymask for raycast</param>
        /// <param name="queryTriggerInteraction">Query trigger interaction for raycast</param>
        /// <param name="closest">The closest raycast hit event</param>
        /// <returns>True if a hit was detected, false otherwise.</returns>
        public static bool RaycastFirstHitIgnore(ICollection<GameObject> ignoreList, Vector3 source, Vector3 direction, float distance,
            LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction, out RaycastHit closest)
        {
            return FilterForFirstHitIgnore(ignoreList, Physics.RaycastAll(source, direction, distance, layerMask, queryTriggerInteraction), out closest);
        }

        /// <summary>
        /// Compute the first object hit while ignoring a given object for a raycast.
        /// Will include overlapping objects if objects overlap.
        /// </summary>
        /// <param name="ignore">Object to ignore from raycast</param>
        /// <param name="source">Source position of raycast</param>
        /// <param name="direction">Direction of raycast</param>
        /// <param name="distance">Distance of raycast</param>
        /// <param name="layerMask">Laymask for raycast</param>
        /// <param name="queryTriggerInteraction">Query trigger interaction for raycast</param>
        /// <param name="closest">The closest raycast hit event</param>
        /// <returns>True if a hit was detected, false otherwise.</returns>
        public static bool RaycastFirstHitIgnore(GameObject ignore, Vector3 source, Vector3 direction, float distance,
            LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction, out RaycastHit closest)
        {
            return FilterForFirstHitIgnore(ignore, Physics.RaycastAll(source, direction, distance, layerMask, queryTriggerInteraction), out closest);
        }

        /// <summary>
        /// Compute the first object hit while ignoring a given object for a spherecast.
        /// Will include overlapping objects if objects overlap.
        /// </summary>
        /// <param name="ignoreList">Objects to ignore from spherecast</param>
        /// <param name="source">Source position of spherecast</param>
        /// <param name="radius">Radius spherecast</param>
        /// <param name="direction">Direction of spherecast</param>
        /// <param name="distance">Distance of spherecast</param>
        /// <param name="layerMask">Laymask for spherecast</param>
        /// <param name="queryTriggerInteraction">Query trigger interaction for spherecast</param>
        /// <param name="closest">The closest raycast hit event</param>
        /// <returns>True if a hit was detected, false otherwise.</returns>
        public static bool SphereCastFirstHitIgnore(ICollection<GameObject> ignoreList, Vector3 source, float radius, Vector3 direction, float distance,
            LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction, out RaycastHit closest)
        {
            return FilterForFirstHitIgnore(ignoreList, Physics.SphereCastAll(source, radius, direction, distance, layerMask, queryTriggerInteraction), out closest);
        }

        /// <summary>
        /// Compute the first object hit while ignoring a given object for a spherecast.
        /// Will include overlapping objects if objects overlap.
        /// </summary>
        /// <param name="ignore">Object to ignore from spherecast</param>
        /// <param name="source">Source position of spherecast</param>
        /// <param name="radius">Radius spherecast</param>
        /// <param name="direction">Direction of spherecast</param>
        /// <param name="distance">Distance of spherecast</param>
        /// <param name="layerMask">Laymask for spherecast</param>
        /// <param name="queryTriggerInteraction">Query trigger interaction for spherecast</param>
        /// <param name="closest">The closest raycast hit event</param>
        /// <returns>True if a hit was detected, false otherwise.</returns>
        public static bool SphereCastFirstHitIgnore(GameObject ignore, Vector3 source, float radius, Vector3 direction, float distance,
            LayerMask layerMask, QueryTriggerInteraction queryTriggerInteraction, out RaycastHit closest)
        {
            return FilterForFirstHitIgnore(ignore, Physics.SphereCastAll(source, radius, direction, distance, layerMask, queryTriggerInteraction), out closest);
        }
    }
}
