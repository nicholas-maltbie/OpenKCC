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
using nickmaltbie.OpenKCC.Character.Attributes;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Character
{
    /// <summary>
    /// Movement engine for the kcc state machine
    /// to abstract calls to the KCC Utils
    /// for a basic character controller.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(IGetKCCConfig))]
    [RequireComponent(typeof(IGetKCCGrounded))]
    public class KCCMovementEngine : MonoBehaviour, IJumping
    {
        private IKCCConfig _config;
        private IKCCGrounded _grounded;

        /// <summary>
        /// KCC Configuration for managing the kcc state machine.
        /// </summary>
        public IKCCConfig config => _config ??= GetComponent<IGetKCCConfig>().kccConfig;

        /// <summary>
        /// KCC Grounded state for the movement engine.
        /// </summary>
        public IKCCGrounded groundedState => _grounded ??= GetComponent<IGetKCCGrounded>().kccGrounded;

        /// <summary>
        /// Position of the player previous frame.
        /// </summary>
        protected Vector3 previousPosition;

        /// <summary>
        /// Velocity of the player from the previous frame.
        /// </summary>
        protected Vector3 previousVelocity;

        /// <summary>
        /// Relative parent configuration for following the ground.
        /// </summary>
        protected RelativeParentConfig relativeParentConfig;

        /// <summary>
        /// Velocity fo the player from the previous frame.
        /// </summary>
        /// <value></value>
        public Vector3 Velocity { get; protected set; }

        /// <summary>
        /// Time in which the player has been falling.
        /// </summary>
        public float FallingTime { get; protected set; }

        /// <summary>
        /// Apply movement of a player based on current state.
        /// </summary>
        /// <param name="position">Current position of the player.</param>
        /// <param name="rotation">Current rotation of the player.</param>
        /// <param name="deltaTime">Delta time for the movement.</param>
        /// <param name="moveSettings">Move settings for the current action.</param>
        /// <returns>Delta in position due to player movement.</returns>
        public Vector3 MovePlayer(
            Vector3 position,
            Quaternion rotation,
            Vector3 move,
            float deltaTime,
            MovementSettingsAttribute moveSettings)
        {
            // Move the player based on movement settings
            Vector3 delta = Vector3.zero;

            // Move the player if they are allowed to walk
            if (moveSettings?.AllowWalk ?? false)
            {
                Vector3 moveDelta = GetMovement(position, move, rotation, config);
                delta += moveDelta;
                position += moveDelta;
            }

            // Apply velocity if allowed to move via velocity
            // Only snap down if the player is currently grounded
            if (groundedState.StandingOnGround && (moveSettings?.SnapPlayerDown ?? false))
            {
                Vector3 snapDelta = GetSnapDelta(
                    position,
                    transform.rotation,
                    -config.Up,
                    config.VerticalSnapDown,
                    config.MinSnapThreshold,
                    config.ColliderCast);
                delta += snapDelta;
                position += snapDelta;
            }

            // Apply velocity if allowed to move via velocity
            if (moveSettings?.AllowVelocity ?? false)
            {
                Vector3 velDelta = GetMovement(position, Velocity * deltaTime, rotation, config);
                delta += velDelta;
            }
            else
            {
                Velocity = Vector3.zero;
            }

            return delta;
        }

        
        /// <summary>
        /// Snap the player down onto the ground
        /// </summary>
        /// <param name="position">Position of the kcc</param>
        /// <param name="rotation">Rotation of the kcc.</param>
        /// <param name="dir">Direction to snap the kcc down.</param>
        /// <param name="dist">Maximum distance the kcc can snap.</param>
        /// <param name="minSnapThreshold">Minimum snap threshold for snapping down.</param>
        /// <param name="colliderCast">Collider cast component associated with the KCC.</param>
        /// <returns></returns>
        public static Vector3 GetSnapDelta(
            Vector3 position,
            Quaternion rotation,
            Vector3 dir,
            float dist,
            float minSnapThreshold,
            IColliderCast colliderCast)
        {
            bool didHit = colliderCast.CastSelf(
                position + dir * KCCUtils.Epsilon,
                rotation,
                dir,
                dist,
                out IRaycastHit hit);

            if (didHit && hit.distance > minSnapThreshold)
            {
                return dir * (hit.distance - KCCUtils.Epsilon);
            }

            return Vector3.zero;
        }

        /// <summary>
        /// Snap the player down onto the ground
        /// </summary>
        /// <param name="position">Position of the kcc</param>
        /// <param name="rotation">Rotation of the kcc.</param>
        /// <param name="dir">Direction to snap the kcc down.</param>
        /// <param name="dist">Maximum distance the kcc can snap.</param>
        /// <param name="minSnapThreshold">Minimum snap threshold for snapping down.</param>
        /// <param name="colliderCast">Collider cast component associated with the KCC.</param>
        /// <returns></returns>
        public static Vector3 SnapPlayerDown(
            Vector3 position,
            Quaternion rotation,
            Vector3 dir,
            float dist,
            float minSnapThreshold,
            IColliderCast colliderCast)
        {
            return position + GetSnapDelta(position, rotation, dir, dist, minSnapThreshold, colliderCast);
        }

        /// <summary>
        /// Get the bounces for a KCC Utils movement action with a set default behaviour.
        /// </summary>
        /// <param name="position">Position to start player movement from.</param>
        /// <param name="movement">Movement to move the player.</param>
        /// <param name="rotation">Rotation of the player during movement.</param>
        /// <param name="config">Configuration settings for player movement.</param>
        /// <returns>Bounces that the player makes when hitting objects as part of it's movement.</returns>
        public static Vector3 GetMovement(
            Vector3 position,
            Vector3 movement,
            Quaternion rotation,
            IKCCConfig config)
        {
            Vector3 finalPos = position;
            foreach (KCCBounce bounce in KCCUtils.GetBounces(position, movement, rotation, config))
            {
                if (bounce.action == KCCUtils.MovementAction.Stop)
                {
                    finalPos = bounce.finalPosition;
                }
            }

            return finalPos - position;
        }

        /// <inheritdoc/>
        public void Update()
        {
            relativeParentConfig.FollowGround(transform);
        }

        /// <summary>
        /// Apply movement of the character.
        /// </summary>
        /// <param name="deltaTime">Delta time.</param>
        /// <param name="move">Desired move for the character.</param>
        /// <param name="currentState">Current state of the character
        /// to load movement settings from.</param>
        public void MovePlayer(
            float deltaTime,
            Vector3 move,
            Type currentState = null)
        {
            bool gravity = currentState != null ?
                Attribute.GetCustomAttribute(
                    currentState,
                    typeof(ApplyGravity)) is ApplyGravity :
                false;

            var moveSettings = currentState != null ?
                Attribute.GetCustomAttribute(
                    currentState,
                    typeof(MovementSettingsAttribute)) as MovementSettingsAttribute :
                null;

            ApplyMovement(
                deltaTime,
                move,
                gravity,
                moveSettings
            );
        }

        /// <summary>
        /// Applies player movement based on current state.
        /// Includes pushing out overlapping objects, updating grounded state, jumping,
        /// moving the player, and updating the grounded state.
        /// </summary>
        /// <param name="deltaTime">Delta time for the update.</param>
        /// <param name="move">Desired player movement.</param>
        /// <param name="applyGravity">Should gravity be applied.</param>
        /// <param name="moveSettings">Movement settings for player.</param>
        public virtual void ApplyMovement(
            float deltaTime,
            Vector3 move,
            bool applyGravity,
            MovementSettingsAttribute moveSettings)
        {
            relativeParentConfig.FollowGround(transform);
            Vector3 vel = (transform.position - previousPosition) / deltaTime;
            previousVelocity = Vector3.Lerp(previousVelocity, vel, 20 * deltaTime);

            Vector3 start = transform.position;
            Vector3 pos = start;

            // Push player out of overlapping objects
            Vector3 overlapPush = config.ColliderCast.PushOutOverlapping(
                pos,
                transform.rotation,
                config.MaxPushSpeed * deltaTime);
            pos += overlapPush;

            // Allow player to move
            Vector3 playerMove = MovePlayer(
                pos,
                transform.rotation,
                move,
                deltaTime,
                moveSettings);
            pos += playerMove;

            // Compute player relative movement state based on final pos
            Vector3 delta = pos - start;
            groundedState.CheckGrounded(config, pos, transform.rotation);

            // Check if player is falling
            if (groundedState.Falling)
            {
                FallingTime += deltaTime;
            }
            else
            {
                FallingTime = 0;
            }

            // Apply gravity if needed
            if (applyGravity)
            {
                Velocity += config.Gravity * deltaTime;
            }

            transform.position += delta;
            transform.position += relativeParentConfig.UpdateMovingGround(transform.position, groundedState, delta, deltaTime);
            relativeParentConfig.FollowGround(transform);
            previousPosition = transform.position;
        }

        /// <summary>
        /// Teleport player to a given position.
        /// </summary>
        /// <param name="position">Position to teleport player to.</param>
        public void TeleportPlayer(Vector3 position)
        {
            relativeParentConfig.Reset();
            transform.position = position;
        }

        /// <inheritdoc/>
        public void ApplyJump(Vector3 velocity)
        {
            Vector3 groundVel = KCCUtils.GetGroundVelocity(groundedState, config, previousVelocity);
            Velocity = velocity + groundVel;
            relativeParentConfig.Reset();
        }
    }
}
