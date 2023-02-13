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

using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity;
using UnityEngine;

namespace nickmaltbie.OpenKCC.cinemachine.Player
{
    public class PlayerFade : MonoBehaviour
    {
        /// <summary>
        /// Unity service for testing the player fading.
        /// </summary>
        public IUnityService unityService = UnityService.Instance;

        /// <summary>
        /// Game object for fading the avatar
        /// </summary>
        public GameObject avatarBase;

        /// <summary>
        /// Object to rotate for camera to follow.
        /// </summary>
        public GameObject followTarget;

        /// <summary>
        /// If the camera is within some given units of the player,
        /// fade the player avatar.
        /// </summary>
        public float fadeThreshold = 1.5f;

        /// <summary>
        /// Distance to only show shadows for the player.
        /// </summary>
        public float shadowOnlyDistance = 0.25f;

        /// <summary>
        /// Camera radius
        /// </summary>
        public float cameraRadius = 0.1f;

        /// <summary>
        /// Time it takes to fade the player avatar.
        /// </summary>
        private float fadeTime = 1.0f;

        /// <summary>
        /// Previous opacity of player.
        /// </summary>
        private float PreviousOpacity { get; set; }

        public void Update()
        {
            // Fade avatar based on distance to player
            Vector3 source = Camera.main.transform.position;
            Vector3 target = followTarget.transform.position;
            Vector3 dir = target - source;
            bool hittingSelf = PhysicsUtils.SphereCastAllow(gameObject, source, cameraRadius, dir, dir.magnitude, ~0, QueryTriggerInteraction.Ignore, out RaycastHit selfHit);
            float actualDistance = hittingSelf ? selfHit.distance : dir.magnitude;

            if (actualDistance <= shadowOnlyDistance)
            {
                MaterialUtils.RecursiveSetShadowCastingMode(avatarBase, UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly);
            }
            else
            {
                MaterialUtils.RecursiveSetShadowCastingMode(avatarBase, UnityEngine.Rendering.ShadowCastingMode.On);
            }

            if (actualDistance > shadowOnlyDistance && actualDistance < fadeThreshold)
            {
                float newOpacity = (actualDistance - shadowOnlyDistance) / fadeThreshold;
                float lerpPosition = fadeTime > 0 ? unityService.deltaTime * 1 / fadeTime : 1;
                PreviousOpacity = Mathf.Lerp(PreviousOpacity, newOpacity, lerpPosition);
                // Set opacity of character based on how close the camera is
                MaterialUtils.RecursiveSetFloatProperty(avatarBase, "_Opacity", PreviousOpacity);
            }
            else
            {
                // Set opacity of character based on how close the camera is
                MaterialUtils.RecursiveSetFloatProperty(avatarBase, "_Opacity", 1);
                PreviousOpacity = actualDistance > shadowOnlyDistance ? 1 : 0;
            }
        }
    }
}
