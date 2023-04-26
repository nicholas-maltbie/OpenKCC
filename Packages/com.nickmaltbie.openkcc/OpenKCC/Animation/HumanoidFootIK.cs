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
    /// Script to attach player feet to ground.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class HumanoidFootIK : MonoBehaviour
    {
        public enum Foot
        {
            LeftFoot,
            RightFoot,
        }

        public class FootTarget
        {
            private Vector3 previousTargetPosition;
            private Quaternion previousTargetRotation;

            public Vector3 TargetPosition { get; private set; }
            public Quaternion TargetRotation { get; private set; }

            private float lastUpdateTime = Mathf.NegativeInfinity;
            public float stepTime = 0.25f;
            public float strideHeight = 0.25f;
            public float footHeight = 0.1f;

            public void UpdateTarget(Vector3 newPos, Quaternion newRotation)
            {
                previousTargetPosition = TargetPosition;
                previousTargetRotation = TargetRotation;

                TargetPosition = newPos;
                TargetRotation = newRotation;

                lastUpdateTime = Time.time;
            }

            public Vector3 GetInterpolatedPosition(float t)
            {
                return Vector3.Lerp(previousTargetPosition, TargetPosition, t) + Vector3.up * Mathf.Sin(Mathf.PI * t) * strideHeight;
            }

            public Quaternion GetInterpolatedRotation(float t)
            {
                return Quaternion.Lerp(previousTargetRotation, TargetRotation, t);
            }

            public Vector3 GetTargetPosition()
            {
                float transitionTime = lastUpdateTime + stepTime - Time.time;
                if (transitionTime > 0)
                {
                    return GetInterpolatedPosition(1 - transitionTime / stepTime) + Vector3.up * footHeight;
                }

                return TargetPosition + Vector3.up * footHeight;
            }

            public Quaternion GetTargetRotation()
            {
                float transitionTime = lastUpdateTime + stepTime - Time.time;
                if (transitionTime > 0)
                {
                    return GetInterpolatedRotation(1 - transitionTime / stepTime);
                }

                return TargetRotation;
            }

            public bool InStride()
            {
                return lastUpdateTime + stepTime - Time.time > 0;
            }
        }

        public bool lockFeetPos;
        public float strideLength = 1.25f;
        public float deltaRotation = 30.0f;
        public float groundCheckDist = 0.35f;
        public float footHeight = 0.1f;
        public float strideTime = 0.1f;
        public float strideHeight = 0.25f;

        protected bool LockLeftFoot => lockFeetPos;
        protected bool LockRightFoot => lockFeetPos;

        protected float LeftFootWeight => LockLeftFoot ? 1.0f : 0.0f;
        protected float RightFootWeight => LockRightFoot ? 1.0f : 0.0f;

        protected FootTarget LeftFootTarget = new FootTarget();
        protected FootTarget RightFootTarget = new FootTarget();

        private Animator animator;

        public void OnValidate()
        {
            LeftFootTarget.stepTime = strideTime;
            RightFootTarget.stepTime = strideTime;

            LeftFootTarget.strideHeight = strideHeight;
            RightFootTarget.strideHeight = strideHeight;

            LeftFootTarget.footHeight = footHeight;
            RightFootTarget.footHeight = footHeight;
        }

        public void Awake()
        {
            animator = GetComponent<Animator>();
        }

        public void OnAnimatorIK(int layerIndex)
        {
            UpdateFootIKState(Foot.LeftFoot);
            UpdateFootIKState(Foot.RightFoot);

            animator.SetIKPosition(AvatarIKGoal.LeftFoot, LeftFootTarget.GetTargetPosition());
            animator.SetIKPosition(AvatarIKGoal.RightFoot, RightFootTarget.GetTargetPosition());
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, LeftFootTarget.GetTargetRotation());
            animator.SetIKRotation(AvatarIKGoal.RightFoot, RightFootTarget.GetTargetRotation());

            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, LeftFootWeight);
            animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, RightFootWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, LeftFootWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, RightFootWeight);
        }

        private bool GetFootGroundedTransform(Foot foot, out Vector3 groundedPos, out Quaternion rotation)
        {
            HumanBodyBones kneeBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg;
            HumanBodyBones footBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
            Transform kneeTransform = animator.GetBoneTransform(kneeBone);
            Transform footTransform = animator.GetBoneTransform(footBone);
            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);

            Vector3 heightOffset = Vector3.Project(kneeTransform.position - footTransform.position, Vector3.up);
            Vector3 source = kneeTransform.position;

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

        private FootTarget GetFootTarget(Foot foot)
        {
            switch (foot)
            {
                case Foot.RightFoot:
                    return RightFootTarget;
                case Foot.LeftFoot:
                    return LeftFootTarget;
            }

            return default;
        }

        private void UpdateFootIKState(Foot foot)
        {
            FootTarget target = GetFootTarget(foot);
            Vector3 currentPos = target.TargetPosition;
            Quaternion currentRotation = target.TargetRotation;
            bool grounded = GetFootGroundedTransform(foot, out Vector3 groundedPos, out Quaternion groundedRotation);

            bool distanceThreshold = Vector3.Distance(currentPos, groundedPos) >= strideLength;
            bool rotationThreshold = Quaternion.Angle(currentRotation, groundedRotation) >= deltaRotation;

            if (!target.InStride() && grounded && (distanceThreshold || rotationThreshold))
            {
                target.UpdateTarget(groundedPos, groundedRotation);
            }
        }
    }
}
