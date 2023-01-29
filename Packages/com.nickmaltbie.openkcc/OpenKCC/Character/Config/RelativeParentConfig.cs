﻿// Copyright (C) 2023 Nicholas Maltbie
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
    [Serializable]
    public struct RelativeParentConfig
    {
        /// <summary>
        /// Relative position in local space.
        /// </summary>
        public Vector3 relativePos;

        /// <summary>
        /// Previous parent for saving relative transform position.
        /// </summary>
        public Transform previousParent;

        /// <summary>
        /// Reset the relative parent transform.
        /// </summary>
        public void Reset()
        {
            relativePos = Vector3.zero;
            previousParent = null;
        }

        /// <summary>
        /// Move the transform's position
        /// to be the same relative position to parent as the saved position.
        /// </summary>
        /// <param name="transform">Transform to move.</param>
        public void FollowGround(Transform transform)
        {
            if (previousParent != null)
            {
                transform.position = previousParent.position + previousParent.rotation * relativePos;
            }
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
        public Vector3 UpdateMovingGround(Vector3 position, IKCCGrounded groundedState, Vector3 delta, float deltaTime)
        {
            if (groundedState.StandingOnGround && groundedState.Floor != null)
            {
                Transform parent = groundedState.Floor?.transform;
                IMovingGround ground = parent.GetComponent<IMovingGround>();

                if (ground == null || ground.ShouldAttach())
                {

                    if (parent != previousParent)
                    {
                        relativePos = position + delta - parent.position;
                        relativePos = Quaternion.Inverse(parent.rotation) * relativePos;
                    }
                    else
                    {
                        relativePos += Quaternion.Inverse(parent.rotation) * delta;
                    }

                    previousParent = parent;
                }
                else
                {
                    previousParent = null;
                    return ground.GetVelocityAtPoint(groundedState.GroundHitPosition) * deltaTime;
                }
            }
            else
            {
                Reset();
            }

            return Vector3.zero;
        }
    }
}
