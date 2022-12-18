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

using System;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Character.Config
{
    [Serializable]
    public struct RelativeParentConfig
    {
        public Vector3 relativePos;
        public Transform previousParent;

        public void Reset()
        {
            relativePos = Vector3.zero;
            previousParent = null;
        }

        public void FollowGround(Transform transform)
        {
            if (previousParent != null)
            {
                transform.position = previousParent.position + previousParent.rotation * relativePos;
            }
        }

        public Vector3 UpdateMovingGround(Vector3 position, KCCGroundedState groundedState, Vector3 delta, float deltaTime)
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
