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

using nickmaltbie.TestUtilsUnity;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Animation
{
    /// <summary>
    /// FootTarget for easily managing the player's current
    /// IK foot target position and weight.
    /// </summary>
    public class FootTarget
    {
        /// <summary>
        /// Threshold for animation weight value for changing between grounded and not grounded.
        /// </summary>
        public const float ThresholdGroundedStateChange = 0.5f;

        /// <summary>
        /// Threshold for animation IK weight value to consider the foot grounded.
        /// </summary>
        public const float ThresholdIKWeightGrounded = 0.95f;

        /// <summary>
        /// Threshold for animation IK weight value to consider the foot released.
        /// </summary>
        public const float ThresholdIKWeightReleased = 0.05f;

        /// <summary>
        /// Threshold fraction of animation at which a stride target can no longer be updated.
        /// </summary>
        public const float ThresholdFractionStrideNoUpdate = 0.15f;

        /// <summary>
        /// Name for animator curve to define the left foot IK weight.
        /// </summary>
        public const string LeftFootIKWeight = "LeftFootIKWeight";

        /// <summary>
        /// Name for animator curve to define the right foot IK weight.
        /// </summary>
        public const string RightFootIKWeight = "RightFootIKWeight";

        /// <summary>
        /// Smooth a value evenly between 0 and 1
        /// using a sine function.
        /// </summary>
        /// <param name="x">Input value between [0,1]</param>
        /// <returns>Smoothed value when placed on a smooth sine curve.</returns>
        public static float SmoothValue(float x)
        {
            if (x <= 0)
            {
                return 0;
            }
            else if (x >= 1)
            {
                return 1;
            }
            else
            {
                return (Mathf.Sin(Mathf.PI * (x - 0.5f)) + 1) * 0.5f;
            }
        }

        /// <summary>
        /// Animator for managing the player avatar.
        /// </summary>
        private Animator animator;

        /// <summary>
        /// Current velocity of the lerped foot IK weight for soothing.
        /// </summary>
        private float footIKWeightVelocityLerp;

        /// <summary>
        /// Position foot is being moved from when grounding.
        /// </summary>
        private Vector3 fromFootPosition = Vector3.zero;

        /// <summary>
        /// Rotation foot is being moved from when grounding.
        /// </summary>
        private Quaternion fromFootRotation = Quaternion.identity;

        /// <summary>
        /// Current velocity of the foot when smoothing foot position while raised.
        /// </summary>
        private Vector3 raisedFootVelocity = Vector3.zero;

        /// <summary>
        /// Unity service for managing time delta.
        /// </summary>
        internal IUnityService unityService = UnityService.Instance;

        /// <summary>
        /// Construct a foot target with a given set of configurations
        /// and parameters.
        /// </summary>
        /// <param name="foot">Foot being controlled.</param>
        /// <param name="animator">Animator managing avatar control.</param>
        /// <param name="strideHeight">Height of stride for short steps.</param>
        /// <param name="strideTime">Time of stride for short steps.</param>
        /// <param name="footGroundedHeight">Height to raise foot of ground when placed.</param>
        public FootTarget(Foot foot, Animator animator, float strideHeight, float strideTime, float footGroundedHeight)
        {
            Foot = foot;
            this.animator = animator;

            StrideHeight = strideHeight;
            FootGroundedHeight = footGroundedHeight;
            StrideTime = strideTime;
        }

        /// <summary>
        /// Which foot does this target correspond to.
        /// </summary>
        public Foot Foot { get; private set; }

        /// <summary>
        /// Time required to take a full stride for the foot.
        /// </summary>
        public float StrideTime { get; private set; }

        /// <summary>
        /// Height foot is lifted to mid stride.
        /// </summary>
        public float StrideHeight { get; private set; }

        /// <summary>
        /// Height foot is raised above the ground when taking a small step.
        /// </summary>
        public float FootGroundedHeight { get; private set; }

        /// <summary>
        /// Gets the time of the most recent stride's start.
        /// </summary>
        public float StrideStartTime { get; private set; } = Mathf.NegativeInfinity;

        /// <summary>
        /// Gets the target foot position for IK controls.
        /// </summary>
        public Vector3 TargetFootPosition { get; private set; } = Vector3.zero;

        /// <summary>
        /// Gets the forward vector for the foot rotation.
        /// </summary>
        public Vector3 FootForward { get; set; }

        /// <summary>
        /// Ground normal for surface player is standing on.
        /// </summary>
        public Vector3 GroundNormal { get; set; }

        /// <summary>
        /// Gets the target foot rotation for IK controls.
        /// </summary>
        public Quaternion TargetFootRotation { get; private set; } = Quaternion.identity;

        /// <summary>
        /// Gets the current weight value for IK foot placement.
        /// </summary>
        public float FootIKWeight { get; private set; }

        /// <summary>
        /// Gets the current state of the foot's control, either grounded
        /// or released.
        /// </summary>
        public FootState State { get; private set; }

        /// <summary>
        /// Gets the current object the foot is standing on.
        /// </summary>
        public GameObject Floor { get; private set; }

        /// <summary>
        /// Is the foot currently taking a small stride and needs to be bumped
        /// up off the ground.
        /// </summary>
        /// <value></value>
        public bool UseBump { get; private set; }

        /// <summary>
        /// Gest the remaining time in the current stride based off the current
        /// game time.
        /// </summary>
        public float RemainingStrideTime => UseBump ? StrideStartTime + StrideTime - unityService.time : 0;

        /// <summary>
        /// Gets if this foot is currently mid stride. This foot
        /// will be considered mid stride if it's currently grounded
        /// and has remaining time in the current stride
        /// of if it was just released form the ground. This should
        /// be used to stop quickly changing state between grounded and
        /// not grounded causing the foot to jitter.
        /// </summary>
        public bool MidStride
        {
            get
            {
                if (State == FootState.Grounded && UseBump)
                {
                    return RemainingStrideTime > 0;
                }
                else if (State == FootState.Grounded)
                {
                    return FootIKWeight <= ThresholdIKWeightGrounded;
                }
                else
                {
                    return FootIKWeight >= ThresholdIKWeightReleased;
                }
            }
        }

        /// <summary>
        /// Can the stride target for this foot be updated or snapped
        /// to a new position.
        /// </summary>
        /// <returns>True if the stride target can be updated, false otherwise.</returns>
        public bool CanUpdateStrideTarget()
        {
            if (State == FootState.Grounded && UseBump)
            {
                return RemainingStrideTime > StrideTime * ThresholdFractionStrideNoUpdate;
            }
            else if (State == FootState.Grounded)
            {
                return false;
            }
            else
            {
                return FootIKWeight <= ThresholdIKWeightReleased;
            }
        }

        /// <summary>
        /// Gets the current animation weight for the foot from the animator.
        /// </summary>
        /// <returns>Float animation weight for the selected foot.</returns>
        public float GetFootAnimationWeight() => Foot == Foot.LeftFoot ? animator.GetFloat(LeftFootIKWeight) : animator.GetFloat(RightFootIKWeight);

        /// <summary>
        /// Is the foot's animation weight over the grounded threshold. (should be grounded)
        /// </summary>
        /// <returns>True if the foot is over the grounded threshold, false otherwise.</returns>
        public bool OverGroundThreshold() => GetFootAnimationWeight() >= ThresholdGroundedStateChange;

        /// <summary>
        /// Is the foot's animation weight under the release threshold. (should be released)
        /// </summary>
        /// <returns>True if the foot is under the release threshold, false otherwise.</returns>
        public bool UnderReleaseThreshold() => GetFootAnimationWeight() <= ThresholdGroundedStateChange;

        /// <summary>
        /// Get the current foot IK target position in world space. This
        /// will account for any blending between the from and to position
        /// as well as any bump up for small strides.
        /// </summary>
        /// <returns>Foot IK Target position in world space.</returns>
        public Vector3 FootIKTargetPos()
        {
            if (RemainingStrideTime <= 0)
            {
                return TargetFootPosition + GroundNormal * FootGroundedHeight;
            }

            float fraction = 1 - Mathf.Clamp(RemainingStrideTime / StrideTime, 0, 1);
            var lerpPos = Vector3.Lerp(fromFootPosition, TargetFootPosition, SmoothValue(fraction));
            Vector3 verticalOffset = UseBump ? Vector3.up * StrideHeight * Mathf.Sin(fraction * Mathf.PI) : Vector3.zero;
            return lerpPos + verticalOffset + GroundNormal * FootGroundedHeight;
        }

        /// <summary>
        /// Gets the current foot IK target rotation in world space.
        /// This will account for any blending between the from and to position.
        /// </summary>
        /// <returns>Foot IK Target rotation in world space.</returns>
        public Quaternion FootIKTargetRot()
        {
            if (RemainingStrideTime <= 0 || !UseBump)
            {
                return TargetFootRotation;
            }

            float fraction = 1 - Mathf.Clamp(RemainingStrideTime / StrideTime, 0, 1);
            return Quaternion.Lerp(fromFootRotation, TargetFootRotation, SmoothValue(fraction));
        }

        /// <summary>
        /// Lerp the current foot weight based on delta time from the current
        /// target value to the new desired value based on foot state.
        /// </summary>
        public void LerpFootIKWeight()
        {
            float currentWeight = FootIKWeight;
            float targetWeight = State == FootState.Grounded ? 1.0f : 0.0f;
            FootIKWeight = Mathf.SmoothDamp(currentWeight, targetWeight, ref footIKWeightVelocityLerp, StrideTime, Mathf.Infinity, unityService.deltaTime);
            FootIKWeight = Mathf.Clamp(FootIKWeight, 0, 1);
        }

        /// <summary>
        /// Release the foot from the grounded state.
        /// </summary>
        public void ReleaseFoot()
        {
            State = FootState.Released;
            Floor = null;
        }

        /// <summary>
        /// Start a stride to either place the foot on the ground
        /// or move the foot a short distance.
        /// </summary>
        /// <param name="toPos">Desired target position of the foot.</param>
        /// <param name="toRot">Desired target rotation of the foot.</param>
        /// <param name="floor">Object the foot is placed on.</param>
        /// <param name="footForward">Forward vector for foot rotation.</param>
        /// <param name="groundNormal">Normal vector for surface player is standing on.</param>
        /// <param name="bumpStep">Is this a small bump from a currently grounded state.</param>
        public void StartStride(Vector3 toPos, Quaternion toRot, GameObject floor, Vector3 footForward, Vector3 groundNormal, bool bumpStep)
        {
            fromFootPosition = TargetFootPosition;
            fromFootRotation = TargetFootRotation;
            TargetFootPosition = toPos;
            TargetFootRotation = toRot;
            Floor = floor;
            FootForward = footForward;
            GroundNormal = groundNormal;
            UseBump = bumpStep;
            State = FootState.Grounded;
            StrideStartTime = unityService.time;
        }

        /// <summary>
        /// Update the stride target but do not start a new step.
        /// </summary>
        /// <param name="toPos">Position to update stride target to.</param>
        /// <param name="toRot">Rotation to update stride target to.</param>
        /// <param name="groundNormal">New ground normal for foot position.</param>
        /// <param name="force">Should this blend based on current delta time or simply be forced to a new position.</param>
        public void UpdateStrideTarget(Vector3 toPos, Quaternion toRot, Vector3 groundNormal, bool force = false)
        {
            if (force)
            {
                TargetFootPosition = toPos;
                raisedFootVelocity = Vector3.zero;
            }
            else
            {
                TargetFootPosition = Vector3.SmoothDamp(TargetFootPosition, toPos, ref raisedFootVelocity, StrideTime, Mathf.Infinity, unityService.deltaTime);
            }

            GroundNormal = groundNormal;
            TargetFootRotation = toRot;
        }
    }
}
