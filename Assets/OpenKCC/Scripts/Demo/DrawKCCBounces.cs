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

using System.Collections.Generic;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;
using static nickmaltbie.OpenKCC.Utils.KCCUtils;

namespace nickmaltbie.OpenKCC.Demo
{
    /// <summary>
    /// Draw the bounces using the gizmos in unity. 
    /// </summary>
    [RequireComponent(typeof(KinematicCharacterController))]
    public class DrawKCCBounces : MonoBehaviour
    {
        private KinematicCharacterController kcc;

        public bool drawInitialCollider = false;

        public bool drawOverlapColliders = true;

        public float movementDistance = 5.0f;

        public void Start()
        {
            kcc = GetComponent<KinematicCharacterController>();
        }

        public void OnDrawGizmos()
        {
            Vector3 movement = kcc.GetProjectedMovement().normalized * movementDistance;
            // Get the bounces the player's movement would make
            _ = new List<(Vector3, Ray, MovementAction)>(
                KCCUtils.GetBounces(
                    kcc.MaxBounces,
                    kcc.PushDecay,
                    kcc.VerticalSnapUp,
                    kcc.StepUpDepth,
                    kcc.AnglePower,
                    kcc.AttemptingJump,
                    kcc.CanSnapUp,
                    kcc.transform.position,
                    movement,
                    kcc.Up,
                    kcc.capsuleColliderCast,
                    kcc.characterPush));

        }
    }
}
