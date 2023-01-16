

using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.MoleKCCSample
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(IGetKCCConfig))]
    [RequireComponent(typeof(IGetKCCGrounded))]
    public class MoleMovementEngine : KCCMovementEngine
    {
        public override Vector3 GetMovement(
            Vector3 position,
            Vector3 movement,
            Quaternion rotation)
        {
            Vector3 finalPos = position;
            foreach (KCCBounce bounce in KCCUtils.GetBounces(position, movement, rotation, config))
            {
                if (bounce.action == KCCUtils.MovementAction.Bounce)
                {
                    // If we bounce off a wall perpendicular to the current surface
                    Vector3 normal = bounce.hit.normal;
                    if (normal != Vector3.zero)
                    {
                        // Rotate the remaining movement
                        bounce.remainingMomentum = Quaternion.LookRotation(bounce.hit.normal) * bounce.remainingMomentum;
                        
                        // Adjust up vector for grounded state
                        KCCGroundedState ground = groundedState as KCCGroundedState;
                        if (ground != null)
                        {
                            ground.SurfaceNormal = bounce.hit.normal;
                            ground.OnGround = true;
                            ground.DistanceToGround = KCCUtils.Epsilon;
                        }
                    }
                }
                else if (bounce.action == KCCUtils.MovementAction.Stop)
                {
                    finalPos = bounce.finalPosition;
                }
            }

            return finalPos - position;
        }
    }
}
