using nickmaltbie.OpenKCC.Character;
using UnityEngine;

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
            // Get the bounces the player's movement would make
            // List<(Vector3, Ray, MovementAction)> actions = new List<Vector3, Ray, MovementAction>()
        }
    }
}
