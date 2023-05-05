
using UnityEngine;

namespace nickmaltbie.OpenKCC.Animation
{
    public class FootTarget
    {
        public const string LeftFootIKWeight = "LeftFootIKWeight";
        public const string RightFootIKWeight = "RightFootIKWeight";


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

        public const float MinFootGroundedWeight = 0.9f;
        public const float MaxFootReleasedWeight = 0.1f;

        private Foot foot;
        private Animator animator;
        private float footIKWeightVelocityLerp;

        public Vector3 fromFootPosition = Vector3.zero;
        private Quaternion fromFootRotation = Quaternion.identity;
        private float strideHeight;
        private float strideTime;
        private float placeBlendTime;
        private float footGroundedHeight;
        private Vector3 footVelocity = Vector3.zero;
        private Quaternion footAngularVelocity = Quaternion.identity;
        private float smoothedAnimationWeight;
        private float smoothedAnimationWeightVelocity;
        private Vector3 raisedFootVelocity = Vector3.zero;

        public FootTarget(Foot foot, Animator animator, float strideHeight, float strideTime, float placeBlendTime, float footGroundedHeight)
        {
            this.foot = foot;
            this.animator = animator;

            this.strideHeight = strideHeight;
            this.strideTime = strideTime;
            this.placeBlendTime = placeBlendTime;
            this.footGroundedHeight = footGroundedHeight;
        }

        public float StrideStartTime { get; private set; } = Mathf.NegativeInfinity;
        public Vector3 TargetFootPosition { get; private set; } = Vector3.zero;
        public Vector3 FootForward { get; set; }
        public Quaternion TargetFootRotation { get; private set; } = Quaternion.identity;
        public float FootIKWeight { get; private set; }
        public FootState State { get; private set; }
        public GameObject Floor { get; private set; }
        public bool UseBump { get; set; }
        protected float TotalStrideTime => UseBump ? strideTime : placeBlendTime;
        protected float RemainingStrideTime => StrideStartTime + TotalStrideTime - Time.time;

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
            if (State == FootState.Grounded && UseBump)
            {
                return RemainingStrideTime > strideTime * 0.25f;
            }
            else if (State == FootState.Grounded)
            {
                return false;
            }
            else
            {
                return FootIKWeight <= 0.01f;
            }
        }

        public float GetFootAnimationWeight() => foot == Foot.LeftFoot ? animator.GetFloat(LeftFootIKWeight) : animator.GetFloat(RightFootIKWeight);

        public bool OverGroundThreshold() => GetFootAnimationWeight() >= 0.5f;
        public bool UnderReleaseThreshold() => GetFootAnimationWeight() <= 0.5f;

        public Vector3 FootIKTargetPos()
        {
            if (RemainingStrideTime <= 0)
            {
                return TargetFootPosition + Vector3.up * footGroundedHeight;
            }

            float fraction = 1 - Mathf.Clamp(RemainingStrideTime / TotalStrideTime, 0, 1);
            Vector3 lerpPos = Vector3.Lerp(fromFootPosition, TargetFootPosition, SmoothValue(fraction));
            Vector3 verticalOffset = UseBump ? Vector3.up * strideHeight * Mathf.Sin(fraction * Mathf.PI) : Vector3.zero;;
            return lerpPos + verticalOffset + Vector3.up * footGroundedHeight;
        }

        public Quaternion FootIKTargetRot()
        {
            if (RemainingStrideTime <= 0 || !UseBump)
            {
                return TargetFootRotation;
            }

            float fraction = 1 - Mathf.Clamp(RemainingStrideTime / TotalStrideTime, 0, 1);
            return Quaternion.Lerp(fromFootRotation, TargetFootRotation, SmoothValue(fraction));
        }

        public void LerpFootIKWeight()
        {
            float currentWeight = FootIKWeight;
            float targetWeight = State == FootState.Grounded ? 1.0f : 0.0f;
            FootIKWeight = Mathf.SmoothDamp(currentWeight, targetWeight, ref footIKWeightVelocityLerp, TotalStrideTime, Mathf.Infinity, Time.deltaTime);
            FootIKWeight = Mathf.Clamp(FootIKWeight, 0, 1);
        }

        public void ReleaseFoot()
        {
            State = FootState.Released;
            Floor = null;
        }

        public void StartStride(Vector3 toPos, Quaternion toRot, GameObject floor, Vector3 footForward, bool bumpStep)
        {
            fromFootPosition = TargetFootPosition;
            fromFootRotation = TargetFootRotation;
            TargetFootPosition = toPos;
            TargetFootRotation = toRot;
            Floor = floor;
            FootForward = footForward;
            this.UseBump = bumpStep;
            State = FootState.Grounded;
            StrideStartTime = Time.time;
        }

        public void UpdateStrideTarget(Vector3 toPos, Quaternion toRot, bool force = false)
        {
            if (force)
            {
                TargetFootPosition = toPos;
            }
            else
            {
                TargetFootPosition = Vector3.SmoothDamp(TargetFootPosition, toPos, ref raisedFootVelocity, strideTime, Mathf.Infinity, Time.deltaTime);
            }
            TargetFootRotation = toRot;
        }
    }
}