
using Moq;
using nickmaltbie.OpenKCC.Animation;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Animation
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class HumanoidFootIKTests : TestBase
    {
        private const string AnimState = "AnimState";

        private Mock<IUnityService> unityServiceMock;
        private Mock<IRaycastHelper> raycastHelperMock;
        private HumanoidFootIK footIK;
        private Animator animator;

        [SetUp]
        public void SetUp()
        {
            
            GameObject go = CreateGameObject();
            animator = go.AddComponent<Animator>();
            footIK = go.AddComponent<HumanoidFootIK>();

            var controller = new AnimatorController();
            controller.AddLayer("base");
            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
            controller.AddParameter(FootTarget.LeftFootIKWeight, AnimatorControllerParameterType.Float);
            controller.AddParameter(FootTarget.RightFootIKWeight, AnimatorControllerParameterType.Float);
            rootStateMachine.AddState(AnimState);
            animator.runtimeAnimatorController = controller;

            unityServiceMock = new Mock<IUnityService>();
            raycastHelperMock = new Mock<IRaycastHelper>();
            footIK.unityService = unityServiceMock.Object;
            footIK.raycastMock = raycastHelperMock.Object;
            footIK.LeftFootTarget.unityService = unityServiceMock.Object;
            footIK.RightFootTarget.unityService = unityServiceMock.Object;

            animator.StartPlayback();
            animator.Play(AnimState, 0);

            footIK.Awake();
        }

        [Test]
        public void Validate_HumanoidFootIK_ConfigUpdates()
        {
            Assert.AreEqual(footIK.stepHeight, footIK.LeftFootTarget.StrideHeight);
            Assert.AreEqual(footIK.stepHeight, footIK.RightFootTarget.StrideHeight);
            Assert.AreEqual(footIK.strideTime, footIK.LeftFootTarget.StrideTime);
            Assert.AreEqual(footIK.strideTime, footIK.RightFootTarget.StrideTime);
            Assert.AreEqual(footIK.placeBlendTime, footIK.LeftFootTarget.PlaceBlendTime);
            Assert.AreEqual(footIK.placeBlendTime, footIK.RightFootTarget.PlaceBlendTime);
            Assert.AreEqual(footIK.footGroundedHeight, footIK.LeftFootTarget.FootGroundedHeight);
            Assert.AreEqual(footIK.footGroundedHeight, footIK.RightFootTarget.FootGroundedHeight);

            footIK.stepHeight *= 2;
            footIK.strideTime *= 2;
            footIK.placeBlendTime *= 2;
            footIK.footGroundedHeight *= 2;

            footIK.OnValidate();

            Assert.AreEqual(footIK.stepHeight, footIK.LeftFootTarget.StrideHeight);
            Assert.AreEqual(footIK.stepHeight, footIK.RightFootTarget.StrideHeight);
            Assert.AreEqual(footIK.strideTime, footIK.LeftFootTarget.StrideTime);
            Assert.AreEqual(footIK.strideTime, footIK.RightFootTarget.StrideTime);
            Assert.AreEqual(footIK.placeBlendTime, footIK.LeftFootTarget.PlaceBlendTime);
            Assert.AreEqual(footIK.placeBlendTime, footIK.RightFootTarget.PlaceBlendTime);
            Assert.AreEqual(footIK.footGroundedHeight, footIK.LeftFootTarget.FootGroundedHeight);
            Assert.AreEqual(footIK.footGroundedHeight, footIK.RightFootTarget.FootGroundedHeight);

            footIK.stepHeight *= 2;
            footIK.strideTime *= 2;
            footIK.placeBlendTime *= 2;
            footIK.footGroundedHeight *= 2;
        }

        [Test]
        public void Validate_HumanoidFootIK_GetFootTargets()
        {
            Assert.AreEqual(footIK.LeftFootTarget, footIK.GetFootTarget(Foot.LeftFoot));
            Assert.AreEqual(footIK.RightFootTarget, footIK.GetFootTarget(Foot.RightFoot));
            Assert.IsNull(footIK.GetFootTarget(Foot.Unspecified));
        }

        [Test]
        public void Validate_HumanoidFootIK_LerpUpdates()
        {
            unityServiceMock.Setup(e => e.deltaTime).Returns(0.01f);

            footIK.placeBlendTime = 1.0f;
            footIK.OnValidate();

            // Ground the left foot
            footIK.LeftFootTarget.StartStride(Vector3.forward, Quaternion.identity, CreateGameObject(), Vector3.forward, Vector3.up, false);
            float previousLeftFootWeight = footIK.LeftFootTarget.FootIKWeight;
            footIK.Update();
            
            // Assert that the footIK will lerp as part of the update
            for (int i = 0; i < 10; i++)
            {
                footIK.Update();
                float leftFootWeight = footIK.LeftFootTarget.FootIKWeight;

                // Assert that the right foot is unaffected
                // and the left foot weight is increasing.
                Assert.AreEqual(0, footIK.RightFootTarget.FootIKWeight);
                Assert.IsTrue(leftFootWeight >= previousLeftFootWeight);
                previousLeftFootWeight = leftFootWeight;
            }
        }
    }
}  