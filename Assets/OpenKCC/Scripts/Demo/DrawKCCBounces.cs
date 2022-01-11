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
            List<(Vector3, Ray, MovementAction)> actions = new List<(Vector3, Ray, MovementAction)>(
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
