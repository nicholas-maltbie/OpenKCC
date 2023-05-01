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
            private float strideHeight;
            private float strideTime;
            private float footGroundedHeight;
            private float strideStartTime = Mathf.NegativeInfinity;

            public FootTarget(Foot foot, Animator animator, float strideHeight, float strideTime, float footGroundedHeight)
            {
                this.foot = foot;
                this.animator = animator;

                this.strideHeight = strideHeight;
                this.strideTime = strideTime;
                this.footGroundedHeight = footGroundedHeight;
            }
            public Vector3 GroundNormal { get; private set; } = Vector3.up;
            public Vector3 TargetFootPosition { get; private set; }
            public Quaternion TargetFootRotation { get; private set; }
            public float FootIKWeight { get; private set; }
            public State FootState { get; private set; }
            public bool MidStride => RemainingStrideTime > 0;
            protected float RemainingStrideTime => strideStartTime != Mathf.NegativeInfinity ? (strideStartTime + strideTime) - Time.time : -1;

            public float GetFootAnimationWeight() => foot == Foot.LeftFoot ? animator.GetFloat(LeftFootIKWeight) : animator.GetFloat(RightFootIKWeight);

            public bool OverGroundThreshold() => GetFootAnimationWeight() >= 0.5f;
            public bool UnderReleaseThreshold() => GetFootAnimationWeight() <= 0.5f;

            public Vector3 FootIKTargetPos()
            {
                if (!MidStride)
                {
                    return TargetFootPosition + GroundNormal * footGroundedHeight;
                }

                float fraction = 1 - Mathf.Clamp(RemainingStrideTime / strideTime, 0, 1);
                Vector3 lerpPos = Vector3.Lerp(fromFootPosition, TargetFootPosition, fraction);
                Vector3 verticalOffset = GroundNormal * strideHeight * Mathf.Sin(fraction * Mathf.PI);
                return lerpPos + verticalOffset + GroundNormal * footGroundedHeight;
            }

            public Quaternion FootIKTargetRot()
            {
                if (!MidStride)
                {
                    return TargetFootRotation;
                }

                float fraction = 1 - Mathf.Clamp(RemainingStrideTime / strideTime, 0, 1);
                return Quaternion.Lerp(fromFootRotation, TargetFootRotation, fraction);
            }

            public void LerpFootIKWeight()
            {
                float currentWeight = FootIKWeight;
                float targetWeight = FootState == State.Grounded ? 1 : 0;
                FootIKWeight = Mathf.SmoothDamp(currentWeight, targetWeight, ref footIKWeightVelocityLerp, strideTime, maxSpeed: Mathf.Infinity, deltaTime: Time.deltaTime);
                FootIKWeight = Mathf.Clamp(FootIKWeight, 0, 1);
            }

            public void ReleaseFoot()
            {
                FootState = State.Released;

                // Mark as not taking a stride
                strideStartTime = Mathf.NegativeInfinity;
            }

            public void StartStride(Vector3 toPos, Quaternion toRot, Vector3 groundNormal)
            {
                GroundNormal = groundNormal;
                StartStride(toPos, toRot, TargetFootPosition, TargetFootRotation, groundNormal);
            }

            public void StartStride(Vector3 toPos, Quaternion toRot, Vector3 fromPos, Quaternion fromRot, Vector3 groundNormal)
            {
                GroundNormal = groundNormal;
                FootState = State.Grounded;
                fromFootPosition = fromPos;
                fromFootRotation = fromRot;
                TargetFootPosition = toPos;
                TargetFootRotation = toRot;
                strideStartTime = Time.time;
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
            leftFootTarget = new FootTarget(Foot.LeftFoot, animator, stepHeight, strideTime, footGroundedHeight);
            rightFootTarget = new FootTarget(Foot.RightFoot, animator, stepHeight, strideTime, footGroundedHeight);
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
                        else if (GetFootGroundedTransform(foot, out Vector3 groundedPos, out Quaternion groundedRot, out Vector3 groundNormal))
                        {
                            // Check if we have exceeded the target angle or target distance
                            float deltaDist = Vector3.Distance(groundedPos, target.TargetFootPosition);
                            float deltaAngle = Quaternion.Angle(groundedRot, target.TargetFootRotation);
                            bool distThreshold = deltaDist >= strideThresholdDistance;
                            bool turnThreshold = deltaAngle >= strideThresholdDegrees;

                            if (!target.MidStride && (distThreshold || turnThreshold))
                            {
                                target.StartStride(groundedPos, groundedRot, groundNormal);
                            }
                            else if (target.MidStride)
                            {
                                target.UpdateStrideTarget(groundedPos, groundedRot);
                            }
                        }

                        break;
                    case State.Released:
                        bool shouldGround = target.OverGroundThreshold();
                        if (shouldGround)
                        {
                            bool onGround = GetFootGroundedTransform(foot, out Vector3 groundedPos, out Quaternion groundedRot, out Vector3 groundNormal);
                            if (onGround)
                            {
                                Transform footTransform = GetFootTransform(foot);
                                target.StartStride(groundedPos, groundedRot, footTransform.position, footTransform.rotation, groundNormal);
                            }
                        }

                        break;
                }

                AvatarIKGoal goal = foot == Foot.LeftFoot ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
                UnityEngine.Debug.Log($"foot:{foot} target.FootIKWeight:{target.FootIKWeight}");
                animator.SetIKPosition(goal, target.FootIKTargetPos());
                animator.SetIKRotation(goal, target.FootIKTargetRot());
                animator.SetIKPositionWeight(goal, target.FootIKWeight);
                animator.SetIKRotationWeight(goal, target.FootIKWeight);
            }
        }

        private Transform GetFootTransform(Foot foot)
        {
            HumanBodyBones footBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
            return animator.GetBoneTransform(footBone);
        }

        private bool GetFootGroundedTransform(Foot foot, out Vector3 groundedPos, out Quaternion rotation, out Vector3 groundNormal)
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
            groundNormal = Vector3.up;
            if (heelGrounded)
            {
                groundedPos = kneeHitInfo.point;
                Vector3 footForward = Vector3.ProjectOnPlane(hipTransform.rotation * Vector3.forward, kneeHitInfo.normal);
                rotation = Quaternion.LookRotation(footForward, kneeHitInfo.normal);
                groundNormal = kneeHitInfo.normal;
            }

            return heelGrounded;
        }
    }
}
