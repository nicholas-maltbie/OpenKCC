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
    [Serializable]
    public class FootIKWeight : UnityEngine.Object
    {
        public float leftFootIKWeight;
        public float rightFootIKWeight;
    }

    /// <summary>
    /// Script to attach player feet to ground.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class HumanoidFootIK : MonoBehaviour
    {
        public static readonly Foot[] Feet = new Foot[]{Foot.LeftFoot, Foot.RightFoot};
        public const string LeftFootIKWeight = "LeftFootIKWeight";
        public const string RightFootIKWeight = "RightFootIKWeight";

        private State leftFootState = State.Released;
        private State rightFootState = State.Released;

        private bool OverGroundThreshold(Foot foot) => GetFootAnimationWeight(foot) >= 0.6f;
        private bool UnderReleaseThreshold(Foot foot) => GetFootAnimationWeight(foot) <= 0.4f;

        private float GetFootAnimationWeight(Foot foot) => foot == Foot.LeftFoot ? animator.GetFloat(LeftFootIKWeight) : animator.GetFloat(RightFootIKWeight);

        private State GetFootState(Foot foot) => foot == Foot.LeftFoot ? leftFootState : rightFootState;
        private void SetFootState(Foot foot, State state)
        {
            switch (foot)
            {
                case Foot.LeftFoot:
                    leftFootState = state;
                    break;
                case Foot.RightFoot:
                    rightFootState = state;
                    break;
            }
        }

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

        public bool lockFeetPos;
        public float deltaRotation = 30.0f;
        public float groundCheckDist = 1.5f;

        public float footGroundedHeight = 0.05f;

        private Animator animator;

        private Vector3 leftFootTargetPos;
        private Vector3 rightFootTargetPos;
        private Quaternion leftFootTargetRot;
        private Quaternion rightFootTargetRot;
        private float leftFootIKWeight;
        private float rightFootIKWeight;

        public void SetFootTarget(Foot foot, Vector3 pos, Quaternion rot)
        {
            switch(foot)
            {
                case Foot.LeftFoot:
                    leftFootTargetPos = pos;
                    leftFootTargetRot = rot;
                    break;
                case Foot.RightFoot:
                    rightFootTargetPos = pos;
                    rightFootTargetRot = rot;
                    break;
            }
        }

        public float GetFootIKWeight(Foot foot)
        {
            return foot == Foot.LeftFoot ? leftFootIKWeight : rightFootIKWeight;
        }

        public void LerpFootIKWeight(Foot foot)
        {
            float currentWeight = GetFootIKWeight(foot);
            float targetWeight = GetFootAnimationWeight(foot);

            switch (GetFootState(foot))
            {
                case State.Grounded:
                    targetWeight = Mathf.Clamp(targetWeight, 0.9f, 1.0f);
                    break;
                case State.Released:
                    targetWeight = Mathf.Clamp(targetWeight, 0.0f, 0.1f);
                    break;
            }

            float lerpValue = Mathf.Clamp(Time.deltaTime * 10, 0, 1);
            float newWeight = Mathf.Lerp(currentWeight, targetWeight, lerpValue);
            switch (foot)
            {
                case Foot.LeftFoot:
                    leftFootIKWeight = newWeight;
                    break;
                case Foot.RightFoot:
                    rightFootIKWeight = newWeight;
                    break;
            }
        }

        public void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void OnAnimatorIK(int layerIndex)
        {
            foreach (Foot foot in Feet)
            {
                LerpFootIKWeight(foot);

                State currentState = GetFootState(foot);
                
                switch (currentState)
                {
                    case State.Grounded:
                        bool shouldRelease = UnderReleaseThreshold(foot);
                        if (shouldRelease)
                        {
                            SetFootState(foot, State.Released);
                        }

                        break;
                    case State.Released:
                        bool shouldGround = OverGroundThreshold(foot);
                        if (shouldGround)
                        {
                            bool onGround = GetFootGroundedTransform(foot, out Vector3 groundedPos, out Quaternion groundedRot);
                            if (onGround)
                            {
                                groundedPos += Vector3.up * footGroundedHeight;
                                SetFootTarget(foot, groundedPos, groundedRot);
                                SetFootState(foot, State.Grounded);
                            }
                        }

                        break;
                }

                AvatarIKGoal goal = foot == Foot.LeftFoot ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
                Vector3 target = foot == Foot.LeftFoot ? leftFootTargetPos : rightFootTargetPos;
                Quaternion rotation = foot == Foot.LeftFoot ? leftFootTargetRot : rightFootTargetRot;
                float weight = GetFootIKWeight(foot);

                animator.SetIKPosition(goal, target);
                animator.SetIKRotation(goal, rotation);
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
