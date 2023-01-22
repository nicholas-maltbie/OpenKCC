using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace com.nickmaltbie.OpenKCC.Demo
{
    [RequireComponent(typeof(IColliderCast))]
    public class DrawKCCBounces : MonoBehaviour
    {
        /// <summary>
        /// Should a collider at the position of the character be drawn?
        /// </summary>
        [SerializeField]
        [Tooltip("Should the initial collider at the current position be drawn.")]
        public bool drawInitialCollider = false;

        /// <summary>
        /// Should colliders for overlapping objects be drawn?
        /// </summary>
        [SerializeField]
        [Tooltip("Should the overlapping colliders be drawn.")]
        public bool drawOverlapColliders = true;

        /// <summary>
        /// Should the colliders be drawn when the player is not moving
        /// </summary>
        [SerializeField]
        [Tooltip("Should the colliders be drawn when the player is not moving.")]
        public bool drawWhenStill = true;

        /// <summary>
        /// Total distance to move player when drawing colliders.
        /// </summary>
        [SerializeField]
        [Tooltip("How far of a movement should be drawn.")]
        public float movementDistance = 5.0f;

        /// <summary>
        /// Alpha value for drawing the character collider fill.
        /// </summary>
        [SerializeField]
        [Tooltip("Alpha value for drawing the character collider fill.")]
        [Range(0, 1)]
        public float fillAlpha = 0.5f;

        /// <summary>
        /// Alpha value for drawing the character collider outline.
        /// </summary>
        [SerializeField]
        [Tooltip("Alpha value for drawing the character collider outline.")]
        [Range(0, 1)]
        public float outlineAlpha = 1.0f;

        /// <summary>
        /// Should the movement vector be projected onto the ground before moving?
        /// </summary>
        [SerializeField]
        [Tooltip("Should the movement vector be projected onto the ground before moving")]
        public bool useProjectedMovement = true;

        /// <summary>
        /// Colors for various iterations of drawing, will loop back to start if this runs out.
        /// </summary>
        [SerializeField]
        [Tooltip("Colors for various iterations of drawing, will loop back to start if this runs out.")]
        public Color[] bounceColors = new Color[]{
            new Color( 22 / 255f, 231 / 255f,  49 / 255f),
            new Color(  0 / 255f, 233 / 255f, 110 / 255f),
            new Color(  0 / 255f, 232 / 255f, 156 / 255f),
            new Color(  0 / 255f, 231 / 255f, 195 / 255f),
            new Color(  0 / 255f, 227 / 255f, 225 / 255f),
            new Color(  0 / 255f, 222 / 255f, 245 / 255f),
            new Color(  0 / 255f, 216 / 255f, 255 / 255f),
            new Color( 59 / 255f, 209 / 255f, 255 / 255f),
            new Color(125 / 255f, 201 / 255f, 248 / 255f),
            new Color(160 / 255f, 195 / 255f, 234 / 255f),
        };

        /// <summary>
        /// Maximum bounces when calculating movement.
        /// </summary>
        [SerializeField]
        [Tooltip("Maximum bounces when calculating movement.")]
        public int maxBounces = 5;

        /// <summary>
        /// Step up height for stairs.
        /// </summary>
        [SerializeField]
        public float stepHeight = 0.35f;

        /// <summary>
        /// Step depth for stairs.
        /// </summary>
        [SerializeField]
        public float stepDepth = 0.1f;

        /// <summary>
        /// Color of overlapping objects for the character collider gizmo.
        /// </summary>
        [SerializeField]
        [Tooltip("Color of overlapping objects for the character collider gizmo.")]
        public Color overlapColor = Color.red;

        [SerializeField]
        public float maxDrawDistance = 10.0f;

        [SerializeField]
        public float drawMoveRate = 0.5f;

        [SerializeField]
        public float drawDelay = 1.0f;

        [SerializeField]
        public float elapsed = 0.0f;

        public void Update()
        {
            elapsed += Time.deltaTime;

            movementDistance = 0.0f;

            if (elapsed >= drawDelay)
            {
                movementDistance = Mathf.Min((elapsed - drawDelay) * drawMoveRate, maxDrawDistance);
            }
        }
    }
}
