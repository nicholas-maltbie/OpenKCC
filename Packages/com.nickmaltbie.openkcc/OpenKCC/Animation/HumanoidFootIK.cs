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

using System;
using System.Collections.Generic;
using nickmaltbie.OpenKCC.Utils;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Animation
{
    /// <summary>
    /// Script to attach player feet to ground.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class HumanoidFootIK : MonoBehaviour
    {
        public static readonly Foot[] Feet = new Foot[]{Foot.LeftFoot, Foot.RightFoot};

        public FootTarget leftFootTarget, rightFootTarget;
        public float groundCheckDist = 1.5f;
        public float stepHeight = 0.1f;
        public float strideThresholdDistance = 0.75f;
        public float strideThresholdDegrees = 45;
        public float strideTime = 0.25f;
        public float placeBlendTime = 0.05f;
        public float footGroundedHeight = 0.05f;
        public float maxVerticalDistanceToFoot = 0.75f;
        public float maxHipVerticalOffset = 0.5f;
        public float hipSmoothTime = 0.1f;
        

        private Animator animator;
        private float hipOffsetSpeed;
        private float hipOffset;

        public float MostRecentStrideTime => Mathf.Max(leftFootTarget.StrideStartTime, rightFootTarget.StrideStartTime);
        public bool CanTakeStride => (MostRecentStrideTime + strideTime) <= Time.time;

        public void Awake()
        {
            animator = GetComponent<Animator>();
            SetupTargets();
        }

        public void OnValidate()
        {
            SetupTargets();
        }

        private void SetupTargets()
        {
            leftFootTarget = new FootTarget(Foot.LeftFoot, animator, stepHeight, strideTime, placeBlendTime, footGroundedHeight);
            rightFootTarget = new FootTarget(Foot.RightFoot, animator, stepHeight, strideTime, placeBlendTime, footGroundedHeight);
        }

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

        public void Update()
        {
            foreach (Foot foot in Feet)
            {
                GetFootTarget(foot).LerpFootIKWeight();
            }
        }

        public void OnAnimatorIK(int layerIndex)
        {
            foreach (Foot foot in Feet)
            {
                FootTarget target = GetFootTarget(foot);
                // UnityEngine.Debug.DrawLine(target.TargetFootPosition + Vector3.up, target.TargetFootPosition, Color.red);
                // UnityEngine.Debug.DrawLine(target.fromFootPosition + Vector3.up, target.fromFootPosition, Color.blue);
                // UnityEngine.Debug.DrawLine(target.FootIKTargetPos() + Vector3.up * 0.5f, target.FootIKTargetPos(), Color.green);

                switch (target.State)
                {
                    case FootState.Grounded:
                        bool shouldRelease = target.UnderReleaseThreshold();
                        if (shouldRelease)
                        {
                            target.ReleaseFoot();
                        }
                        else if (GetFootTargetPosViaHips(foot, out Vector3 hipGroundPos, out Quaternion hipGroundRot, out Vector3 normal))
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
                                    target.StartStride(hipGroundPos, hipGroundRot, normal, true);
                                }
                            }
                            else if (target.MidStride && target.CanUpdateStrideTarget())
                            {
                                target.UpdateStrideTarget(hipGroundPos, hipGroundRot, normal);
                            }
                        }
                        break;
                    case FootState.Released:
                        if (target.OverGroundThreshold() &&
                            GetFootGroundedTransform(foot, out Vector3 groundedPos, out Quaternion groundedRot, out Vector3 groundNormal))
                        {
                            target.StartStride(groundedPos, groundedRot, groundNormal, false);
                        }
                        else
                        {
                            Transform footTransform = GetFootTransform(foot);
                            target.UpdateStrideTarget(footTransform.position, target.TargetFootRotation, Vector3.up, target.CanUpdateStrideTarget());
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

        private Transform GetFootTransform(Foot foot)
        {
            HumanBodyBones footBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
            return animator.GetBoneTransform(footBone);
        }

        private Transform GetToeTransform(Foot foot)
        {
            HumanBodyBones toesBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftToes : HumanBodyBones.RightToes;
            return animator.GetBoneTransform(toesBone);
        }

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

        private float GetVerticalOffsetByFoot(Foot foot)
        {
            FootTarget target = GetFootTarget(foot);
            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
            float verticalDistance = Vector3.Project(hipTransform.position - target.FootIKTargetPos(), Vector3.up).magnitude;
            return Mathf.Clamp(maxVerticalDistanceToFoot - verticalDistance, -maxHipVerticalOffset, 0);
        }

        private bool GetFootTargetPosViaHips(Foot foot, out Vector3 hipGroundedPos, out Quaternion hipGroundedRotation, out Vector3 hitNormal)
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
                return true;
            }
            else
            {
                hipGroundedPos = footPos;
                hitNormal = Vector3.up;
                hipGroundedRotation = Quaternion.LookRotation(Vector3.ProjectOnPlane(footTransform.forward, hipHitInfo.normal), Vector3.up);
                return false;
            }
        }

        private bool GetFootGroundedTransform(Foot foot, out Vector3 groundedPos, out Quaternion rotation, out Vector3 groundNormal)
        {
            HumanBodyBones kneeBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg;
            Transform kneeTransform = animator.GetBoneTransform(kneeBone);
            Transform toesTransform = GetToeTransform(foot);
            Transform footTransform = GetFootTransform(foot);
            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);

            Vector3 heightOffset = Vector3.Project(kneeTransform.position - footTransform.position, Vector3.up);
            Vector3 source = footTransform.position + heightOffset;

            bool heelGrounded = Physics.Raycast(source, Vector3.down, out RaycastHit kneeHitInfo, groundCheckDist);
            UnityEngine.Debug.DrawLine(source, source + Vector3.down * groundCheckDist, Color.magenta);
            groundedPos = footTransform.position;
            rotation = footTransform.rotation;
            groundNormal = Vector3.up;
            if (heelGrounded)
            {
                groundedPos = kneeHitInfo.point;
                Vector3 footForward = Vector3.ProjectOnPlane(hipTransform.forward, kneeHitInfo.normal);
                rotation = Quaternion.LookRotation(footForward, kneeHitInfo.normal);
                groundNormal = kneeHitInfo.normal;
            }

            return heelGrounded;
        }
    }
}
