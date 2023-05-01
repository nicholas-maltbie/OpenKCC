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
        public const string LeftFootIKWeight = "LeftFootIKWeight";
        public const string RightFootIKWeight = "RightFootIKWeight";

        public enum State
        {
            Released,
            Grounded
        }

        public enum Foot
        {
            LeftFoot,
            RightFoot,
        }

        public class FootTarget
        {
            public const float MinFootGroundedWeight = 0.9f;
            public const float MaxFootReleasedWeight = 0.1f;

            private Foot foot;
            private Animator animator;
            private float footIKWeightVelocityLerp;

            private Vector3 fromFootPosition;
            private Quaternion fromFootRotation;
            private float strideStartTime = Mathf.NegativeInfinity;
            private float lastReleaseTime = Mathf.NegativeInfinity;

            public FootTarget(Foot foot, Animator animator)
            {
                this.foot = foot;
                this.animator = animator;
            }

            public float StrideHeight { get; set; }
            public float StrideTime { get; set; }
            public float FootGroundedHeight { get; set; }
            public Vector3 TargetFootPosition { get; private set; }
            public Quaternion TargetFootRotation { get; private set; }
            public float FootIKWeight { get; private set; }
            public State FootState { get; private set; }
            public bool MidStride => RemainingStrideTime > 0;
            protected float RemainingStrideTime => strideStartTime != Mathf.NegativeInfinity ? (strideStartTime + StrideTime) - Time.time : -1;

            public float GetFootAnimationWeight() => foot == Foot.LeftFoot ? animator.GetFloat(LeftFootIKWeight) : animator.GetFloat(RightFootIKWeight);

            public bool OverGroundThreshold() => GetFootAnimationWeight() >= 0.5f;
            public bool UnderReleaseThreshold() => GetFootAnimationWeight() <= 0.5f;

            public Vector3 FootIKTargetPos()
            {
                if (!MidStride)
                {
                    return TargetFootPosition + Vector3.up * FootGroundedHeight;
                }

                float fraction = 1 - Mathf.Clamp(RemainingStrideTime / StrideTime, 0, 1);
                Vector3 lerpPos = Vector3.Lerp(fromFootPosition, TargetFootPosition, fraction);
                Vector3 verticalOffset = Vector3.up * StrideHeight * Mathf.Sin(fraction * Mathf.PI);
                return lerpPos + verticalOffset + Vector3.up * FootGroundedHeight;
            }

            public Quaternion FootIKTargetRot()
            {
                if (!MidStride)
                {
                    return TargetFootRotation;
                }

                float fraction = 1 - Mathf.Clamp(RemainingStrideTime / StrideTime, 0, 1);
                return Quaternion.Lerp(fromFootRotation, TargetFootRotation, fraction);
            }

            public void LerpFootIKWeight()
            {
                float currentWeight = FootIKWeight;
                float targetWeight = GetFootAnimationWeight();
                FootIKWeight = Mathf.SmoothDamp(currentWeight, targetWeight, ref footIKWeightVelocityLerp, StrideTime, maxSpeed: Mathf.Infinity, deltaTime: Time.deltaTime);
            }

            public void ReleaseFoot()
            {
                FootState = State.Released;
                lastReleaseTime = Time.time;

                // Mark as not taking a stride
                strideStartTime = Mathf.NegativeInfinity;
            }

            public void GroundFoot(Vector3 pos, Quaternion rot)
            {
                FootState = State.Grounded;
                TargetFootPosition = pos;
                TargetFootRotation = rot;

                // Mark as not taking a stride
                strideStartTime = Mathf.NegativeInfinity;
            }

            public void StartStride(Vector3 toPos, Quaternion toRot)
            {
                fromFootPosition = TargetFootPosition;
                fromFootRotation = TargetFootRotation;
                TargetFootPosition = toPos;
                TargetFootRotation = toRot;
                strideStartTime = Time.time;
                lastReleaseTime = Time.time;
            }

            public void UpdateStrideTarget(Vector3 toPos, Quaternion toRot)
            {
                TargetFootPosition = toPos;
                TargetFootRotation = toRot;
            }
        }

        public FootTarget leftFootTarget, rightFootTarget;
        public float groundCheckDist = 1.5f;
        public float stepHeight = 0.1f;
        public float strideThresholdDistance = 0.75f;
        public float strideThresholdDegrees = 45;
        public float strideTime = 0.15f;
        public float footGroundedHeight = 0.05f;

        private Animator animator;

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
            leftFootTarget = new FootTarget(Foot.LeftFoot, animator);
            rightFootTarget = new FootTarget(Foot.RightFoot, animator);

            leftFootTarget.StrideHeight = stepHeight;
            leftFootTarget.StrideTime = strideTime;
            leftFootTarget.FootGroundedHeight = footGroundedHeight;
            rightFootTarget.StrideHeight = stepHeight;
            rightFootTarget.StrideTime = strideTime;
            rightFootTarget.FootGroundedHeight = footGroundedHeight;
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

        public void OnAnimatorIK(int layerIndex)
        {
            foreach (Foot foot in Feet)
            {
                FootTarget target = GetFootTarget(foot);
                target.LerpFootIKWeight();
                switch (target.FootState)
                {
                    case State.Grounded:
                        bool shouldRelease = target.UnderReleaseThreshold();
                        if (!target.MidStride && shouldRelease)
                        {
                            target.ReleaseFoot();
                        }
                        else if (GetFootGroundedTransform(foot, out Vector3 groundedPos, out Quaternion groundedRot))
                        {
                            // Check if we have exceeded the target angle or target distance
                            float deltaDist = Vector3.Distance(groundedPos, target.TargetFootPosition);
                            float deltaAngle = Quaternion.Angle(groundedRot, target.TargetFootRotation);
                            bool distThreshold = deltaDist >= strideThresholdDistance;
                            bool turnThreshold = deltaAngle >= strideThresholdDegrees;

                            if (distThreshold || turnThreshold)
                            {
                                if (!target.MidStride)
                                {
                                    target.StartStride(groundedPos, groundedRot);
                                }
                                else
                                {
                                    target.UpdateStrideTarget(groundedPos, groundedRot);
                                }
                            }
                        }

                        break;
                    case State.Released:
                        bool shouldGround = target.OverGroundThreshold();
                        if (shouldGround)
                        {
                            bool onGround = GetFootGroundedTransform(foot, out Vector3 groundedPos, out Quaternion groundedRot);
                            if (onGround)
                            {
                                Transform footTransform = GetFootTransform(foot);
                                target.GroundFoot(groundedPos, groundedRot);
                            }
                        }

                        break;
                }

                AvatarIKGoal goal = foot == Foot.LeftFoot ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
                float weight = target.FootIKWeight;
                animator.SetIKPosition(goal, target.FootIKTargetPos());
                animator.SetIKRotation(goal, target.FootIKTargetRot());
                animator.SetIKPositionWeight(goal, weight);
                animator.SetIKRotationWeight(goal, weight);
            }
        }

        private Transform GetFootTransform(Foot foot)
        {
            HumanBodyBones footBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
            return animator.GetBoneTransform(footBone);
        }

        private bool GetFootGroundedTransform(Foot foot, out Vector3 groundedPos, out Quaternion rotation)
        {
            HumanBodyBones kneeBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg;
            Transform kneeTransform = animator.GetBoneTransform(kneeBone);
            Transform footTransform = GetFootTransform(foot);
            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);

            Vector3 heightOffset = Vector3.Project(kneeTransform.position - footTransform.position, Vector3.up);
            Vector3 source = footTransform.position + heightOffset;

            bool heelGrounded = Physics.Raycast(source, Vector3.down, out RaycastHit kneeHitInfo, groundCheckDist);
            Debug.DrawLine(source, source + Vector3.down * groundCheckDist);

            groundedPos = Vector3.zero;
            rotation = Quaternion.identity;
            if (heelGrounded)
            {
                groundedPos = kneeHitInfo.point;
                Vector3 footForward = Vector3.ProjectOnPlane(hipTransform.rotation * Vector3.forward, kneeHitInfo.normal);
                rotation = Quaternion.LookRotation(footForward, kneeHitInfo.normal);
            }

            return heelGrounded;
        }
    }
}
