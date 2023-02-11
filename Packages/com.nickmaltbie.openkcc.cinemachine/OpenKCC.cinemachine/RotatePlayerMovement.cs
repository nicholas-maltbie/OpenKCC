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

using nickmaltbie.OpenKCC.CameraControls;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.cinemachine
{
    /// <summary>
    /// Rotate a player avatar to face in the
    /// direction of their movement.
    /// </summary>
    [RequireComponent(typeof(KCCStateMachine))]
    [RequireComponent(typeof(ICameraControls))]
    public class RotatePlayerMovement : MonoBehaviour
    {
        /// <summary>
        /// Avatar base to rotate.
        /// </summary>
        public GameObject avatarBase;

        /// <summary>
        /// Rotation speed in degrees per second.
        /// </summary>
        public float rotationSpeed = 15.0f;

        public void LateUpdate()
        {
            KCCStateMachine sm = GetComponent<KCCStateMachine>();
            ICameraControls cameraControls = GetComponent<ICameraControls>();

            // Rotate the avatar to follow the direction of the
            // player's movement if it's grater than some dead zone
            // with a given turning rate.
            Quaternion currentHeading = avatarBase.transform.rotation;
            var desiredHeading = Quaternion.LookRotation(cameraControls.PlayerHeading * sm.InputMovement);

            if (sm.InputMovement.magnitude > KCCUtils.Epsilon)
            {
                // Rotate the current heading towards the desired heading.
                var rotatedHeading = Quaternion.RotateTowards(currentHeading, desiredHeading, rotationSpeed * Time.deltaTime);

                // set the avatar rotation to be the new rotated heading.
                avatarBase.transform.rotation = rotatedHeading;
            }
        }
    }
}
