
using System.Collections.Generic;
using System.Linq;
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
        private static readonly (HumanBodyBones, HumanBodyBones)[] RequiredBones = new []
        {
            (HumanBodyBones.Hips, HumanBodyBones.Hips),
            (HumanBodyBones.Spine, HumanBodyBones.Hips),
            (HumanBodyBones.Chest, HumanBodyBones.Spine),
            (HumanBodyBones.UpperChest, HumanBodyBones.Chest),
            (HumanBodyBones.Neck, HumanBodyBones.UpperChest),
            (HumanBodyBones.Head, HumanBodyBones.Neck),
            (HumanBodyBones.LeftUpperLeg, HumanBodyBones.Hips),
            (HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftUpperLeg),
            (HumanBodyBones.LeftFoot, HumanBodyBones.LeftLowerLeg),
            (HumanBodyBones.LeftToes, HumanBodyBones.LeftFoot),
            (HumanBodyBones.RightUpperLeg, HumanBodyBones.Hips),
            (HumanBodyBones.RightLowerLeg, HumanBodyBones.RightUpperLeg),
            (HumanBodyBones.RightFoot, HumanBodyBones.RightLowerLeg),
            (HumanBodyBones.RightToes, HumanBodyBones.RightFoot),
            (HumanBodyBones.LeftShoulder, HumanBodyBones.UpperChest),
            (HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftShoulder),
            (HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm),
            (HumanBodyBones.LeftHand, HumanBodyBones.LeftLowerArm),
            (HumanBodyBones.RightShoulder, HumanBodyBones.UpperChest),
            (HumanBodyBones.RightUpperArm, HumanBodyBones.RightShoulder),
            (HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm),
            (HumanBodyBones.RightHand, HumanBodyBones.RightLowerArm),
        };

        private const string AnimState = "AnimState";

        private Dictionary<HumanBodyBones, Transform> avatarBones;
        private Mock<IUnityService> unityServiceMock;
        private Mock<IRaycastHelper> raycastHelperMock;
        private HumanoidFootIK footIK;
        private Animator animator;

        [SetUp]
        public void SetUp()
        {
            // Create game object for the avatar
            GameObject go = CreateGameObject();
            go.name = "Avatar Base";
            animator = go.AddComponent<Animator>();
            footIK = go.AddComponent<HumanoidFootIK>();

            // Setup animation controller.
            var controller = new AnimatorController();
            controller.AddLayer("base");
            AnimatorStateMachine rootStateMachine = controller.layers[0].stateMachine;
            controller.AddParameter(FootTarget.LeftFootIKWeight, AnimatorControllerParameterType.Float);
            controller.AddParameter(FootTarget.RightFootIKWeight, AnimatorControllerParameterType.Float);
            rootStateMachine.AddState(AnimState);
            animator.runtimeAnimatorController = controller;

            // Attach mocks to the object.
            unityServiceMock = new Mock<IUnityService>();
            raycastHelperMock = new Mock<IRaycastHelper>();
            footIK.unityService = unityServiceMock.Object;
            footIK.raycastMock = raycastHelperMock.Object;
            footIK.LeftFootTarget.unityService = unityServiceMock.Object;
            footIK.RightFootTarget.unityService = unityServiceMock.Object;

            // setup the avatar for the player
            avatarBones = RequiredBones.Select(set => set.Item1).ToDictionary<HumanBodyBones, HumanBodyBones, Transform>(
                bone => bone,
                bone =>
                {
                    GameObject created = CreateGameObject();
                    created.name = bone.ToString();
                    created.transform.SetParent(go.transform);
                    return created.transform;
                });

            // Set the hierarchy of bones
            foreach (var set in RequiredBones)
            {
                if (set.Item1 != set.Item2)
                {
                    avatarBones[set.Item1].SetParent(avatarBones[set.Item2]);
                }
            }

            HumanDescription description = new HumanDescription()
            {
                human = avatarBones.Values.Select(bone => new HumanBone { boneName = bone.name, humanName = bone.name } ).ToArray(),
                skeleton = avatarBones.Values.Select(bone => new SkeletonBone { name = bone.name }).Append(new SkeletonBone { name = go.name }).ToArray(),
                upperArmTwist = 0.1f,
                lowerArmTwist = 0.1f,
                upperLegTwist = 0.1f,
                lowerLegTwist = 0.1f,
                armStretch = 0.1f,
                legStretch = 0.1f,
                feetSpacing = 0.1f,
                hasTranslationDoF = false,
            };

            Avatar avatar = AvatarBuilder.BuildHumanAvatar(go, description);
            animator.avatar = avatar;

            // Configure objects
            footIK.Awake();
            animator.StartPlayback();
            animator.Play(AnimState, 0);
        }

        [Test]
        public void Validate_HumanoidFootIK_PlaceFeets()
        {
            footIK.OnAnimatorIK(0);
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