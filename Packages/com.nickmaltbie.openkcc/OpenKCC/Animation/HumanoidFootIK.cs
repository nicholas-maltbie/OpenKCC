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

using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity;
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
        public static readonly Foot[] Feet = new Foot[] { Foot.LeftFoot, Foot.RightFoot };

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
        /// Unity service for managing time delta.
        /// </summary>
        internal IUnityService unityService = UnityService.Instance;

        /// <summary>
        /// Raycast helper for mocking raycast behavior.
        /// </summary>
        internal IRaycastHelper raycastHelper = RaycastHelper.Instance;

        /// <summary>
        /// Layer for calculating collisions with ground.
        /// </summary>
        public LayerMask feetCollisionDetection = -1;

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
        /// Test debug value to disable correcting for overlap.
        /// </summary>
        internal bool correctForOverlap = true;

        /// <summary>
        /// Left foot target for player.
        /// </summary>
        public FootTarget LeftFootTarget { get; private set; }

        /// <summary>
        /// Right foot target for player.
        /// </summary>
        public FootTarget RightFootTarget { get; private set; }

        /// <summary>
        /// Gest the most recent stride time between
        /// the left and right foot targets for this avatar.
        /// </summary>
        public float MostRecentStrideTime => Mathf.Max(LeftFootTarget.StrideStartTime, RightFootTarget.StrideStartTime);

        /// <summary>
        /// Gets if the player can take a stride with their feet right now.
        /// This checks that neither foot has taken a stride within the past
        /// stride time threshold.
        /// </summary>
        public bool CanTakeStride => (MostRecentStrideTime + strideTime) <= unityService.time;

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
            LeftFootTarget = new FootTarget(Foot.LeftFoot, animator, stepHeight, strideTime, footGroundedHeight);
            RightFootTarget = new FootTarget(Foot.RightFoot, animator, stepHeight, strideTime, footGroundedHeight);
            LeftFootTarget.unityService = unityService;
            RightFootTarget.unityService = unityService;
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
                return LeftFootTarget;
            }
            else if (foot == Foot.RightFoot)
            {
                return RightFootTarget;
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
            if (deltaPos.magnitude <= KCCUtils.Epsilon)
            {
                return;
            }

            foreach (Foot foot in Feet)
            {
                UpdateFootPosition(foot, deltaPos);
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
            UpdateFootTargets();

            // Move hips according to target positions
            float targetHipOffset = GetTargetHipOffset();
            hipOffset = Mathf.SmoothDamp(hipOffset, targetHipOffset, ref hipOffsetSpeed, hipSmoothTime, Mathf.Infinity, unityService.deltaTime);
            transform.localPosition = Vector3.up * hipOffset;
        }

        /// <summary>
        /// Update the foot target position for a specific foot.
        /// </summary>
        /// <param name="foot">foot to update.</param>
        /// <param name="deltaPos">Delta position in world space to move the feet.</param>
        public void UpdateFootPosition(Foot foot, Vector3 deltaPos)
        {
            FootTarget target = GetFootTarget(foot);
            if (target.State != FootState.Grounded)
            {
                return;
            }

            Vector3 newPos = target.TargetFootPosition + deltaPos;

            // If we're overlapping with something, move the foot back
            if (VerifySpotOverlap(newPos, out IRaycastHit hit))
            {
                newPos = hit.point;
            }

            // Recompute angle for the foot as well and check if grounded target has changed.
            HumanBodyBones kneeBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg;
            Transform kneeTransform = animator.GetBoneTransform(kneeBone);
            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);

            var heightOffset = Vector3.Project(kneeTransform.position - newPos, Vector3.up);
            Vector3 source = newPos + heightOffset;

            bool grounded = GetFootGroundedInfo(source, out Vector3 groundedPos, out Vector3 groundNormal, out GameObject floor);
            if (grounded && floor == target.Floor)
            {
                Vector3 footForward = Vector3.ProjectOnPlane(target.FootForward, groundNormal).normalized;
                target.UpdateStrideTarget(groundedPos, Quaternion.LookRotation(footForward, groundNormal), groundNormal, true);
            }
            else if (grounded && floor != target.Floor)
            {
                var footForward = Vector3.ProjectOnPlane(hipTransform.forward, groundNormal);
                var rotation = Quaternion.LookRotation(footForward, groundNormal);
                target.StartStride(groundedPos, rotation, floor, footForward, groundNormal, true);
            }
            else
            {
                target.ReleaseFoot();
            }
        }

        /// <summary>
        /// Gets the target hip offset based on the current vertical offset
        /// of each foot.
        /// </summary>
        /// <returns>The new target hip offset based on the current distance from
        /// the hips.</returns>
        public float GetTargetHipOffset()
        {
            // Average the hip offset required of each foot
            float leftOffset = GetVerticalOffsetByFoot(Foot.LeftFoot);
            float rightOffset = GetVerticalOffsetByFoot(Foot.RightFoot);
            return Mathf.Min(leftOffset, rightOffset);
        }

        /// <summary>
        /// Update the left and right foot targets based on their current state.
        /// </summary>
        private void UpdateFootTargets()
        {
            foreach (Foot foot in Feet)
            {
                FootTarget target = GetFootTarget(foot);
                switch (target.State)
                {
                    case FootState.Grounded:
                        UpdateFootWhenGrounded(target);
                        break;
                    case FootState.Released:
                        UpdateFootWhenReleased(target);
                        break;
                }

                UpdateFootIKState(target);
            }
        }

        /// <summary>
        /// Update the foot IK state based on the current target configuration.
        /// </summary>
        /// <param name="target">Foot target to update.</param>
        private void UpdateFootIKState(FootTarget target)
        {
            AvatarIKGoal goal = target.Foot == Foot.LeftFoot ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
            Vector3 targetPosition = target.TargetFootPosition;
            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);

            // If we are overlapping with something, snap it back
            Transform footTransform = GetFootTransform(target.Foot);
            bool overlapping = VerifySpotOverlap(footTransform.position, out IRaycastHit footHit);
            Vector3 worldPos = footTransform.position;
            if (overlapping)
            {
                worldPos = Vector3.Lerp(footTransform.position, footHit.point + footGroundedHeight * footHit.normal, 0.5f);
            }

            animator.SetIKPosition(goal, Vector3.Lerp(worldPos, target.FootIKTargetPos(), target.FootIKWeight));
            animator.SetIKRotation(goal, target.FootIKTargetRot());
            animator.SetIKPositionWeight(goal, 1);
            animator.SetIKRotationWeight(goal, target.FootIKWeight);

            if (target.State == FootState.Released && overlapping)
            {
                var footForward = Vector3.ProjectOnPlane(hips.forward, footHit.normal);
                animator.SetIKRotationWeight(goal, 1);
                animator.SetIKRotation(goal, Quaternion.LookRotation(footForward, footHit.normal));
            }
        }

        /// <summary>
        /// Update the foot when in the released state.
        /// </summary>
        /// <param name="target">Foot to update</param>
        private void UpdateFootWhenReleased(FootTarget target)
        {
            if (target.OverGroundThreshold() &&
                GetFootGroundedTransform(target.Foot, out Vector3 groundedPos, out Quaternion groundedRot, out Vector3 groundNormal, out GameObject groundedFloor, out Vector3 groundedForward))
            {
                target.StartStride(groundedPos, groundedRot, groundedFloor, groundedForward, groundNormal, false);
            }
        }

        /// <summary>
        /// Update foot targets when the foot is grounded.
        /// </summary>
        /// <param name="target">Foot target to update.</param>
        private void UpdateFootWhenGrounded(FootTarget target)
        {
            bool shouldRelease = target.UnderReleaseThreshold();
            if (shouldRelease)
            {
                target.ReleaseFoot();
            }
            else if (GetFootTargetPosViaHips(target.Foot, out Vector3 hipGroundPos, out Quaternion hipGroundRot, out Vector3 hipNormal, out GameObject hipFloor, out Vector3 hipForward))
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
                        target.StartStride(hipGroundPos, hipGroundRot, hipFloor, hipForward, hipNormal, true);
                    }
                }
                else if (target.MidStride && target.CanUpdateStrideTarget())
                {
                    target.UpdateStrideTarget(hipGroundPos, hipGroundRot, Vector3.up);
                }
            }
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
            if (target.State != FootState.Grounded)
            {
                return 0;
            }

            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
            float dist = Vector3.Project(hipTransform.position - Vector3.up * hipOffset - target.FootIKTargetPos(), Vector3.up).magnitude;
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
        /// <param name="groundPos">Desired foot position in world space.</param>
        /// <param name="rotation">Desired foot rotation in world space.</param>
        /// <param name="groundNormal">Ground normal for the surface the foot is standing on.</param>
        /// <param name="floor">GameObject/collider player is currently standing on.</param>
        /// <param name="footForward">Forward vector for the foot's desired rotation.</param>
        /// <returns>True if the foot has a surface to ground on, false otherwise.</returns>
        private bool GetFootTargetPosViaHips(Foot foot, out Vector3 groundPos, out Quaternion rotation, out Vector3 groundNormal, out GameObject floor, out Vector3 footForward)
        {
            HumanBodyBones kneeBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg;
            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
            Transform kneeTransform = animator.GetBoneTransform(kneeBone);
            Transform footTransform = GetFootTransform(foot);

            Vector3 footPos = footTransform.position;
            Vector3 source = Vector3.ProjectOnPlane(footPos - hipTransform.position, hipTransform.forward) + hipTransform.position;
            source.y = kneeTransform.position.y;

            bool grounded = GetFootGroundedInfo(source, out groundPos, out groundNormal, out floor);
            footForward = Vector3.ProjectOnPlane(hipTransform.forward, groundNormal);
            rotation = Quaternion.LookRotation(footForward, groundNormal);
            return grounded;
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

            var heightOffset = Vector3.Project(kneeTransform.position - footTransform.position, Vector3.up);
            bool grounded = GetFootGroundedInfo(footTransform.position + heightOffset, out groundedPos, out groundNormal, out floor);
            footForward = Vector3.ProjectOnPlane(hipTransform.forward, groundNormal);
            rotation = Quaternion.LookRotation(footForward, groundNormal);
            return grounded;
        }

        /// <summary>
        /// Gets the foot's current grounded location in world space given
        /// the current configuration.
        /// </summary>
        /// <param name="sourcePos">Source position to draw ray from.</param>
        /// <param name="groundedPos">Desired foot position in world space.</param>
        /// <param name="rotation">Desired foot rotation in world space.</param>
        /// <param name="groundNormal">Ground normal for the surface the foot is standing on.</param>
        /// <param name="floor">GameObject/collider player is currently standing on.</param>
        /// <returns>True if the foot has a surface to ground on, false otherwise.</returns>
        private bool GetFootGroundedInfo(Vector3 sourcePos, out Vector3 groundedPos, out Vector3 groundNormal, out GameObject floor)
        {
            bool grounded = raycastHelper.DoRaycastInDirection(sourcePos, Vector3.down, groundCheckDist, out IRaycastHit hitInfo, layerMask: feetCollisionDetection);
            groundedPos = grounded ? hitInfo.point : Vector3.zero;
            groundNormal = grounded ? hitInfo.normal : Vector3.up;
            floor = grounded ? hitInfo.collider?.gameObject : null;
            return grounded;
        }

        /// <summary>
        /// Verifies if a target position for the foot is overlapping with anything.
        /// </summary>
        /// <param name="newTarget">New target position for the foot.</param>
        /// <param name="hit">Hit associated with the reachability check.</param>
        /// <returns>True if reachable, false otherwise.</returns>
        private bool VerifySpotOverlap(Vector3 newTarget, out IRaycastHit hit)
        {
            if (!correctForOverlap)
            {
                hit = new RaycastHitWrapper(new RaycastHit());
                return false;
            }

            // Check if the foot is currently in an overlapping position
            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            Vector3 dir = (newTarget - hips.position).normalized;
            float dist = Vector3.Distance(newTarget, hips.position);

            return raycastHelper.DoRaycastInDirection(
                hips.position,
                dir,
                dist,
                out hit,
                layerMask: feetCollisionDetection);
        }
    }
}
