
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
        /// Rotation speed in degrees per second.
        /// </summary>
        public float rotationSpeed = 15.0f;

        public void Update()
        {
            KCCStateMachine sm = GetComponent<KCCStateMachine>();
            ICameraControls cameraControls = GetComponent<ICameraControls>();

            if (sm.InputMovement.magnitude > KCCUtils.Epsilon)
            {
                // Rotate the avatar to follow the direction of the
                // player's movement if it's grater than some dead zone
                // with a given turning rate.
                Quaternion currentHeading = transform.rotation;
                Quaternion desiredHeading = Quaternion.LookRotation(cameraControls.PlayerHeading * sm.InputMovement);

                // Rotate the current heading towards the desired heading.
                Quaternion rotatedHeading = Quaternion.RotateTowards(currentHeading, desiredHeading, rotationSpeed * Time.deltaTime);

                // set the avatar rotation to be the new rotated heading.
                transform.rotation = rotatedHeading;
            }
        }
    }
}