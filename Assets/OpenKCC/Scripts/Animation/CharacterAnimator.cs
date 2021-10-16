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
        public float turningDeadZone = 0.1f;

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
        public KinematicCharacterController kcc;

        /// <summary>
        /// Camera controller for getting player rotation information
        /// </summary>
        public CameraController cameraController;

        /// <summary>
        /// Character controller to move character
        /// </summary>
        private CharacterController characterController;

        /// <summary>
        /// Animator for controlling character
        /// </summary>
        public Animator animator;

        private CharacterAnimatorState animState = new CharacterAnimatorState();

        public void Start()
        {
            this.characterController = this.GetComponent<CharacterController>();
        }

        public void Update()
        {
            if (animator == null)
            {
                return;
            }

            bool jumping = kcc.CanJump && kcc.AttemptingJump;
            bool falling = kcc.IsProne || kcc.FallingTime >= fallingThreshold;
            bool jumpingOrFalling = falling || jumping;
            bool moving = !kcc.IsProne && !jumpingOrFalling && kcc.InputMovement.magnitude > this.movementDeadZone;

            animState.move = new Vector2(kcc.InputMovement.x, kcc.InputMovement.z);
            animState.moving = moving;
            animState.rotation = cameraController.frameRotation > 0 ? 1 : -1;
            animState.turning = !moving &&
                !jumpingOrFalling &&
                Mathf.Abs(cameraController.frameRotation) > this.turningDeadZone;
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
