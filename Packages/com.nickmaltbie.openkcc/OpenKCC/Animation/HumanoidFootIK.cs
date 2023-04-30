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
            private Vector3 footPosition;
            private Quaternion footRotation;
            private Animator animator;
            private float velocityLerp;
            private float smoothTime = 0.1f;

            public FootTarget(Foot foot, Animator animator)
            {
                this.foot = foot;
                this.animator = animator;
            }

            public float FootIKWeight { get; private set; }
            public State FootState { get; private set; }

            public float GetFootAnimationWeight() => foot == Foot.LeftFoot ? animator.GetFloat(LeftFootIKWeight) : animator.GetFloat(RightFootIKWeight);

            public bool OverGroundThreshold() => GetFootAnimationWeight() >= 0.5f;
            public bool UnderReleaseThreshold() => GetFootAnimationWeight() <= 0.5f;

            public void ReleaseFoot()
            {
                FootState = State.Released;
            }

            public Vector3 FootIKTargetPos()
            {
                return footPosition;
            }

            public Quaternion FootIKTargetRot()
            {
                return footRotation;
            }

            public void LerpFootIKWeight()
            {
                float currentWeight = FootIKWeight;
                float targetWeight = GetFootAnimationWeight();
                float lerpValue = Mathf.Clamp(Time.deltaTime * 10, 0, 1);
                FootIKWeight = Mathf.SmoothDamp(currentWeight, targetWeight, ref velocityLerp, smoothTime, 2.5f, Time.deltaTime);
            }

            public void GroundFoot(Vector3 pos, Quaternion rot)
            {
                FootState = State.Grounded;
                footPosition = pos;
                footRotation = rot;
            }
        }

        public FootTarget leftFootTarget, rightFootTarget;

        public bool lockFeetPos;
        public float deltaRotation = 30.0f;
        public float groundCheckDist = 1.5f;

        public float footGroundedHeight = 0.05f;

        private Animator animator;

        public void Awake()
        {
            animator = GetComponent<Animator>();
            leftFootTarget = new FootTarget(Foot.LeftFoot, animator);
            rightFootTarget = new FootTarget(Foot.RightFoot, animator);
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
                        if (shouldRelease)
                        {
                            UnityEngine.Debug.Log($"Releasing Foot:{foot}");
                            target.ReleaseFoot();
                        }

                        break;
                    case State.Released:
                        bool shouldGround = target.OverGroundThreshold();
                        if (shouldGround)
                        {
                            bool onGround = GetFootGroundedTransform(foot, out Vector3 groundedPos, out Quaternion groundedRot);
                            if (onGround)
                            {
                                UnityEngine.Debug.Log($"Grounding Foot:{foot}");
                                groundedPos += Vector3.up * footGroundedHeight;
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

        private bool GetFootGroundedTransform(Foot foot, out Vector3 groundedPos, out Quaternion rotation)
        {
            HumanBodyBones kneeBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg;
            HumanBodyBones footBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
            Transform kneeTransform = animator.GetBoneTransform(kneeBone);
            Transform footTransform = animator.GetBoneTransform(footBone);
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
