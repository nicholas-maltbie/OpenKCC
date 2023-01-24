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

using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.MoleKCCSample
{
    [RequireComponent(typeof(Rigidbody))]
    public class MoleMovementEngine : KCCMovementEngine
    {
        /*
        /// <inheritdoc/>
        protected override Vector3 GetMovement(
            Vector3 position,
            Vector3 movement,
            Quaternion rotation)
        {
            Vector3 finalPos = position;
            foreach (KCCBounce bounce in KCCUtils.GetBounces(position, movement, rotation, config))
            {
                if (bounce.action == KCCUtils.MovementAction.Bounce)
                {
                    // If we bounce off a wall perpendicular to the current surface
                    Vector3 normal = bounce.hit.normal;
                    if (normal != Vector3.zero)
                    {
                        // Rotate the remaining movement
                        bounce.remainingMomentum = Quaternion.LookRotation(bounce.hit.normal) * bounce.remainingMomentum;

                        // Adjust up vector for grounded state
                        if (groundedState is KCCGroundedState ground)
                        {
                            ground.SurfaceNormal = bounce.hit.normal;
                            ground.OnGround = true;
                            ground.DistanceToGround = KCCUtils.Epsilon;
                        }
                    }
                }
                else if (bounce.action == KCCUtils.MovementAction.Stop)
                {
                    finalPos = bounce.finalPosition;
                }
            }

            return finalPos - position;
        }*/
    }
}
