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
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.MoleKCCSample
{
    /// <summary>
    /// MovementEngine for the mole character to allow the player to
    /// walk up and down walls directly.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class MoleMovementEngine : KCCMovementEngine
    {
        /// <summary>
        /// Have no angle decay in movement up walls.
        /// </summary>
        public override float AnglePower => 1.0f;

        /// <summary>
        /// Surface normal for overriding up direction.
        /// </summary>
        protected Vector3 groundNormal = Vector3.up;

        /// <summary>
        /// Should the grounded angle be overwritten.
        /// </summary>
        public bool overrideGrounded = false;

        /// <summary>
        /// Set the normal vector for the ground the mole is standing on.
        /// </summary>
        /// <param name="normal"></param>
        public void SetNormal(Vector3 normal)
        {
            groundNormal = normal;
            overrideGrounded = true;
        }

        /// <summary>
        /// Override the up direction for the mole.
        /// </summary>
        public override Vector3 Up
        {
            get
            {
                if (overrideGrounded)
                {
                    return groundNormal;
                }

                if (GroundedState.StandingOnGround)
                {
                    return GroundedState.SurfaceNormal;
                }

                return Vector3.up;
            }
        }

        /// <inheritdoc/>
        protected override IEnumerable<KCCBounce> GetMovement(Vector3 movement)
        {
            foreach (KCCBounce bounce in base.GetMovement(movement))
            {
                if (bounce.action == KCCUtils.MovementAction.Bounce || bounce.action == KCCUtils.MovementAction.Stop)
                {
                    if (bounce.hit == null)
                    {
                        yield return bounce;
                        continue;
                    }

                    // If we bounce off a wall perpendicular to the current surface
                    Vector3 normal = bounce.hit.normal;
                    if (normal != Vector3.zero)
                    {
                        // Rotate the remaining movement
                        bounce.remainingMomentum = Quaternion.LookRotation(bounce.hit.normal) *
                            bounce.initialMomentum.normalized *
                            bounce.remainingMomentum.magnitude;
                    }

                    SetNormal(bounce.hit.normal);
                }

                yield return bounce;
            }

            // Compute the new grounded state
            CheckGrounded(false);
            SetNormal(GroundedState.SurfaceNormal);
        }
    }
}
