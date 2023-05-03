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
        public static readonly Foot[] Feet = new Foot[]{/*Foot.LeftFoot, */Foot.RightFoot};
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

            public Vector3 fromFootPosition = Vector3.zero;
            private Quaternion fromFootRotation = Quaternion.identity;
            private float strideHeight;
            private float strideTime;
            private float footGroundedHeight;
            private Vector3 footVelocity = Vector3.zero;
            private Quaternion footAngularVelocity = Quaternion.identity;
            private float smoothedAnimationWeight;
            private float smoothedAnimationWeightVelocity;

            private Vector3 raisedFootVelocity = Vector3.zero;
            private Vector3 footSmoothVelocity = Vector3.zero;
            private Vector3 footSmoothAngularVelocity = Vector3.zero;

            public FootTarget(Foot foot, Animator animator, float strideHeight, float strideTime, float footGroundedHeight)
            {
                this.foot = foot;
                this.animator = animator;

                this.strideHeight = strideHeight;
                this.strideTime = strideTime;
                this.footGroundedHeight = footGroundedHeight;
            }

            public float StrideStartTime { get; private set; } = Mathf.NegativeInfinity;
            public Vector3 GroundNormal { get; private set; } = Vector3.up;
            public Vector3 TargetFootPosition { get; private set; } = Vector3.zero;
            public Quaternion TargetFootRotation { get; private set; } = Quaternion.identity;
            public float FootIKWeight { get; private set; }
            public State FootState { get; private set; }
            public bool UseBump { get; set; }
            protected float RemainingStrideTime => StrideStartTime + strideTime - Time.time;

            public bool MidStride
            {
                get
                {
                    if (FootState == State.Grounded && UseBump)
                    {
                        return RemainingStrideTime > 0;
                    }
                    else if (FootState == State.Grounded)
                    {
                        return FootIKWeight <= 0.95f;
                    }
                    else
                    {
                        return FootIKWeight >= 0.05f;
                    }
                }
            }

            public bool CanUpdateStrideTarget()
            {
                if (FootState == State.Grounded && UseBump)
                {
                    return RemainingStrideTime > strideTime * 0.5f;
                }
                else if (FootState == State.Grounded)
                {
                    return FootIKWeight <= 0.5f;
                }
                else
                {
                    return FootIKWeight >= 0.5f;
                }
            }


            public float GetFootAnimationWeight() => foot == Foot.LeftFoot ? animator.GetFloat(LeftFootIKWeight) : animator.GetFloat(RightFootIKWeight);

            public bool OverGroundThreshold() => GetFootAnimationWeight() >= 0.1f;
            public bool UnderReleaseThreshold() => GetFootAnimationWeight() <= 0.1f;

            public Vector3 FootIKTargetPos()
            {
                if (RemainingStrideTime <= 0)
                {
                    return TargetFootPosition + GroundNormal * footGroundedHeight;
                }

                float fraction = 1 - Mathf.Clamp(RemainingStrideTime / strideTime, 0, 1);
                Vector3 lerpPos = Vector3.Lerp(fromFootPosition, TargetFootPosition, fraction);
                Vector3 verticalOffset = UseBump ? Vector3.up * strideHeight * Mathf.Sin(fraction * Mathf.PI) : Vector3.zero;;
                return lerpPos + verticalOffset + GroundNormal * footGroundedHeight;
            }

            public Quaternion FootIKTargetRot()
            {
                if (RemainingStrideTime <= 0)
                {
                    return TargetFootRotation;
                }

                float fraction = 1 - Mathf.Clamp(RemainingStrideTime / strideTime, 0, 1);
                return Quaternion.Lerp(fromFootRotation, TargetFootRotation, fraction);
            }

            public void LerpFootIKWeight()
            {
                float currentWeight = FootIKWeight;
                float targetWeight = FootState == State.Grounded ? 1.0f : 0.0f;
                FootIKWeight = Mathf.SmoothDamp(currentWeight, targetWeight, ref footIKWeightVelocityLerp, strideTime, Mathf.Infinity, Time.deltaTime);
                FootIKWeight = Mathf.Clamp(FootIKWeight, 0, 1);
            }

            public void ReleaseFoot()
            {
                UnityEngine.Debug.Log($"Foot:{foot} releasing");

                FootState = State.Released;
            }

            public void StartStride(Vector3 toPos, Quaternion toRot, Vector3 groundNormal, bool bumpStep)
            {
                UnityEngine.Debug.Log($"Foot:{foot} starting stride bumpStep:{bumpStep}");

                Vector3 currentPos = FootIKTargetPos();
                Quaternion currentRot = FootIKTargetRot();

                fromFootPosition = TargetFootPosition;
                fromFootRotation = TargetFootRotation;
                TargetFootPosition = toPos;
                TargetFootRotation = toRot;
                this.UseBump = bumpStep;
                GroundNormal = groundNormal;
                FootState = State.Grounded;
                StrideStartTime = Time.time;
            }

            public void UpdateStrideTarget(Vector3 toPos, Quaternion toRot, Vector3 groundNormal)
            {
                TargetFootPosition = Vector3.SmoothDamp(TargetFootPosition, toPos, ref raisedFootVelocity, 0.05f, Mathf.Infinity, Time.deltaTime);
                TargetFootRotation = toRot;
                GroundNormal = groundNormal;
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
                UnityEngine.Debug.DrawLine(target.TargetFootPosition + Vector3.up, target.TargetFootPosition, Color.red);
                UnityEngine.Debug.DrawLine(target.fromFootPosition + Vector3.up, target.fromFootPosition, Color.blue);
                UnityEngine.Debug.DrawLine(target.FootIKTargetPos() + Vector3.up * 0.5f, target.FootIKTargetPos(), Color.green);
                bool grounded = GetFootTargetPosViaHips(foot, out Vector3 hipGroundPos, out Quaternion hipGroundRot, out Vector3 groundNormal);

                switch (target.FootState)
                {
                    case State.Grounded:
                        bool shouldRelease = target.UnderReleaseThreshold();
                        if (shouldRelease)
                        {
                            target.ReleaseFoot();
                        }
                        else if (grounded)
                        {
                            // Check if we have exceeded the target angle or target distance
                            float deltaDist = Vector3.Distance(hipGroundPos, target.TargetFootPosition);
                            float deltaAngle = Quaternion.Angle(hipGroundRot, target.TargetFootRotation);
                            bool distThreshold = deltaDist >= strideThresholdDistance;
                            bool turnThreshold = deltaAngle >= strideThresholdDegrees;

                            // When replacing the foot, try to move the foot up towards the hip if the
                            // step is behind the hips.
                            if (!target.MidStride && (distThreshold || turnThreshold) && CanTakeStride)
                            {
                                target.StartStride(hipGroundPos, hipGroundRot, groundNormal, true);
                            }
                            else if (target.CanUpdateStrideTarget())
                            {
                                target.UpdateStrideTarget(hipGroundPos, hipGroundRot, groundNormal);
                            }
                        }
                        break;
                    case State.Released:
                        if (target.OverGroundThreshold())
                        {
                            bool onGround = GetFootGroundedTransform(foot, out Vector3 groundedPos, out Quaternion groundedRot, out groundNormal);
                            if (onGround)
                            {
                                target.StartStride(groundedPos, groundedRot, groundNormal, false);
                            }
                        }
                        else
                        {
                            Transform footTransform = GetFootTransform(foot);
                            target.UpdateStrideTarget(footTransform.position, target.TargetFootRotation, Vector3.up);
                        }
                        break;
                }

                AvatarIKGoal goal = foot == Foot.LeftFoot ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
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

        private Transform GetToeTransform(Foot foot)
        {
            HumanBodyBones toesBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftToes : HumanBodyBones.RightToes;
            return animator.GetBoneTransform(toesBone);
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
            Vector3 onHipPlane = Vector3.ProjectOnPlane(footPos - hipTransform.position, hipForward) + hipTransform.position;
            Vector3 deltaToHipPlane = onHipPlane - footPos;

            Vector3 source = Vector3.ProjectOnPlane(onHipPlane - hipTransform.position, hipForward) + hipTransform.position;
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
                hipGroundedPos = onHipPlane;
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
