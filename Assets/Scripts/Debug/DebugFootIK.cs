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
# if UNITY_EDITOR
using UnityEditor;
# endif
using UnityEngine;

namespace nickmaltbie.OpenKCC.Animation
{
    public enum DebugType
    {
        Disabled = 0,
        DrawLines = 1,
        PlaceFeet = 2,
        LiftFeet = 3,
    }

    /// <summary>
    /// Script to attach player feet to ground as the player
    /// moves around naturally.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class DebugFootIK : MonoBehaviour
    {
        public static readonly Foot[] Feet = new Foot[] { Foot.LeftFoot, Foot.RightFoot };

        private Animator animator;

        public Color debugColor = Color.blue;

        public DebugType debugType;

        [Range(1, 100)]
        public float lineThickness = 10.0f;

        public float groundCheckDist = 1.5f;

        public float footGroundedHeight = 0.05f;

        public float stepHeight = 0.1f;

        public float strideTime = 0.25f;

        public float strideThresholdDistance = 0.75f;

        public float strideThresholdDegrees = 45;

        public LayerMask feetCollisionDetection = -1;

        internal IRaycastHelper raycastHelper = RaycastHelper.Instance;

        public FootTarget LeftFootTarget { get; private set; }

        public FootTarget RightFootTarget { get; private set; }

        public float MostRecentStrideTime => Mathf.Max(LeftFootTarget.StrideStartTime, RightFootTarget.StrideStartTime);

        public bool CanTakeStride => (MostRecentStrideTime + strideTime) <= Time.time;

        public void Awake()
        {
            animator = GetComponent<Animator>();
            SetupTargets();
        }

        public void Update()
        {
            foreach (Foot foot in Feet)
            {
                GetFootTarget(foot).LerpFootIKWeight();
            }
        }

        public void OnValidate()
        {
            animator = GetComponent<Animator>();
            SetupTargets();
        }

        private void SetupTargets()
        {
            LeftFootTarget = new FootTarget(Foot.LeftFoot, animator, stepHeight, strideTime, footGroundedHeight);
            RightFootTarget = new FootTarget(Foot.RightFoot, animator, stepHeight, strideTime, footGroundedHeight);
        }

        public void OnAnimatorIK(int layerIndex)
        {
            UpdateFootTargets();
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

        public void OnDrawGizmos()
        {
            foreach (Foot foot in Feet)
            {
                _ = GetFootTarget(foot);
                GetFootGroundedTransform(foot, out _, out _, out _, out _, out _, debugLine: true);
            }
        }

        /// <summary>
        /// Update the left and right foot targets based on their current state.
        /// </summary>
        private void UpdateFootTargets()
        {
            _ = Vector3.zero;
            _ = Quaternion.identity;
            _ = Vector3.up;
            GameObject floor = null;
            _ = transform.forward;

            foreach (Foot foot in Feet)
            {
                FootTarget target = GetFootTarget(foot);
                Vector3 groundedPos;
                Quaternion rotation;
                Vector3 groundNormal;
                Vector3 footForward;
                switch (debugType)
                {
                    default:
                    case DebugType.Disabled:
                    case DebugType.DrawLines:
                        break;
                    case DebugType.PlaceFeet:
                        if (target.State == FootState.Grounded)
                        {
                            bool grounded = GetFootGroundedTransform(foot, out groundedPos, out rotation, out groundNormal, out floor, out footForward, false);
                            if (!grounded)
                            {
                                target.ReleaseFoot();
                            }
                            else
                            {
                                target.UpdateStrideTarget(groundedPos + groundNormal * footGroundedHeight, rotation, groundNormal, true);
                            }
                        }
                        else
                        {
                            bool grounded = GetFootGroundedTransform(foot, out groundedPos, out rotation, out groundNormal, out floor, out footForward, false);
                            if (grounded)
                            {
                                target.StartStride(groundedPos, rotation, floor, footForward, groundNormal, false);
                            }
                        }

                        break;
                    case DebugType.LiftFeet:
                        if (target.State == FootState.Grounded)
                        {
                            bool shouldRelease = target.UnderReleaseThreshold();
                            bool grounded = GetFootTargetPosViaHips(foot, out groundedPos, out rotation, out groundNormal, out floor, out footForward, false);
                            if (!grounded)
                            {
                                target.ReleaseFoot();
                            }
                            else if (grounded)
                            {
                                // Check if we have exceeded the target angle or target distance
                                float deltaDist = Vector3.ProjectOnPlane(groundedPos - target.TargetFootPosition, Vector3.up).magnitude;
                                float deltaAngle = Quaternion.Angle(rotation, target.TargetFootRotation);
                                bool distThreshold = deltaDist >= strideThresholdDistance;
                                bool turnThreshold = deltaAngle >= strideThresholdDegrees;

                                if (CanTakeStride && !target.MidStride)
                                {
                                    if (distThreshold)
                                    {
                                        target.ReleaseFoot();
                                    }
                                    else if (turnThreshold)
                                    {
                                        target.StartStride(groundedPos, rotation, floor, footForward, groundNormal, true);
                                    }
                                }
                                else if (target.MidStride && target.CanUpdateStrideTarget())
                                {
                                    target.UpdateStrideTarget(groundedPos, rotation, Vector3.up);
                                }
                            }
                        }
                        else
                        {
                            bool grounded = GetFootGroundedTransform(foot, out groundedPos, out rotation, out groundNormal, out floor, out footForward, false);
                            if (target.OverGroundThreshold() && grounded)
                            {
                                target.StartStride(groundedPos, rotation, floor, footForward, groundNormal, false);
                            }
                        }

                        break;
                }

                UpdateFootIKState(target);
            }
        }

        private bool GetFootGroundedTransform(Foot foot, out Vector3 groundedPos, out Quaternion rotation, out Vector3 groundNormal, out GameObject floor, out Vector3 footForward, bool debugLine = false)
        {
            HumanBodyBones kneeBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg;
            Transform kneeTransform = animator.GetBoneTransform(kneeBone);
            Transform footTransform = GetFootTransform(foot);
            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);

            var heightOffset = Vector3.Project(kneeTransform.position - footTransform.position, Vector3.up);
            bool grounded = GetFootGroundedInfo(footTransform.position + heightOffset, out groundedPos, out groundNormal, out floor, debugLine: debugLine);
            footForward = Vector3.ProjectOnPlane(hipTransform.forward, groundNormal);
            rotation = Quaternion.LookRotation(footForward, groundNormal);
            return grounded;
        }

        private bool GetFootTargetPosViaHips(Foot foot, out Vector3 groundPos, out Quaternion rotation, out Vector3 groundNormal, out GameObject floor, out Vector3 footForward, bool debugLine = false)
        {
            HumanBodyBones kneeBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg;
            Transform hipTransform = animator.GetBoneTransform(HumanBodyBones.Hips);
            Transform kneeTransform = animator.GetBoneTransform(kneeBone);
            Transform footTransform = GetFootTransform(foot);

            Vector3 footPos = footTransform.position;
            Vector3 source = Vector3.ProjectOnPlane(footPos - hipTransform.position, hipTransform.forward) + hipTransform.position;
            source.y = kneeTransform.position.y;

            bool grounded = GetFootGroundedInfo(source, out groundPos, out groundNormal, out floor, debugLine: debugLine);
            footForward = Vector3.ProjectOnPlane(hipTransform.forward, groundNormal);
            rotation = Quaternion.LookRotation(footForward, groundNormal);
            return grounded;
        }

        private Transform GetFootTransform(Foot foot)
        {
            HumanBodyBones footBone = foot == Foot.LeftFoot ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot;
            return animator.GetBoneTransform(footBone);
        }

        private void UpdateFootIKState(FootTarget target)
        {
            AvatarIKGoal goal = target.Foot == Foot.LeftFoot ? AvatarIKGoal.LeftFoot : AvatarIKGoal.RightFoot;
            Transform footTransform = GetFootTransform(target.Foot);
            Vector3 worldPos = footTransform.position;

            animator.SetIKPosition(goal, Vector3.Lerp(worldPos, target.FootIKTargetPos(), target.FootIKWeight));
            animator.SetIKRotation(goal, target.FootIKTargetRot());
            animator.SetIKPositionWeight(goal, 1);
            animator.SetIKRotationWeight(goal, target.FootIKWeight);
        }

        private bool GetFootGroundedInfo(Vector3 sourcePos, out Vector3 groundedPos, out Vector3 groundNormal, out GameObject floor, bool debugLine = false)
        {
            bool grounded = raycastHelper.DoRaycastInDirection(sourcePos, Vector3.down, groundCheckDist, out IRaycastHit hitInfo, layerMask: feetCollisionDetection);
            groundedPos = grounded ? hitInfo.point : Vector3.zero;
            groundNormal = grounded ? hitInfo.normal : Vector3.up;
            floor = grounded ? hitInfo.collider?.gameObject : null;

            if (debugLine)
            {
# if UNITY_EDITOR
                if (grounded)
                {
                    Handles.color = debugColor;
                    Handles.DrawLine(sourcePos, hitInfo.point, lineThickness);
                    Handles.DrawSolidDisc(hitInfo.point, groundNormal, 0.1f);
                    Handles.color = Color.Lerp(debugColor, Color.black, 0.9f);
                    Handles.DrawWireDisc(hitInfo.point, groundNormal, 0.1f);

                    Handles.color = Color.Lerp(debugColor, Color.white, 0.9f);
                    Handles.DrawLine(hitInfo.point, sourcePos + Vector3.down * groundCheckDist, lineThickness);

                    if (debugType == DebugType.PlaceFeet)
                    {
                        Handles.color = Color.green;
                        Handles.DrawLine(hitInfo.point, hitInfo.point + groundNormal * 0.25f, lineThickness / 2);

                        var right = Vector3.Cross(transform.forward, groundNormal);
                        Vector3 forward = -Vector3.Cross(right, groundNormal);
                        Handles.color = Color.red;
                        Handles.DrawLine(
                            hitInfo.point,
                            hitInfo.point + Quaternion.LookRotation(forward, groundNormal) * Vector3.forward * 0.25f,
                            lineThickness / 2);
                    }
                }
                else
                {
                    Handles.color = debugColor;
                    Handles.DrawLine(sourcePos, sourcePos + Vector3.down * groundCheckDist, lineThickness);
                }
# endif
            }

            return grounded;
        }
    }
}
