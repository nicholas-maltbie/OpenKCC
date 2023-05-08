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

using UnityEngine;

namespace nickmaltbie.OpenKCC.Animation
{
    /// <summary>
    /// Script to attach player feet to ground as the player
    /// moves around naturally.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class HumanoidFootIK : MonoBehaviour
    {
        /// <summary>
        /// Array of each kind of foot associated with a human character.
        /// </summary>
        public static readonly Foot[] Feet = new Foot[]{Foot.LeftFoot, Foot.RightFoot};

        /// <summary>
        /// Targets for the feet positions to manage them separately.
        /// </summary>
        public FootTarget leftFootTarget, rightFootTarget;

        /// <summary>
        /// Distance at which this will check to the ground from the player's
        /// knee height.
        /// </summary>
        [Tooltip("Distance to check for ground from knee height")]
        public float groundCheckDist = 1.5f;

        /// <summary>
        /// How high player will lift feet when stepping in place.
        /// </summary>
        [Tooltip("How high player will lift feet when stepping in place.")]
        public float stepHeight = 0.1f;

        /// <summary>
        /// Distance threshold above which the player will release
        /// their foot from the ground. This is the distance between the current
        /// target/desired position and the current position the foot is on the
        /// ground.
        /// </summary>
        [Tooltip("Distance threshold above which the player will release their grounded feet.")]
        public float strideThresholdDistance = 0.75f;

        /// <summary>
        /// Degrees of rotation above which the player will rotate their foot
        /// when their foot is a given number of degrees away from their
        /// current facing.
        /// </summary>
        [Tooltip("Degree threshold above which the player will rotate and replace their feet.")]
        public float strideThresholdDegrees = 45;

        /// <summary>
        /// Time for the player to complete a grounded stride.
        /// </summary>
        [Tooltip("Time needed to complete a grounded stride.")]
        public float strideTime = 0.25f;

        /// <summary>
        /// Blend time between foot in the air and being placed on the ground
        /// in seconds.
        /// </summary>
        [Tooltip("Blend time (in seconds) when placing foot on the ground.")]
        public float placeBlendTime = 0.05f;

        /// <summary>
        /// How far off the ground are the feet bones when the player's foot
        /// is grounded.
        /// </summary>
        [Tooltip("Height offset for feet when grounded.")]
        public float footGroundedHeight = 0.05f;
        
        /// <summary>
        /// Max distance the player's foot should be from the hips, above
        /// which the player will over their hips down to properly place
        /// feet on the ground.
        /// </summary>
        [Tooltip("Max distance the player's foot should be from the hips.")]
        public float maxHipFootDistance = 0.85f;

        /// <summary>
        /// Time to take to sooth hip offset from the ground.
        /// </summary>
        [Tooltip("Time to take to sooth hip offset from the ground.")]
        public float hipSmoothTime = 0.35f;
        
        /// <summary>
        /// Animator component for managing the player avatar.
        /// </summary>
        private Animator animator;

        /// <summary>
        /// Current speed at which the hips are moving to match the grounded
        /// position.
        /// </summary>
        private float hipOffsetSpeed;

        /// <summary>
        /// The current offset of the hip location from the current
        /// grounded location.
        /// </summary>
        private float hipOffset;

        /// <summary>
        /// Gest the most recent stride time between
        /// the left and right foot targets for this avatar.
        /// </summary>
        public float MostRecentStrideTime => Mathf.Max(leftFootTarget.StrideStartTime, rightFootTarget.StrideStartTime);

        /// <summary>
        /// Gets if the player can take a stride with their feet right now.
        /// This checks that neither foot has taken a stride within the past
        /// stride time threshold.
        /// </summary>
        public bool CanTakeStride => (MostRecentStrideTime + strideTime) <= Time.time;

        /// <summary>
        /// Configure and setup the humanoid foot ik controller.
        /// </summary>
        public void Awake()
        {
            animator = GetComponent<Animator>();
            SetupTargets();
        }

        /// <summary>
        /// On validate function to update values when they are changed
        /// in the editor.
        /// </summary>
        public void OnValidate()
        {
            SetupTargets();
        }

        /// <summary>
        /// Setup the individual foot targets based on the configuration values.
        /// </summary>
        private void SetupTargets()
        {
            leftFootTarget = new FootTarget(Foot.LeftFoot, animator, stepHeight, strideTime, placeBlendTime, footGroundedHeight);
            rightFootTarget = new FootTarget(Foot.RightFoot, animator, stepHeight, strideTime, placeBlendTime, footGroundedHeight);
        }

        /// <summary>
        /// Gets the foot target based on the selection.
        /// </summary>
        /// <param name="foot">Foot to select (right or left).</param>
        /// <returns>FootTarget for managing that foot's grounded state.</returns>
        public FootTarget GetFootTarget(Foot foot)
        {
            if (foot == Foot.LeftFoot)
            {
                return leftFootTarget;
            }
            else if (foot == Foot.RightFoot)
            {
                return rightFootTarget;
            }

            return null;
        }

        /// <summary>
        /// Update called each frame to lerp the foot ik weights.
        /// </summary>
        public void Update()
        {
            foreach (Foot foot in Feet)
            {
                GetFootTarget(foot).LerpFootIKWeight();
            }
        }

        /// <summary>
        /// Update the feet positions based on some current delta
        /// in position for moving ground targets.
        /// </summary>
        /// <param name="deltaPos">Delta position in world space to move the feet.</param>
        public void UpdateFeetPositions(Vector3 deltaPos)
        {
            foreach (Foot foot in Feet)
            {
                var target = GetFootTarget(foot);
                if (target.State != FootState.Grounded)
                {
                    continue;
                }

                Vector3 newPos = target.TargetFootPosition + deltaPos;

                // Recompute angle for the foot as well and check if grounded target has changed.
                HumanBodyBones kneeBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg;
                Transform kneeTransform = animator.GetBoneTransform(kneeBone);
                Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);

                Vector3 heightOffset = Vector3.Project(kneeTransform.position - newPos, Vector3.up);
                Vector3 source = newPos + heightOffset;

                bool grounded = Physics.Raycast(source, Vector3.down, out RaycastHit hitInfo, groundCheckDist);
                if (grounded)
                {
                    if (hitInfo.collider.gameObject == target.Floor)
                    {
                        Vector3 footForward = Vector3.ProjectOnPlane(target.FootForward, hitInfo.normal).normalized;
                        target.UpdateStrideTarget(hitInfo.point, Quaternion.LookRotation(footForward, hitInfo.normal), true);
                    }
                    else
                    {
                        Vector3 footForward = Vector3.ProjectOnPlane(hipTransform.forward, hitInfo.normal);
                        Quaternion rotation = Quaternion.LookRotation(footForward, hitInfo.normal);
                        target.StartStride(hitInfo.point, rotation, hitInfo.collider.gameObject, footForward, true);
                    }
                }
                else
                {
                    target.ReleaseFoot();
                }
            }
        }

        /// <summary>
        /// Called each time the animator is updated. This is used to get the
        /// currently desired position of the feet from the animator's current
        /// animation then update the foot based on its current state.
        /// </summary>
        /// <param name="layerIndex">Layer index for the animator.</param>
        public void OnAnimatorIK(int layerIndex)
        {
            foreach (Foot foot in Feet)
            {
                FootTarget target = GetFootTarget(foot);

                switch (target.State)
                {
                    case FootState.Grounded:
                        bool shouldRelease = target.UnderReleaseThreshold();
                        if (shouldRelease)
                        {
                            target.ReleaseFoot();
                        }
                        else if (GetFootTargetPosViaHips(foot, out Vector3 hipGroundPos, out Quaternion hipGroundRot, out Vector3 _, out GameObject hipFloor, out Vector3 hipForward))
                        {
                            // Check if we have exceeded the target angle or target distance
                            float deltaDist = Vector3.ProjectOnPlane(hipGroundPos - target.TargetFootPosition, Vector3.up).magnitude;
                            float deltaAngle = Quaternion.Angle(hipGroundRot, target.TargetFootRotation);
                            bool distThreshold = deltaDist >= strideThresholdDistance;
                            bool turnThreshold = deltaAngle >= strideThresholdDegrees;

                            // When replacing the foot, try to move the foot up towards the hip if the
                            // step is behind the hips.
                            if (CanTakeStride && !target.MidStride)
                            {
                                if (distThreshold)
                                {
                                    target.ReleaseFoot();
                                }
                                else if (turnThreshold)
                                {
                                    target.StartStride(hipGroundPos, hipGroundRot, hipFloor, hipForward, true);
                                }
                            }
                            else if (target.MidStride && target.CanUpdateStrideTarget())
                            {
                                target.UpdateStrideTarget(hipGroundPos, hipGroundRot);
                            }
                        }
                        break;
                    case FootState.Released:
                        if (target.OverGroundThreshold() &&
                            GetFootGroundedTransform(foot, out Vector3 groundedPos, out Quaternion groundedRot, out Vector3 _, out GameObject groundedFloor, out Vector3 groundedForward))
                        {
                            target.StartStride(groundedPos, groundedRot, groundedFloor, groundedForward, false);
                        }
                        else
                        {
                            Transform footTransform = GetFootTransform(foot);
                            target.UpdateStrideTarget(footTransform.position, target.TargetFootRotation, target.CanUpdateStrideTarget());
                        }
                        break;
                }

                AvatarIKGoal goal = foot == Foot.LeftFoot ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
                animator.SetIKPosition(goal, target.FootIKTargetPos());
                animator.SetIKRotation(goal, target.FootIKTargetRot());
                animator.SetIKPositionWeight(goal, target.FootIKWeight);
                animator.SetIKRotationWeight(goal, target.FootIKWeight);
            }

            // Move hips according to target positions
            float targetHipOffset = GetTargetHipOffset();
            hipOffset = Mathf.SmoothDamp(hipOffset, targetHipOffset, ref hipOffsetSpeed, hipSmoothTime, Mathf.Infinity, Time.deltaTime);
            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
            transform.localPosition = Vector3.up * hipOffset;
        }

        /// <summary>
        /// Get the foot transform from the animator avatar.
        /// </summary>
        /// <param name="foot">Foot to check.</param>
        /// <returns>Bone transform for that avatar's selected foot.</returns>
        private Transform GetFootTransform(Foot foot)
        {
            HumanBodyBones footBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
            return animator.GetBoneTransform(footBone);
        }

        /// <summary>
        /// Gets the target hip offset based on the current vertical offset
        /// of each foot.
        /// </summary>
        /// <returns>The new target hip offset based on the current distance from
        /// the hips.</returns>
        private float GetTargetHipOffset()
        {
            // Average the hip offset required of each foot
            float leftOffset = GetVerticalOffsetByFoot(Foot.LeftFoot);
            float rightOffset = GetVerticalOffsetByFoot(Foot.RightFoot);

            // Only include grounded feet
            FootTarget leftTarget = GetFootTarget(Foot.LeftFoot);
            FootTarget rightTarget = GetFootTarget(Foot.RightFoot);

            if (leftTarget.State == FootState.Grounded && rightTarget.State == FootState.Grounded)
            {
                return Mathf.Min(leftOffset, rightOffset);
            }
            else if (leftTarget.State == FootState.Grounded)
            {
                return leftOffset;
            }
            else if (rightTarget.State == FootState.Grounded)
            {
                return rightOffset;
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Gets the foot's desired hip offset based on the current grounded
        /// state. Basically, if a foot is grounded on floor too far away from the
        /// avatar, the avatar should move down to account for this height
        /// difference.
        /// </summary>
        /// <param name="foot">Foot target for the avatar.</param>
        /// <returns>Desired hip offset for the selected foot.</returns>
        private float GetVerticalOffsetByFoot(Foot foot)
        {
            FootTarget target = GetFootTarget(foot);
            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
            float dist = Vector3.Distance(hipTransform.position, target.FootIKTargetPos());
            return Mathf.Clamp(maxHipFootDistance - dist, -maxHipFootDistance, 0);
        }

        /// <summary>
        /// Get where a foot's current target ground position
        /// is in the vertical plane defined by the player's hips.
        /// This will take wherever the foot is in world space and project
        /// it onto the plane that is normal to the hip's forward vector.
        /// This gets where the foot would be if the player were to lift up
        /// their foot and bring it in line with their hips from the foot's
        /// current location.
        /// </summary>
        /// <param name="foot">Foot target to check position of.</param>
        /// <param name="hipGroundedPos">Desired foot position in world space.</param>
        /// <param name="hipGroundedRotation">Desired foot rotation in world space.</param>
        /// <param name="hitNormal">Ground normal for the surface the foot is standing on.</param>
        /// <param name="floor">GameObject/collider player is currently standing on.</param>
        /// <param name="footForward">Forward vector for the foot's desired rotation.</param>
        /// <returns>True if the foot has a surface to ground on, false otherwise.</returns>
        private bool GetFootTargetPosViaHips(Foot foot, out Vector3 hipGroundedPos, out Quaternion hipGroundedRotation, out Vector3 hitNormal, out GameObject floor, out Vector3 footForward)
        {
            HumanBodyBones kneeBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg;
            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
            Transform kneeTransform = animator.GetBoneTransform(kneeBone);
            Transform footTransform = GetFootTransform(foot);

            Vector3 hipForward = hipTransform.forward;
            Vector3 hipRight = hipTransform.right;

            Vector3 footPos = footTransform.position;
            Vector3 source = Vector3.ProjectOnPlane(footPos - hipTransform.position, hipForward) + hipTransform.position;
            source.y = kneeTransform.position.y;
            bool hipGrounded = Physics.Raycast(source, Vector3.down, out RaycastHit hipHitInfo, groundCheckDist);
            if (hipGrounded)
            {
                hipGroundedPos = hipHitInfo.point;
                hitNormal = hipHitInfo.normal;
                hipGroundedRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(hipTransform.forward, hipHitInfo.normal), Vector3.up);
                floor = hipHitInfo.collider?.gameObject;
                footForward = hipTransform.forward;
                return true;
            }
            else
            {
                hipGroundedPos = footPos;
                hitNormal = Vector3.up;
                floor = null;
                footForward = footTransform.forward;
                hipGroundedRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(footTransform.forward, hipHitInfo.normal), Vector3.up);
                return false;
            }
        }

        /// <summary>
        /// Gets the foot's current grounded location in world space given
        /// the current location of the foot from the animator.
        /// Will draw a line from the knee height of the player down and check if
        /// the foot can stand on some surface under the player.
        /// </summary>
        /// <param name="foot">Foot target to check position of.</param>
        /// <param name="groundedPos">Desired foot position in world space.</param>
        /// <param name="rotation">Desired foot rotation in world space.</param>
        /// <param name="groundNormal">Ground normal for the surface the foot is standing on.</param>
        /// <param name="floor">GameObject/collider player is currently standing on.</param>
        /// <param name="footForward">Forward vector for the foot's desired rotation.</param>
        /// <returns>True if the foot has a surface to ground on, false otherwise.</returns>
        private bool GetFootGroundedTransform(Foot foot, out Vector3 groundedPos, out Quaternion rotation, out Vector3 groundNormal, out GameObject floor, out Vector3 footForward)
        {
            HumanBodyBones kneeBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg;
            Transform kneeTransform = animator.GetBoneTransform(kneeBone);
            Transform footTransform = GetFootTransform(foot);
            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);

            Vector3 heightOffset = Vector3.Project(kneeTransform.position - footTransform.position, Vector3.up);
            Vector3 source = footTransform.position + heightOffset;

            bool heelGrounded = Physics.Raycast(source, Vector3.down, out RaycastHit kneeHitInfo, groundCheckDist);
            groundedPos = footTransform.position;
            rotation = footTransform.rotation;
            groundNormal = Vector3.up;
            floor = null;
            footForward = Vector3.forward;
            if (heelGrounded)
            {
                groundedPos = kneeHitInfo.point;
                footForward = Vector3.ProjectOnPlane(hipTransform.forward, kneeHitInfo.normal);
                rotation = Quaternion.LookRotation(footForward, kneeHitInfo.normal);
                groundNormal = kneeHitInfo.normal;
                floor = kneeHitInfo.collider?.gameObject;
            }

            return heelGrounded;
        }
    }
}
