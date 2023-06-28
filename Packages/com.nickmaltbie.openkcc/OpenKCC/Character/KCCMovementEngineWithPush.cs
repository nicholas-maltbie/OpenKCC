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
using nickmaltbie.OpenKCC.Environment.Pushable;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Basic extension of KCCMovementEngine that allows for
    /// pushing rigidbody objects that it collides with that
    /// have the attached MonoBehaviour IPushable.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(IColliderCast))]
    public class KCCMovementEngineWithPush : KCCMovementEngine, ICharacterPush
    {
        /// <summary>
        /// Power of the player push.
        /// </summary>
        [Tooltip("Power of the player push.")]
        public float pushPower = 4.0f;

        /// <summary>
        /// Decay in momentum when pushing an object.
        /// Zero indicates all momentum is lost while 1 indicates all momentum
        /// is maintained.
        /// </summary>
        [Range(0, 1)]
        [Tooltip("Decay in momentum when pushing an object.")]
        public float pushDecay = 0.8f;

        /// <inheritdoc/>
        public bool CanPushObject(Collider hit)
        {
            return hit.attachedRigidbody != null &&
                !hit.attachedRigidbody.isKinematic &&
                hit.gameObject.GetComponent<IPushable>() != null;
        }

        /// <inheritdoc/>
        public void PushObject(IControllerColliderHit hit)
        {
            // Check if the thing we hit can be pushed
            Rigidbody body = hit.rigidbody;
            IPushable pushable = hit.gameObject.GetComponent<IPushable>();

            // Do nothing if the object does not have a rigidbody or if
            //   the rigidbody is kinematic
            if (body == null || body.isKinematic || pushable == null)
            {
                return;
            }

            // If to the side, use the controller velocity
            // Project movement vector onto plane defined by gravity normal (horizontal plane)
            Vector3 force = Vector3.ProjectOnPlane(hit.moveDirection, Vector3.down) * pushPower;

            Vector3 pushForce = force * pushPower;

            // Apply the push
            body.AddForceAtPosition(pushForce, hit.point, ForceMode.Force);
            pushable.PushObject(
                pushForce,
                hit.point,
                (int)ForceMode.Force);
        }

        /// <inheritdoc/>
        public override IEnumerable<KCCBounce> GetMovement(Vector3 movement)
        {
            foreach (KCCBounce bounce in base.GetMovement(movement))
            {
                if (bounce.action == KCCUtils.MovementAction.Bounce && CanPushObject(bounce.hit?.collider))
                {
                    Collider collider = bounce.hit.collider;
                    PushObject(new KinematicCharacterControllerHit(
                        collider,
                        bounce.hit.rigidbody,
                        collider.gameObject,
                        collider.transform,
                        bounce.hit.point,
                        bounce.hit.normal,
                        bounce.initialMomentum.normalized,
                        movement.magnitude
                    ));

                    // If pushing something, reduce remaining force significantly
                    bounce.remainingMomentum *= pushDecay;
                }

                yield return bounce;
            }
        }
    }
}
