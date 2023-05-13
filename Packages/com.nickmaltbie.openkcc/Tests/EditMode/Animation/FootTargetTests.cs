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

using Moq;
using nickmaltbie.OpenKCC.Animation;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Animation
{
    /// <summary>
    /// Basic tests for FootTarget in edit mode.
    /// </summary>
    [TestFixture]
    public class FootTargetTests : TestBase
    {
        private const string AnimState = "AnimState";

        private Mock<IUnityService> unityServiceMock;
        private FootTarget leftFootTarget, rightFootTarget;
        private Animator animator;

        [SetUp]
        public void SetUp()
        {
            var controller = new AnimatorController();
            controller.AddLayer("base");
            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
            controller.AddParameter(FootTarget.LeftFootIKWeight, AnimatorControllerParameterType.Float);
            controller.AddParameter(FootTarget.RightFootIKWeight, AnimatorControllerParameterType.Float);
            rootStateMachine.AddState(AnimState);
            
            animator = CreateGameObject().AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            leftFootTarget = new FootTarget(Foot.LeftFoot, animator, 0.05f, 0.25f, 0.05f, 0.15f);
            rightFootTarget = new FootTarget(Foot.RightFoot, animator, 0.05f, 0.25f, 0.05f, 0.15f);

            unityServiceMock = new Mock<IUnityService>();
            leftFootTarget.unityService = unityServiceMock.Object;
            rightFootTarget.unityService = unityServiceMock.Object;

            animator.StartPlayback();
            animator.Play(AnimState, 0);
        }

        [Test]
        public void Validate_FootTarget_SmoothValue()
        {
            int total = 100;

            // Check that the value is constantly increasing by some non zero value
            // between 0 and 1
            for (int sample = 0; sample < total; sample++)
            {
                float i = (float) sample / total;
                float j = (float) (sample + 1) / total;

                float f_i = FootTarget.SmoothValue(i);
                float f_j = FootTarget.SmoothValue(j);

                Assert.IsTrue(f_j > f_i);
            }
        }

        [Test]
        public void Verify_FootTarget_LerpedMovement()
        {
            // Set foot stride time to 1 second and time step to 0.1 seconds
            var footTarget = new FootTarget(Foot.LeftFoot, animator, 0.25f, 1.0f, 1.0f, 0.1f);
            unityServiceMock.Setup(e => e.deltaTime).Returns(0.1f);
            unityServiceMock.Setup(e => e.time).Returns(0.0f);
            footTarget.unityService = unityServiceMock.Object;

            // Assert that foot is released
            footTarget.ReleaseFoot();

            // Start a stride and check that foot will lock from position to stride position
            GameObject floor = CreateGameObject();
            footTarget.UpdateStrideTarget(Vector3.zero, Quaternion.identity, true);
            footTarget.StartStride(Vector3.forward, Quaternion.identity, floor, Vector3.forward, Vector3.up, false);

            // Assert that the foot is grounded and now mid stride
            Assert.IsTrue(footTarget.State == FootState.Grounded);
            Assert.IsTrue(footTarget.MidStride);
            float previousWeight = footTarget.FootIKWeight;

            // It should take 10 updates to fully transition to finished position
            // Assert that foot is moving towards target position now.
            for (int i = 0; i < 10; i++)
            {
                unityServiceMock.Setup(e => e.time).Returns(i / 10.0f);
                footTarget.LerpFootIKWeight();

                // Just assert that the weight is increasing
                Assert.IsTrue(previousWeight <= footTarget.FootIKWeight);
                previousWeight = footTarget.FootIKWeight;

                // Should be moving towards Vecotr3.forward.
                Vector3 expected = Vector3.forward * FootTarget.SmoothValue((float) i / 10) + Vector3.up * 0.1f;
                TestUtils.AssertInBounds(footTarget.FootIKTargetPos(), expected);
                TestUtils.AssertInBounds(Quaternion.Angle(footTarget.FootIKTargetRot(), Quaternion.identity), 0, 1.0f);
            }

            // After 10 updates, foot should be fully grounded and weight should be close to 1.0f
            while (footTarget.MidStride)
            {
                footTarget.LerpFootIKWeight();
            }

            Assert.IsFalse(footTarget.MidStride);
            unityServiceMock.Setup(e => e.time).Returns(2.0f);
            footTarget.LerpFootIKWeight();
            TestUtils.AssertInBounds(footTarget.FootIKTargetPos(), new Vector3(0, 0.1f, 1.0f));
            TestUtils.AssertInBounds(Quaternion.Angle(footTarget.FootIKTargetRot(), Quaternion.identity), 0, 1.0f);
            TestUtils.AssertInBounds(footTarget.FootIKWeight, FootTarget.ThresholdIKWeightGrounded, 0.01f);
        }

        [Test]
        public void Validate_FootTarget_UpdateStrideTarget()
        {
            // If updating with force, should snap right away.
            leftFootTarget.UpdateStrideTarget(Vector3.forward, Quaternion.identity, true);
            Assert.AreEqual(Vector3.forward, leftFootTarget.TargetFootPosition);
            Assert.AreEqual(Quaternion.identity, leftFootTarget.TargetFootRotation);
            leftFootTarget.UpdateStrideTarget(Vector3.zero, Quaternion.identity, true);
            Assert.AreEqual(Vector3.zero, leftFootTarget.TargetFootPosition);
            Assert.AreEqual(Quaternion.identity, leftFootTarget.TargetFootRotation);

            // Measure distance and ensure it keeps dropping
            float previousDelta = Vector3.Distance(leftFootTarget.TargetFootPosition, Vector3.forward);
            for (int i = 0; i < 100; i++)
            {
                leftFootTarget.UpdateStrideTarget(Vector3.forward, Quaternion.identity, false);
                float newDelta = Vector3.Distance(leftFootTarget.TargetFootPosition, Vector3.forward);
                Assert.IsTrue(newDelta <= previousDelta);
                previousDelta = newDelta;
            }
        }

        [Test]
        public void Validate_FootTarget_StrideState()
        {
            float time = 0.0f;
            GameObject floor = CreateGameObject();
            unityServiceMock.Setup(e => e.deltaTime).Returns(0.05f);
            unityServiceMock.Setup(e => e.time).Returns(() => time);

            // Foot should start off released and able to update stride target
            Assert.AreEqual(FootState.Released, leftFootTarget.State);
            Assert.IsTrue(leftFootTarget.CanUpdateStrideTarget());
            Assert.AreEqual(0.0f, leftFootTarget.FootIKWeight);

            // Start basic action to ground foot
            leftFootTarget.StartStride(Vector3.forward, Quaternion.identity, floor, Vector3.forward, Vector3.up, false);
            Assert.IsFalse(leftFootTarget.CanUpdateStrideTarget());

            // Assert that the foot is now mid stride and remains mid stride until
            // the foot ik weight drops below the threshold ik weight grounded
            while (leftFootTarget.FootIKWeight >= FootTarget.ThresholdIKWeightGrounded)
            {
                Assert.IsTrue(leftFootTarget.MidStride);
                Assert.IsFalse(leftFootTarget.CanUpdateStrideTarget());
                leftFootTarget.LerpFootIKWeight();
                time += 0.05f;
            }

            // Once the foot is above threshold, it should be fully grounded
            while (leftFootTarget.MidStride)
            {
                leftFootTarget.LerpFootIKWeight();
                time += 0.05f;
            }

            Assert.AreEqual(FootState.Grounded, leftFootTarget.State);
            Assert.IsFalse(leftFootTarget.MidStride);
            Assert.IsFalse(leftFootTarget.CanUpdateStrideTarget());

            // Now take a small bump step and assert that we are mid
            // stride as long as the stride time is greater than zero
            Quaternion previousRotation = leftFootTarget.FootIKTargetRot();
            leftFootTarget.StartStride(Vector3.left, Quaternion.LookRotation(Vector3.left, Vector3.up), floor, Vector3.left, Vector3.up, true);
            while (leftFootTarget.RemainingStrideTime > 0)
            {
                bool expectedStrideUpdate = leftFootTarget.RemainingStrideTime >= leftFootTarget.StrideTime * FootTarget.ThresholdFractionStrideNoUpdate;

                Assert.IsTrue(leftFootTarget.MidStride);
                Assert.AreEqual(expectedStrideUpdate, leftFootTarget.CanUpdateStrideTarget());
                leftFootTarget.LerpFootIKWeight();
                time += 0.01f;

                // Also... assert check our delta rotation
                Assert.IsTrue(Quaternion.Angle(previousRotation, leftFootTarget.FootIKTargetRot()) >= 0);
                previousRotation = leftFootTarget.FootIKTargetRot();
            }

            Assert.IsFalse(leftFootTarget.MidStride);
            Assert.IsFalse(leftFootTarget.CanUpdateStrideTarget());

            // Assert that we are close to our target rotation
            Assert.IsTrue(Quaternion.Angle(Quaternion.LookRotation(Vector3.left, Vector3.up), leftFootTarget.FootIKTargetRot()) <= 5.0f);

            // Finally release the foot and assert it's mid stride until
            // foot IK Weight is below a specific value.
            leftFootTarget.ReleaseFoot();
            Assert.IsTrue(leftFootTarget.MidStride);
            Assert.IsFalse(leftFootTarget.CanUpdateStrideTarget());

            while(leftFootTarget.MidStride)
            {
                leftFootTarget.LerpFootIKWeight();
                time += 0.01f;

                if (leftFootTarget.FootIKWeight <= FootTarget.ThresholdIKWeightReleased)
                {
                    Assert.IsTrue(leftFootTarget.CanUpdateStrideTarget());
                }
            }
        }

        [Test]
        public void Validate_FootTarget_GetAnimationCurveValue()
        {
            for (float sample = 0; sample <= 1.0f; sample += 0.001f)
            {
                float rightValue = 1 - sample;
                float leftValue = sample;

                animator.SetFloat(FootTarget.RightFootIKWeight, rightValue);
                animator.SetFloat(FootTarget.LeftFootIKWeight, leftValue);

                Assert.AreEqual(rightValue, rightFootTarget.GetFootAnimationWeight());
                Assert.AreEqual(leftValue, leftFootTarget.GetFootAnimationWeight());

                // Also verify thresholds
                Assert.AreEqual(leftValue >= FootTarget.ThresholdGroundedStateChange, leftFootTarget.OverGroundThreshold());
                Assert.AreEqual(leftValue <= FootTarget.ThresholdGroundedStateChange, leftFootTarget.UnderReleaseThreshold());
                Assert.AreEqual(rightValue >= FootTarget.ThresholdGroundedStateChange, rightFootTarget.OverGroundThreshold());
                Assert.AreEqual(rightValue <= FootTarget.ThresholdGroundedStateChange, rightFootTarget.UnderReleaseThreshold());
            }
        }
    }
}
