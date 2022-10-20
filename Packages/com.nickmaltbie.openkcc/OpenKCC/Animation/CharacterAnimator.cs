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

using UnityEngine;

namespace nickmaltbie.OpenKCC.Animation
{
    public struct CharacterAnimatorState
    {
        public Vector2 move;
        public bool moving;
        public float rotation;
        public bool turning;
        public bool jumping;
        public bool falling;
        public bool longFalling;
    }

    public class CharacterAnimator : MonoBehaviour
    {
        /// <summary>
        /// Dead zone to consider movement as stopped
        /// </summary>
        public float movementDeadZone = 0.01f;

        /// <summary>
        /// Dead zone to consider turning action as stopped
        /// </summary>
        public float turningDeadZone = 10f;

        /// <summary>
        /// Amount of time falling to switch to falling animation
        /// </summary>
        public float fallingThreshold = 0.1f;

        /// <summary>
        /// Falling time before player goes into long falling animation
        /// </summary>
        public float longFallingThreshold = 1.5f;

        /// <summary>
        /// Character controller for getting character motion information
        /// </summary>
        public KCCStateMachine kcc;

        /// <summary>
        /// Camera controller for getting player rotation information
        /// </summary>
        public CameraController cameraController;

        /// <summary>
        /// Animator for controlling character
        /// </summary>
        public Animator animator;

        private CharacterAnimatorState animState = new CharacterAnimatorState();

        public void Update()
        {
            if (animator == null)
            {
                return;
            }

            bool jumping = kcc.jumpAction.CanJump() && kcc.jumpAction.AttemptingJump;
            bool falling = kcc.FallingTime >= fallingThreshold;
            bool jumpingOrFalling = falling || jumping;
            bool moving = !jumpingOrFalling && kcc.InputMovement.magnitude > movementDeadZone;

            animState.move = new Vector2(kcc.InputMovement.x, kcc.InputMovement.z);
            animState.moving = moving;
            animState.rotation = Mathf.Lerp(animState.rotation, cameraController.frameRotation > 0 ? 1 : -1, 0.5f);
            animState.turning = !moving &&
                !jumpingOrFalling &&
                Mathf.Abs(cameraController.frameRotation) > turningDeadZone;
            animState.jumping = jumping;
            animState.falling = falling;
            animState.longFalling = kcc.FallingTime >= longFallingThreshold;

            // Set animator fields based on information
            animator.SetFloat("MoveX", animState.move.x);
            animator.SetFloat("MoveY", animState.move.y);
            // Set moving if movement is greater than dead zone
            animator.SetBool("Moving", animState.moving);
            // Set turning value based on turning direction
            animator.SetFloat("Rotation", animState.rotation);
            animator.SetBool("Turning", animState.turning);
            animator.SetBool("Jumping", animState.jumping);
            animator.SetBool("Falling", animState.falling);
            animator.SetBool("Long Falling", animState.longFalling);
        }
    }
}
