
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.cinemachine
{
    public class PlayerFade : MonoBehaviour
    {
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

            if (actualDistance < shadowOnlyDistance)
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
                float lerpPosition = fadeTime > 0 ? Time.deltaTime * 1 / fadeTime : 1;
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
