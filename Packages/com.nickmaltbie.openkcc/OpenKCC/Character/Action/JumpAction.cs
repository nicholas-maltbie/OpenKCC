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
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Input;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Character.Action
{
    /// <summary>
    /// Jump action that can be performed by a character controller.
    /// </summary>
    [Serializable]
    public class JumpAction : ConditionalAction
    {
        [Header("Player Jump Settings")]

        /// <summary>
        /// Action reference for jumping.
        /// </summary>
        [Tooltip("Action reference for jumping.")]
        [SerializeField]
        public BufferedInput jumpInput;

        /// <summary>
        /// Velocity of player jump in units per second.
        /// </summary>
        [Tooltip("Vertical velocity of player jump.")]
        [SerializeField]
        public float jumpVelocity = 5.0f;

        /// <summary>
        /// Maximum angle at which the player can jump (in degrees).
        /// </summary>
        [Tooltip("Maximum angle at which the player can jump (in degrees).")]
        [SerializeField]
        [Range(0, 90)]
        public float maxJumpAngle = 85f;

        /// <summary>
        /// Weight to which the player's jump is weighted towards the direction
        /// of the surface they are standing on.
        /// </summary>
        [Tooltip("Weight to which the player's jump is weighted towards the angle of their surface.")]
        [SerializeField]
        [Range(0, 1)]
        public float jumpAngleWeightFactor = 0.0f;

        /// <summary>
        /// Grounded state for managing player grounded configuration.
        /// </summary>
        private IKCCGrounded kccGrounded;

        /// <summary>
        /// Actor to apply jumps to.
        /// </summary>
        private IJumping actor;

        /// <summary>
        /// Configuration of the character controller.
        /// </summary>
        private IKCCConfig kccConfig;

        /// <summary>
        /// Has the player jumped while they are sliding.
        /// </summary>
        public bool JumpedWhileSliding { get; private set; }

        /// <summary>
        /// Setup this jump action.
        /// </summary>
        public void Setup(IKCCGrounded kccGrounded, IKCCConfig kccConfig, IJumping actor)
        {
            base.condition = CanJump;
            this.kccGrounded = kccGrounded;
            this.kccConfig = kccConfig;
            this.actor = actor;
            JumpedWhileSliding = false;

            jumpInput.InputAction?.Enable();
        }

        /// <inheritdoc/>
        public override void Update()
        {
            base.Update();
            jumpInput?.Update();

            if (kccGrounded != null && kccGrounded.StandingOnGround && !kccGrounded.Sliding)
            {
                JumpedWhileSliding = false;
            }
        }

        /// <summary>
        /// Apply the jump action to the actor if attempting
        /// jump and the player can jump.
        /// </summary>
        /// <returns>True if the player jumped, false otherwise.</returns>
        public bool ApplyJumpIfPossible()
        {
            if (AttemptingJump && CanPerform)
            {
                Jump();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Apply the jump to the player.
        /// </summary>
        public void Jump()
        {
            if (kccGrounded.Sliding)
            {
                JumpedWhileSliding = true;
            }

            Vector3 jumpDirection = (kccGrounded.StandingOnGround ? kccGrounded.SurfaceNormal : kccConfig.Up) *
                jumpAngleWeightFactor + kccConfig.Up * (1 - jumpAngleWeightFactor);
            actor.ApplyJump(jumpVelocity * jumpDirection.normalized);
            jumpInput.Reset();
        }

        /// <summary>
        /// Is the player currently attempting to jump
        /// </summary>
        public bool AttemptingJump => jumpInput.Pressed;

        /// <summary>
        /// Can the player jump based on their current state.
        /// </summary>
        /// <returns>True if the player can jump, false otherwise.</returns>
        public bool CanJump()
        {
            bool canJump = kccGrounded.StandingOnGround && kccGrounded.Angle <= maxJumpAngle;
            if (canJump && !kccGrounded.Sliding)
            {
                return true;
            }
            else if (canJump)
            {
                return !JumpedWhileSliding;
            }

            return false;
        }
    }
}
