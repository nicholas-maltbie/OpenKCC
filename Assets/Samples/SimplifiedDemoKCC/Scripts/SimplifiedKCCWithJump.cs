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
using System.Linq;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.Demo
{
    /// <summary>
    /// Example of the KCC class with a simplified movement script for learning purposes with an additional feature of a
    /// Jump and snap down mechanic.
    /// </summary>
    [RequireComponent(typeof(CapsuleCollider))]
    public class SimplifiedKCCWithJump : MonoBehaviour
    {
        /// <summary>
        /// Minimum pitch for camera movement.
        /// </summary>
        public const float minPitch = -90;

        /// <summary>
        /// Maximum pitch for camera movement.
        /// </summary>
        public const float maxPitch = 90;

        [Header("Input Actions")]

        /// <summary>
        /// Action realted to moving the camera, should be a two component vector.
        /// </summary>
        [Tooltip("Action with two axis to rotate player camera around.")]
        public InputActionReference lookAround;

        /// <summary>
        /// Action realted to moving the player, should be a two component vector.
        /// </summary>
        [Tooltip("Action with two axis used to move the player around.")]
        public InputActionReference movePlayer;

        /// <summary>
        /// Action realted to moving the jumping, should be a button input.
        /// </summary>
        [Tooltip("Action realted to moving the jumping, should be a button input.")]
        public InputActionReference jumpAction;

        [Header("Camera Settings")]

        /// <summary>
        /// How fast the player can rotate in degrees per second.
        /// </summary>
        [Tooltip("Rotation speed when moving the camera.")]
        [SerializeField]
        public float rotateSpeed = 90f;

        /// <summary>
        /// Transform holding camera position.
        /// </summary>
        [Tooltip("Transform holding camera position.")]
        [SerializeField]
        public Transform cameraTransform;

        [Header("Movement Settings")]

        /// <summary>
        /// Maximum number of bounces when moving the player.
        /// </summary>
        [Tooltip("Maximum number of bounces when moving the player.")]
        [SerializeField]
        public int maxBounces = 5;

        /// <summary>
        /// Player movement speed.
        /// </summary>
        [Tooltip("Move speed of the player character.")]
        [SerializeField]
        public float moveSpeed = 5.0f;

        /// <summary>
        /// Decrease in momentum factor due to angle change when walking.
        /// Should be a positive float value. It's an exponential applied to 
        /// values between [0, 1] so values smaller than 1 create a positive
        /// curve and grater than 1 for a negative curve.
        /// </summary>
        [Tooltip("Decrease in momentum when walking into objects (such as walls) at an angle as an exponential." +
        "Values between [0, 1] so values smaller than 1 create a positive curve and grater than 1 for a negative curve")]
        [SerializeField]
        public float anglePower = 0.5f;

        /// <summary>
        /// Distance that the character can "snap down" vertical steps
        /// </summary>
        [Tooltip("Snap down distance when snapping onto the floor")]
        [SerializeField]
        private float verticalSnapDown = 0.45f;

        /// <summary>
        /// Speed at which the player falls.
        /// </summary>
        [Tooltip("Speed at which the player falls.")]
        [SerializeField]
        public Vector3 gravity = new Vector3(0, -20, 0);

        [Header("Grounded Settings")]

        /// <summary>
        /// Distance at which the player is considered grounded.
        /// </summary>
        [Tooltip("Distance from ground at which the player will stop falling.")]
        [SerializeField]
        public float groundDist = 0.01f;

        /// <summary>
        /// Max angle at which the player can walk.
        /// </summary>
        [Tooltip("Max angle at which the player can walk.")]
        [SerializeField]
        public float maxWalkingAngle = 60f;

        [Header("Jump Settings")]

        /// <summary>
        /// Velocity of player jump in units per second
        /// </summary>
        [Tooltip("Vertical velocity of player jump")]
        [SerializeField]
        private float jumpVelocity = 5.0f;

        /// <summary>
        /// Max angle at which the player can jump.
        /// </summary>
        [Tooltip("Max angle at which the player can jump.")]
        [SerializeField]
        public float maxJumpAngle = 80f;

        /// <summary>
        /// Minimum cooldown time between player jumps.
        /// </summary>
        [Tooltip("Minimum cooldown time between player jumps.")]
        [SerializeField]
        public float jumpCooldown = 0.25f;

        /// <summary>
        /// Time in which player can jump after they walk off the edge off a surface.
        /// </summary>
        [Tooltip("Time in which player can jump after they walk off the edge off a surface.")]
        [SerializeField]
        public float coyoteTime = 0.05f;

        /// <summary>
        /// Time in seconds that an input can be buffered for jumping.
        /// </summary>
        [Tooltip("Time in seconds that an input can be buffered for jumping")]
        [SerializeField]
        private float jumpBufferTime = 0.05f;

        /// <summary>
        /// Weight to which the player's jump is weighted towards the direction
        /// of the surface they are standing on.
        /// </summary>
        [Tooltip("Weight to which the player's jump is weighted towards the angle of their surface")]
        [SerializeField]
        [Range(0, 1)]
        private float jumpAngleWeightFactor = 0.0f;

        /// <summary>
        /// Time since the player last tried to hit the jump button.
        /// </summary>
        private float jumpInputElapsed = Mathf.Infinity;

        /// <summary>
        /// Time since the player last successfully jumped.
        /// </summary>
        private float timeSinceLastJump = 0.0f;

        /// <summary>
        /// Time in which the player has been falling.
        /// </summary>
        private float elapsedFalling = 0f;

        /// <summary>
        /// Has the player jumped while sliding?
        /// </summary>
        private bool notSlidingSinceJump = true;

        /// <summary>
        /// Velocity at which player is moving.
        /// </summary>
        private Vector3 velocity;

        /// <summary>
        /// Current angle at which the player is looking.
        /// </summary>
        private Vector2 cameraAngle;

        /// <summary>
        /// Configuration for capsule collider to compute player collision.
        /// </summary>
        private CapsuleCollider capsuleCollider;

        /// <summary>
        /// Is the player pressing jump action.
        /// </summary>
        private bool jumpInputPressed = false;

        public void Start()
        {
            capsuleCollider = GetComponent<CapsuleCollider>();

            jumpAction.action.performed += value => jumpInputPressed = value.ReadValueAsButton();

            Rigidbody rigidbody = GetComponent<Rigidbody>();
            rigidbody.isKinematic = true;
        }

        public void Update()
        {
            // Read input values from player
            Vector2 cameraMove = lookAround.action.ReadValue<Vector2>();
            Vector2 playerMove = movePlayer.action.ReadValue<Vector2>();

            // If player is not allowed to move, stop player input
            if (PlayerInputUtils.playerMovementState == PlayerInputState.Deny)
            {
                playerMove = Vector2.zero;
                cameraMove = Vector2.zero;
            }

            // Camera move on x (horizontal movement) controls the yaw (look left or look right)
            // Camera move on y (vertical movement) controls the pitch (look up or look down)
            cameraAngle.x += -cameraMove.y * rotateSpeed * Time.deltaTime;
            cameraAngle.y += cameraMove.x * rotateSpeed * Time.deltaTime;

            cameraAngle.x = Mathf.Clamp(cameraAngle.x, minPitch, maxPitch);
            cameraAngle.y %= 360;

            // Rotate player based on mouse input, ensure pitch is bounded to not overshoot
            transform.rotation = Quaternion.Euler(0, cameraAngle.y, 0);
            // Only rotate camera pitch
            cameraTransform.rotation = Quaternion.Euler(cameraAngle.x, cameraAngle.y, 0);

            // Check if the player is falling
            (bool onGround, float groundAngle) = CheckGrounded(out RaycastHit groundHit);
            bool falling = !(onGround && groundAngle <= maxWalkingAngle);

            // If falling, increase falling speed, otherwise stop falling.
            if (falling)
            {
                velocity += gravity * Time.deltaTime;
                elapsedFalling += Time.deltaTime;
            }
            else
            {
                velocity = Vector3.zero;
                elapsedFalling = 0;
                notSlidingSinceJump = true;
            }

            // If the player is attemtping to jump and can jump allow for player jump
            if (jumpInputPressed)
            {
                jumpInputElapsed = 0.0f;
            }
            // Player is attempting to jump if they hit jump this frame or within the last buffer time.
            bool attemptingJump = jumpInputElapsed <= jumpBufferTime;

            // Player can jump if they are (1) on the ground, (2) within the ground jump angle,
            //  (3) has not jumped within the jump cooldown time period, and (4) has only jumped once while sliding
            bool canJump = (onGround || elapsedFalling <= coyoteTime) &&
                groundAngle <= maxJumpAngle &&
                timeSinceLastJump >= jumpCooldown &&
                (!falling || notSlidingSinceJump);

            // Have player jump if they can jump and are attempting to jump
            if (canJump && attemptingJump)
            {
                velocity = Vector3.Lerp(Vector3.up, groundHit.normal, jumpAngleWeightFactor) * jumpVelocity;
                timeSinceLastJump = 0.0f;
                jumpInputElapsed = Mathf.Infinity;

                // Mark if the player is jumping while they are sliding
                notSlidingSinceJump = false;
            }
            else
            {
                timeSinceLastJump += Time.deltaTime;
                jumpInputElapsed += Time.deltaTime;
            }

            // Read player input movement
            var inputVector = new Vector3(playerMove.x, 0, playerMove.y);

            // Rotate movement by current viewing angle
            var viewYaw = Quaternion.Euler(0, cameraAngle.y, 0);
            Vector3 rotatedVector = viewYaw * inputVector;
            Vector3 normalizedInput = rotatedVector.normalized * Mathf.Min(rotatedVector.magnitude, 1.0f);

            // Scale movement by speed and time
            Vector3 movement = normalizedInput * moveSpeed * Time.deltaTime;

            // If the player is standing on the ground, project their movement onto that plane
            // This allows for walking down slopes smoothly.
            if (!falling)
            {
                movement = Vector3.ProjectOnPlane(movement, groundHit.normal);
            }

            // Attempt to move the player based on player movement
            transform.position = MovePlayer(movement);

            // Move player based on falling speed
            transform.position = MovePlayer(velocity * Time.deltaTime);

            // If player was on ground at the start of the ground, snap the player down
            if (onGround && !attemptingJump)
            {
                SnapPlayerDown();
            }
        }

        /// <summary>
        /// Check if the player is standing on the ground.
        /// </summary>
        /// <param name="groundHit">Hit event for standing on the ground.</param>
        /// <returns>A tuple of a (boolean, float), the boolean is whether the player is within groundDist of
        /// the ground, the float is the angle between the surface and the ground.</returns>
        private (bool, float) CheckGrounded(out RaycastHit groundHit)
        {
            bool onGround = CastSelf(transform.position, transform.rotation, Vector3.down, groundDist, out groundHit);
            float angle = Vector3.Angle(groundHit.normal, Vector3.up);
            return (onGround, angle);
        }

        /// <summary>
        /// Snap the player down if they are within a specific distance of the ground.
        /// </summary>
        public void SnapPlayerDown()
        {
            bool closeToGround = CastSelf(
                transform.position,
                transform.rotation,
                Vector3.down,
                verticalSnapDown,
                out RaycastHit groundHit);

            // If within the threshold distance of the ground
            if (closeToGround && groundHit.distance > 0)
            {
                // Snap the player down the distance they are from the ground
                transform.position += Vector3.down * (groundHit.distance - KCCUtils.Epsilon * 2);
            }
        }

        /// <summary>
        /// Move the player with a bounce and slide motion.
        /// </summary>
        /// <param name="movement">Movement of the player.</param>
        /// <returns>Final position of player after moving and bouncing.</returns>
        public Vector3 MovePlayer(Vector3 movement)
        {
            Vector3 position = transform.position;
            Quaternion rotation = transform.rotation;

            Vector3 remaining = movement;

            int bounces = 0;

            while (bounces < maxBounces && remaining.magnitude > KCCUtils.Epsilon)
            {
                // Do a cast of the collider to see if an object is hit during this
                // movement bounce
                float distance = remaining.magnitude;
                if (!CastSelf(position, rotation, remaining.normalized, distance, out RaycastHit hit))
                {
                    // If there is no hit, move to desired position
                    position += remaining;

                    // Exit as we are done bouncing
                    break;
                }

                // If we are overlapping with something, just exit.
                if (hit.distance == 0)
                {
                    break;
                }

                float fraction = hit.distance / distance;
                // Set the fraction of remaining movement (minus some small value)
                position += remaining * (fraction);
                // Push slightly along normal to stop from getting caught in walls
                position += hit.normal * KCCUtils.Epsilon * 2;
                // Decrease remaining movement by fraction of movement remaining
                remaining *= (1 - fraction);

                // Plane to project rest of movement onto
                Vector3 planeNormal = hit.normal;

                // Only apply angular change if hitting something
                // Get angle between surface normal and remaining movement
                float angleBetween = Vector3.Angle(hit.normal, remaining) - 90.0f;

                // Normalize angle between to be between 0 and 1
                // 0 means no angle, 1 means 90 degree angle
                angleBetween = Mathf.Min(KCCUtils.MaxAngleShoveRadians, Mathf.Abs(angleBetween));
                float normalizedAngle = angleBetween / KCCUtils.MaxAngleShoveRadians;

                // Reduce the remaining movement by the remaining movement that ocurred
                remaining *= Mathf.Pow(1 - normalizedAngle, anglePower) * 0.9f + 0.1f;

                // Rotate the remaining movement to be projected along the plane 
                // of the surface hit (emulate pushing against the object)
                Vector3 projected = Vector3.ProjectOnPlane(remaining, planeNormal).normalized * remaining.magnitude;

                // If projected remaining movement is less than original remaining movement (so if the projection broke
                // due to float operations), then change this to just project along the vertical.
                if (projected.magnitude + KCCUtils.Epsilon < remaining.magnitude)
                {
                    remaining = Vector3.ProjectOnPlane(remaining, Vector3.up).normalized * remaining.magnitude;
                }
                else
                {
                    remaining = projected;
                }

                // Track number of times the character has bounced
                bounces++;
            }

            // We're done, player was moved as part of loop
            return position;
        }

        /// <summary>
        /// Cast self in a given direction and get the first object hit.
        /// </summary>
        /// <param name="position">Position of the object when it is being raycast.</param>
        /// <param name="rotation">Rotation of the objecting when it is being raycast.</param>
        /// <param name="direction">Direction of the raycast.</param>
        /// <param name="distance">Maximum distance of raycast.</param>
        /// <param name="hit">First object hit and related information, will have a distance of Mathf.Infinity if none
        /// is found.</param>
        /// <returns>True if an object is hit within distance, false otherwise.</returns>
        public bool CastSelf(Vector3 pos, Quaternion rot, Vector3 dir, float dist, out RaycastHit hit)
        {
            // Get Parameters associated with the KCC
            Vector3 center = rot * capsuleCollider.center + pos;
            float radius = capsuleCollider.radius;
            float height = capsuleCollider.height;

            // Get top and bottom points of collider
            Vector3 bottom = center + rot * Vector3.down * (height / 2 - radius);
            Vector3 top = center + rot * Vector3.up * (height / 2 - radius);

            // Check what objects this collider will hit when cast with this configuration excluding itself
            IEnumerable<RaycastHit> hits = Physics.CapsuleCastAll(
                top, bottom, radius, dir, dist, ~0, QueryTriggerInteraction.Ignore)
                .Where(hit => hit.collider.transform != transform);
            bool didHit = hits.Count() > 0;

            // Find the closest objects hit
            float closestDist = didHit ? Enumerable.Min(hits.Select(hit => hit.distance)) : 0;
            IEnumerable<RaycastHit> closestHit = hits.Where(hit => hit.distance == closestDist);

            // Get the first hit object out of the things the player collides with
            hit = closestHit.FirstOrDefault();

            // Return if any objects were hit
            return didHit;
        }
    }
}
