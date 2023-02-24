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
    public class MoleMovementEngine : KCCMovementEngine
    {
        /// <summary>
        /// Have no angle decay in movement up walls.
        /// </summary>
        public override float AnglePower => 1.0f;

        /// <summary>
        /// Surface normal for overriding up direction.
        /// </summary>
        protected Vector3 GroundNormal { get; set; } = -Physics.gravity.normalized;

        /// <summary>
        /// Max speed that the player can snap down at.
        /// </summary>
        public override float MaxSnapDownSpeed => Mathf.Infinity;

        /// <summary>
        /// Override the up direction for the mole.
        /// </summary>
        public override Vector3 Up => GroundNormal;

        /// <inheritdoc/>
        public override IEnumerable<KCCBounce> GetMovement(Vector3 movement)
        {
            foreach (KCCBounce bounce in base.GetMovement(movement))
            {
                if (bounce.action == KCCUtils.MovementAction.Bounce)
                {
                    if (bounce.hit == null)
                    {
                        yield return bounce;
                        continue;
                    }

                    // If we bounce off a wall perpendicular to the current surface
                    Vector3 normal = bounce.hit.normal;
                    float remainingSpeed = bounce.initialMomentum.magnitude - bounce.Movement.magnitude;
                    if (normal == Vector3.zero)
                    {
                        normal = GroundNormal;
                    }

                    bool perpendicular = Vector3.Dot(normal, bounce.initialMomentum) <= KCCUtils.Epsilon;
                    if (perpendicular)
                    {
                        bounce.remainingMomentum = GroundNormal * remainingSpeed;
                    }
                    else
                    {
                        // Rotate the remaining movement
                        bounce.remainingMomentum = Quaternion.LookRotation(bounce.hit.normal) *
                            bounce.initialMomentum.normalized * remainingSpeed;
                    }

                    GroundNormal = normal;
                }

                yield return bounce;
            }

            // Only snap down if the player was grounded before they started
            // moving and are not currently trying to move upwards.
            if (GroundedState.StandingOnGround && !GroundedState.Sliding && !MovingUp(movement))
            {
                SnapPlayerDown();
            }

            // Compute the new grounded state
            CheckGrounded(false);
            GroundNormal = GroundedState.SurfaceNormal;
        }
    }
}
