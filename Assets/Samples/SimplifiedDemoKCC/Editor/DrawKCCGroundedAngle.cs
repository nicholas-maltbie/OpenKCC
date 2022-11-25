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

using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace nickmaltbie.OpenKCC.Demo.Editor
{
    /// <summary>
    /// Draw angle the kcc makes between the ground and their player as a debug gizmo.
    /// </summary>
    [RequireComponent(typeof(KCCStateMachine))]
    public class DrawKCCGroundedAngle : MonoBehaviour
    {
        /// <summary>
        /// Kinematic character controller reference.
        /// </summary>
        private KCCStateMachine kcc;

        /// <summary>
        /// Collider cast for checking movement and collision of the character controller.
        /// </summary>
        private IColliderCast colliderCast;

        /// <summary>
        /// How far should the downward cast be made.
        /// </summary>
        public float groundCheckDistance = 1.0f;

        /// <summary>
        /// How far is considered grounded.
        /// </summary>
        public float groundedDistance = 0.0f;

        /// <summary>
        /// Color of player when standing on the ground.
        /// </summary>
        [SerializeField]
        [Tooltip("Color of player when standing on the ground.")]
        public Color groundedColor = Color.blue;

        /// <summary>
        /// Color of of player when over the sliding threshold.
        /// </summary>
        [SerializeField]
        [Tooltip("Color of of player when over the sliding threshold.")]
        public Color slidingColor = Color.yellow;

        /// <summary>
        /// Alpha value for drawing the angle arc.
        /// </summary>
        [SerializeField]
        [Tooltip("Alpha value for drawing the angle arc.")]
        [Range(0, 1)]
        public float fillAlpha = 0.5f;

        /// <summary>
        /// Length of arc for drawing character angle.
        /// </summary>
        [SerializeField]
        [Tooltip("Length of arc for drawing character angle.")]
        [Range(0, 2)]
        public float debugAngleLength = 0.5f;

        /// <summary>
        /// Treshold angle for when player is considered sliding.
        /// </summary>
        [SerializeField]
        [Tooltip("Treshold angle for when player is considered sliding.")]
        [Range(0, 90.0f)]
        public float thresholdAngle = 60f;

        /// <summary>
        /// Radius of grounded draw point.
        /// </summary>
        [SerializeField]
        [Tooltip("Radius of grounded draw point.")]
        public float pointRadius = 0.01f;

        /// <summary>
        /// Should the ground position be used to draw the angle or should it be drawn from the origin of the kcc's
        /// feet. 
        /// </summary>
        [SerializeField]
        [Tooltip("Location to draw angle, at ground position (true) or feet (false).")]
        public bool useGroundPosition = true;

        public void Start()
        {
            kcc = GetComponent<KCCStateMachine>();
            colliderCast = GetComponent<IColliderCast>();
        }

        public void OnDrawGizmos()
        {
            if (kcc == null)
            {
                kcc = GetComponent<KCCStateMachine>();
            }

            if (colliderCast == null)
            {
                colliderCast = GetComponent<IColliderCast>();
            }

            bool hitGround = colliderCast.CastSelf(
                transform.position,
                transform.rotation,
                Vector3.down,
                groundCheckDistance,
                out IRaycastHit hit);

            if (hitGround)
            {
                bool isGrounded = hitGround && hit.distance <= groundedDistance;

                Vector3 source = useGroundPosition ? hit.point : colliderCast.GetBottom(
                    transform.position, transform.rotation);

                float angle = Vector3.Angle(kcc.config.Up, hit.normal);
                bool sliding = angle >= thresholdAngle;

                Color selectedColor = sliding ? slidingColor : groundedColor;
                Gizmos.DrawWireSphere(hit.point, pointRadius);
                Gizmos.color = selectedColor;
                Gizmos.DrawRay(source, Vector3.up * debugAngleLength);
                Gizmos.DrawRay(source, hit.normal * debugAngleLength);

#if UNITY_EDITOR
                Handles.color = selectedColor;
                Handles.DrawWireArc(source, Vector3.Cross(hit.normal, kcc.config.Up), kcc.config.Up, -angle, debugAngleLength);
                Handles.color = new Color(selectedColor.r, selectedColor.g, selectedColor.b, fillAlpha);
                Handles.DrawSolidArc(source, Vector3.Cross(hit.normal, kcc.config.Up), kcc.config.Up, -angle, debugAngleLength);
#endif
            }
        }
    }
}
