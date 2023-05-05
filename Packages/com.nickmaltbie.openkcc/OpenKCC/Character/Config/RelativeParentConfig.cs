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

using System;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Character.Config
{
    /// <summary>
    /// Relative parent configuration for saving
    /// the position of an object relative to a given parent object.
    /// </summary>
    public class RelativeParentConfig
    {
        /// <summary>
        /// Relative position in local space.
        /// </summary>
        public Vector3 RelativePos { get; internal set; }

        /// <summary>
        /// Relative rotation in local space.
        /// </summary>
        public Quaternion RelativeRotation { get; internal set; }

        /// <summary>
        /// Previous parent for saving relative transform position.
        /// </summary>
        public Transform PreviousParent { get; internal set; }

        /// <summary>
        /// Check if the player is standing on moving ground.
        /// </summary>
        public bool OnMovingGround => PreviousParent != null;

        /// <summary>
        /// Reset the relative parent transform.
        /// </summary>
        public void Reset()
        {
            RelativePos = Vector3.zero;
            PreviousParent = null;
        }

        /// <summary>
        /// Update the moving ground state for a given
        /// relative parent for some given movement.
        /// </summary>
        /// <param name="position">Position of object before moving.</param>
        /// <param name="groundedState">Current grounded state of the object.</param>
        /// <param name="delta">Delta that the object has moved.</param>
        /// <param name="deltaTime">Delta time for update.</param>
        /// <returns>The distance the player should be moved based
        /// on moving ground. Will only be more than Vector3.zero if the ground
        /// is moving and should not attach.</returns>
        public virtual Vector3 UpdateMovingGround(Vector3 position, IKCCGrounded groundedState, Vector3 delta, float deltaTime)
        {
            if (groundedState.StandingOnGround && groundedState.Floor != null)
            {
                Transform parent = groundedState.Floor?.transform;
                IMovingGround ground = parent.GetComponent<IMovingGround>();

                if (ground == null || ground.ShouldAttach())
                {
                    if (parent != PreviousParent)
                    {
                        RelativePos = position + delta - parent.position;
                        RelativePos = Quaternion.Inverse(parent.rotation) * RelativePos;
                    }
                    else
                    {
                        RelativePos += Quaternion.Inverse(parent.rotation) * delta;
                    }

                    PreviousParent = parent;
                }
                else
                {
                    PreviousParent = null;
                    return ground.GetVelocityAtPoint(groundedState.GroundHitPosition) * deltaTime;
                }
            }
            else
            {
                Reset();
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Move the transform's position
        /// to be the same relative position to parent as the saved position.
        /// </summary>
        /// <param name="transform">Transform to move.</param>
        public virtual void FollowGround(Transform transform)
        {
            transform.position += DeltaPosition(transform);
        }

        /// <summary>
        /// Compute the delta in position required to move the character
        /// with the moving ground for this update.
        /// </summary>
        /// <param name="transform">Transform to move.</param>
        /// <returns>Delta in world space to move the player.</returns>
        protected Vector3 DeltaPosition(Transform transform)
        {
            if (OnMovingGround)
            {
                return (PreviousParent.position + PreviousParent.rotation * RelativePos) - transform.position;
            }

            return Vector3.zero;
        }
    }
}
