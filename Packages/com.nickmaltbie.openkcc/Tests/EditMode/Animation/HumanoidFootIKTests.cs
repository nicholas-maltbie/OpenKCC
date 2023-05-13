
using System.Collections.Generic;
using System.Linq;
using Moq;
using nickmaltbie.OpenKCC.Animation;
using nickmaltbie.OpenKCC.Tests.TestCommon;
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
        /// <summary>
        /// Animation state for testing avatar.
        /// </summary>
        private const string AnimState = "AnimState";

        /// <summary>
        /// Set of bones in basic humanoid avatar.
        /// </summary>
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
        
        /// <summary>
        /// Validate that the <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK.UpdateFeetPositions(Vector3)"/>
        /// will properly translate foot targets to simulate player standing on moving
        /// ground.
        /// </summary>
        [Test]
        public void Validate_HumanoidFootIK_UpdateFootPlacement()
        {
            GameObject ground = CreateGameObject();
            ground.name = "ground";
            BoxCollider box = ground.AddComponent<BoxCollider>();
            GameObject ground2 = CreateGameObject();
            ground2.name = "ground2";
            BoxCollider box2 = ground2.AddComponent<BoxCollider>();

            // Ground one of the two feet
            footIK.LeftFootTarget.ReleaseFoot();
            footIK.RightFootTarget.StartStride(Vector3.forward, Quaternion.identity, ground, Vector3.forward, Vector3.up, false);
            Assert.AreEqual(FootState.Released, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Grounded, footIK.RightFootTarget.State);

            // Assert that the foot targets are expected values.
            Assert.AreEqual(Vector3.zero, footIK.LeftFootTarget.TargetFootPosition);
            Assert.AreEqual(Vector3.forward, footIK.RightFootTarget.TargetFootPosition);

            // Set the player as standing on something
            Mock<IRaycastHit> mockHit = new Mock<IRaycastHit>();
            mockHit.Setup(e => e.normal).Returns(Vector3.up);
            mockHit.Setup(e => e.collider).Returns(box);
            raycastHelperMock.Setup(e => e.DoRaycastInDirection(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), out It.Ref<IRaycastHit>.IsAny, It.IsAny<int>(), It.IsAny<QueryTriggerInteraction>()))
                .Callback(new KCCTestUtils.DoRaycastInDirectionCallback((Vector3 pos, Vector3 dir, float dist, out IRaycastHit hit, int layerMask, QueryTriggerInteraction queryTriggerInteraction) =>
                {
                    mockHit.Setup(e => e.point).Returns(pos);
                    hit = mockHit.Object;
                }))
                .Returns(true);

            // Move player feet one unit to the right
            footIK.UpdateFeetPositions(Vector3.right);

            // Verify that a check for the right foot was performed
            raycastHelperMock.Verify(
                e => e.DoRaycastInDirection(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), out It.Ref<IRaycastHit>.IsAny, It.IsAny<int>(), It.IsAny<QueryTriggerInteraction>()),
                Times.AtLeastOnce);

            // After grounding the foot, trigger the update foot placement
            // This update foot placement should only be applied to the grounded foot (right foot)
            Assert.AreEqual(Vector3.zero, footIK.LeftFootTarget.TargetFootPosition);
            Assert.AreEqual(Vector3.forward + Vector3.right, footIK.RightFootTarget.TargetFootPosition);
            Assert.AreEqual(FootState.Released, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Grounded, footIK.RightFootTarget.State);

            // Trigger another update, but have the player's moved foot be on a new ground
            mockHit.Setup(e => e.collider).Returns(box2);
            footIK.UpdateFeetPositions(Vector3.right);
            Assert.AreEqual(Vector3.zero, footIK.LeftFootTarget.TargetFootPosition);
            Assert.AreEqual(Vector3.forward + Vector3.right + Vector3.right, footIK.RightFootTarget.TargetFootPosition);
            Assert.AreEqual(FootState.Released, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Grounded, footIK.RightFootTarget.State);

            // Trigger another update, but have the player's moved foot be no longer supported
            raycastHelperMock.Setup(e => e.DoRaycastInDirection(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), out It.Ref<IRaycastHit>.IsAny, It.IsAny<int>(), It.IsAny<QueryTriggerInteraction>())).Returns(false);
            footIK.UpdateFeetPositions(Vector3.forward);
            Assert.AreEqual(FootState.Released, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Released, footIK.RightFootTarget.State);
        }

        /// <summary>
        /// Validate that the <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK.OnAnimatorIK(int)"/>
        /// will trigger updates for both feet. When triggering update, if one
        /// foot is grounded and the other is released. if the threshold value is exceeded,
        /// they should change states
        /// </summary>
        [Test]
        public void Validate_HumanoidFootIK_BasicUpdateChangeState()
        {
            // Set one foot as grounded, one as released
            footIK.LeftFootTarget.ReleaseFoot();
            footIK.RightFootTarget.StartStride(Vector3.forward, Quaternion.identity, CreateGameObject(), Vector3.forward, Vector3.up, false);
            Assert.AreEqual(FootState.Released, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Grounded, footIK.RightFootTarget.State);
            unityServiceMock.Setup(e => e.deltaTime).Returns(0.1f);

            // Lerp the weights until they are within expected values
            for (int i = 0; i < 100 && footIK.RightFootTarget.FootIKWeight <= 0.95f; i++)
            {
                UnityEngine.Debug.Log(footIK.RightFootTarget.FootIKWeight);
                footIK.RightFootTarget.LerpFootIKWeight();
            }
            UnityEngine.Debug.Log(footIK.RightFootTarget.FootIKWeight);
            Assert.IsTrue(footIK.RightFootTarget.FootIKWeight >= 0.95f);

            // Flip the foot thresholds
            animator.SetFloat(FootTarget.LeftFootIKWeight, 1.0f);
            animator.SetFloat(FootTarget.RightFootIKWeight, 0.0f);

            // Set the player as standing on something
            Mock<IRaycastHit> mockHit = new Mock<IRaycastHit>();
            mockHit.Setup(e => e.normal).Returns(Vector3.up);
            raycastHelperMock.Setup(e => e.DoRaycastInDirection(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), out It.Ref<IRaycastHit>.IsAny, It.IsAny<int>(), It.IsAny<QueryTriggerInteraction>()))
                .Callback(new KCCTestUtils.DoRaycastInDirectionCallback((Vector3 pos, Vector3 dir, float dist, out IRaycastHit hit, int layerMask, QueryTriggerInteraction queryTriggerInteraction) =>
                {
                    hit = mockHit.Object;
                }))
                .Returns(true);

            // Trigger the update feet method
            footIK.OnAnimatorIK(0);

            // Verify that a check for the feet were performed
            raycastHelperMock.Verify(
                e => e.DoRaycastInDirection(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), out It.Ref<IRaycastHit>.IsAny, It.IsAny<int>(), It.IsAny<QueryTriggerInteraction>()),
                Times.AtLeastOnce);

            // Feet should flip states
            Assert.AreEqual(FootState.Grounded, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Released, footIK.RightFootTarget.State);
        }

        /// <summary>
        /// Validate that the <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK.OnAnimatorIK(int)"/>
        /// will trigger updates for both feet. When triggering update, if
        /// a foot is grounded but desired position is too far away,
        /// foot should be released.
        /// </summary>
        [Test]
        public void Validate_HumanoidFootIK_BasicUpdateReleaseStepWhenGrounded()
        {
            // Set one foot as grounded, one as released
            footIK.LeftFootTarget.StartStride(Vector3.zero, Quaternion.identity, CreateGameObject(), Vector3.forward, Vector3.up, false);
            footIK.RightFootTarget.StartStride(Vector3.zero, Quaternion.identity, CreateGameObject(), Vector3.forward, Vector3.up, false);
            Assert.AreEqual(FootState.Grounded, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Grounded, footIK.RightFootTarget.State);
            unityServiceMock.Setup(e => e.deltaTime).Returns(0.1f);

            // Lerp the weights until they are within expected values
            for (int i = 0; i < 100 && (footIK.RightFootTarget.FootIKWeight <= 0.95f || footIK.LeftFootTarget.FootIKWeight <= 0.95f); i++)
            {
                footIK.RightFootTarget.LerpFootIKWeight();
                footIK.LeftFootTarget.LerpFootIKWeight();
            }
            Assert.IsTrue(footIK.RightFootTarget.FootIKWeight >= 0.95f);
            Assert.IsTrue(footIK.RightFootTarget.FootIKWeight >= 0.95f);

            // Set so both feet are no longer mid stride
            unityServiceMock.Setup(e => e.time).Returns(100.0f);
            Assert.IsFalse(footIK.RightFootTarget.MidStride);
            Assert.IsFalse(footIK.RightFootTarget.MidStride);

            // set booth feet as remain grounded
            animator.SetFloat(FootTarget.LeftFootIKWeight, 1.0f);
            animator.SetFloat(FootTarget.RightFootIKWeight, 1.0f);

            // Set the player as standing on something
            // For the left foot, have the foot be too far away and trigger
            // the foot to release
            // move hips forward, like a lot forward
            avatarBones[HumanBodyBones.Hips].transform.position += Vector3.forward * 100;
            Mock<IRaycastHit> mockHit = new Mock<IRaycastHit>();
            mockHit.Setup(e => e.normal).Returns(Vector3.up);
            raycastHelperMock.Setup(e => e.DoRaycastInDirection(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), out It.Ref<IRaycastHit>.IsAny, It.IsAny<int>(), It.IsAny<QueryTriggerInteraction>()))
                .Callback(new KCCTestUtils.DoRaycastInDirectionCallback((Vector3 pos, Vector3 dir, float dist, out IRaycastHit hit, int layerMask, QueryTriggerInteraction queryTriggerInteraction) =>
                {
                    mockHit.Setup(e => e.point).Returns(pos);
                    hit = mockHit.Object;
                }))
                .Returns(true);
            
            // when manually updating the feet, feet should be released because
            // it's way too far form original position
            footIK.OnAnimatorIK(0);
            Assert.AreEqual(FootState.Released, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Released, footIK.RightFootTarget.State);
        }

        /// <summary>
        /// Validate that the <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK.OnAnimatorIK(int)"/>
        /// will trigger updates for both feet. When triggering update, if
        /// a foot is grounded but desired rotation is beyond the current rotation.
        /// </summary>
        [Test]
        public void Validate_HumanoidFootIK_BasicUpdateBumpStepWhenGrounded()
        {
            // Set one foot as grounded, one as released
            footIK.LeftFootTarget.StartStride(Vector3.zero, Quaternion.identity, CreateGameObject(), Vector3.forward, Vector3.up, false);
            footIK.RightFootTarget.StartStride(Vector3.zero, Quaternion.identity, CreateGameObject(), Vector3.forward, Vector3.up, false);
            Assert.AreEqual(FootState.Grounded, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Grounded, footIK.RightFootTarget.State);
            unityServiceMock.Setup(e => e.deltaTime).Returns(0.1f);

            // Lerp the weights until they are within expected values
            for (int i = 0; i < 100 && (footIK.RightFootTarget.FootIKWeight <= 0.95f || footIK.LeftFootTarget.FootIKWeight <= 0.95f); i++)
            {
                footIK.RightFootTarget.LerpFootIKWeight();
                footIK.LeftFootTarget.LerpFootIKWeight();
            }
            Assert.IsTrue(footIK.RightFootTarget.FootIKWeight >= 0.95f);
            Assert.IsTrue(footIK.LeftFootTarget.FootIKWeight >= 0.95f);

            // Set so both feet are no longer mid stride
            unityServiceMock.Setup(e => e.time).Returns(100.0f);
            Assert.IsFalse(footIK.RightFootTarget.MidStride);
            Assert.IsFalse(footIK.LeftFootTarget.MidStride);

            // set booth feet as remain grounded
            animator.SetFloat(FootTarget.LeftFootIKWeight, 1.0f);
            animator.SetFloat(FootTarget.RightFootIKWeight, 1.0f);

            // Set the player as standing on something
            // move hips rotated around completely.
            avatarBones[HumanBodyBones.Hips].transform.rotation *= Quaternion.Euler(0, 180, 0);
            Mock<IRaycastHit> mockHit = new Mock<IRaycastHit>();
            mockHit.Setup(e => e.normal).Returns(Vector3.up);
            raycastHelperMock.Setup(e => e.DoRaycastInDirection(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), out It.Ref<IRaycastHit>.IsAny, It.IsAny<int>(), It.IsAny<QueryTriggerInteraction>()))
                .Callback(new KCCTestUtils.DoRaycastInDirectionCallback((Vector3 pos, Vector3 dir, float dist, out IRaycastHit hit, int layerMask, QueryTriggerInteraction queryTriggerInteraction) =>
                {
                    mockHit.Setup(e => e.point).Returns(pos);
                    hit = mockHit.Object;
                }))
                .Returns(true);
            
            // when manually updating the feet, feet should be updated to new position
            // and rotation
            footIK.OnAnimatorIK(0);

            // Only of of the feet should lift because we don't want to hop in place
            Assert.AreEqual(FootState.Grounded, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Grounded, footIK.RightFootTarget.State);
            Assert.IsTrue(footIK.LeftFootTarget.MidStride ^ footIK.RightFootTarget.MidStride);
            Assert.IsTrue(footIK.LeftFootTarget.UseBump ^ footIK.RightFootTarget.UseBump);
            Assert.IsFalse(footIK.CanTakeStride);
        }

        /// <summary>
        /// Validate that the <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK.OnAnimatorIK(int)"/>
        /// will trigger update the stride target until the foot is placed.
        /// </summary>
        [Test]
        public void Validate_HumanoidFootIK_UpdateTargetMidStride()
        {
            // Set one foot as grounded, one as released
            footIK.RightFootTarget.StartStride(Vector3.zero, Quaternion.identity, CreateGameObject(), Vector3.forward, Vector3.up, true);
            Assert.AreEqual(FootState.Released, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Grounded, footIK.RightFootTarget.State);
            unityServiceMock.Setup(e => e.deltaTime).Returns(0.05f);

            // Lerp the weights until they are within expected values
            for (int i = 0; i < 100 && footIK.RightFootTarget.FootIKWeight <= 0.05f; i++)
            {
                footIK.RightFootTarget.LerpFootIKWeight();
            }
            Assert.IsTrue(footIK.RightFootTarget.FootIKWeight >= 0.05f);

            // Right foot should still be mid stride
            Assert.IsTrue(footIK.RightFootTarget.MidStride);
            Assert.IsTrue(footIK.RightFootTarget.UseBump);

            // set foot as remain grounded
            animator.SetFloat(FootTarget.RightFootIKWeight, 1.0f);

            // Set the player as standing on something
            // move hips rotated around completely.
            avatarBones[HumanBodyBones.Hips].transform.position = Vector3.forward;
            Mock<IRaycastHit> mockHit = new Mock<IRaycastHit>();
            mockHit.Setup(e => e.normal).Returns(Vector3.up);
            raycastHelperMock.Setup(e => e.DoRaycastInDirection(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), out It.Ref<IRaycastHit>.IsAny, It.IsAny<int>(), It.IsAny<QueryTriggerInteraction>()))
                .Callback(new KCCTestUtils.DoRaycastInDirectionCallback((Vector3 pos, Vector3 dir, float dist, out IRaycastHit hit, int layerMask, QueryTriggerInteraction queryTriggerInteraction) =>
                {
                    mockHit.Setup(e => e.point).Returns(pos + Vector3.forward * 0.01f);
                    hit = mockHit.Object;
                }))
                .Returns(true);
            
            // when manually updating the feet, feet should be moved slightly forward
            footIK.OnAnimatorIK(0);
            footIK.OnAnimatorIK(0);
            footIK.OnAnimatorIK(0);
            Assert.IsTrue(Vector3.Dot(footIK.RightFootTarget.TargetFootPosition, Vector3.forward) > 0.05f);
            Assert.AreEqual(FootState.Grounded, footIK.RightFootTarget.State);
        }

        /// <summary>
        /// Validate that the <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK.OnAnimatorIK(int)"/>
        /// will trigger updates for both feet. When triggering update, if one
        /// foot is grounded and the other is released. As long as teh
        /// feet remain within their threshold, they should not have any
        /// change in state.
        /// 
        /// The right foot target should also attempt to check
        /// if it's still grounded calling the
        /// <see cref="nickmaltbie.OpenKCC.Utils.RaycastHelper.DoRaycastInDirection(Vector3, Vector3, float, out IRaycastHit, int, QueryTriggerInteraction)"/>
        /// at least once.
        /// </summary>
        [Test]
        public void Validate_HumanoidFootIK_BasicUpdateMaintainState()
        {
            // Set one foot as grounded, one as released
            footIK.LeftFootTarget.ReleaseFoot();
            footIK.RightFootTarget.StartStride(Vector3.forward, Quaternion.identity, CreateGameObject(), Vector3.forward, Vector3.up, false);
            Assert.AreEqual(FootState.Released, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Grounded, footIK.RightFootTarget.State);

            // Set the right foot as over grounded threshold and left foot as under
            animator.SetFloat(FootTarget.LeftFootIKWeight, 0.0f);
            animator.SetFloat(FootTarget.RightFootIKWeight, 1.0f);

            // Set the player as standing on something
            Mock<IRaycastHit> mockHit = new Mock<IRaycastHit>();
            mockHit.Setup(e => e.normal).Returns(Vector3.up);
            raycastHelperMock.Setup(e => e.DoRaycastInDirection(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), out It.Ref<IRaycastHit>.IsAny, It.IsAny<int>(), It.IsAny<QueryTriggerInteraction>()))
                .Callback(new KCCTestUtils.DoRaycastInDirectionCallback((Vector3 pos, Vector3 dir, float dist, out IRaycastHit hit, int layerMask, QueryTriggerInteraction queryTriggerInteraction) =>
                {
                    hit = mockHit.Object;
                }))
                .Returns(true);

            // Trigger the update feet method
            footIK.OnAnimatorIK(0);

            // Verify that a check for the right foot was performed
            raycastHelperMock.Verify(
                e => e.DoRaycastInDirection(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), out It.Ref<IRaycastHit>.IsAny, It.IsAny<int>(), It.IsAny<QueryTriggerInteraction>()),
                Times.AtLeastOnce);

            // Feet should remain in same state
            Assert.AreEqual(FootState.Released, footIK.LeftFootTarget.State);
            Assert.AreEqual(FootState.Grounded, footIK.RightFootTarget.State);
        }

        /// <summary>
        /// Validate the when the configuration values for the
        /// <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK"/>
        /// are updated and <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK.OnValidate"/>
        /// is performed (such as when a change is detected by the unity editor),
        /// that these new values are properly synchronized
        /// with the <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK.RightFootTarget"/> and
        /// <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK.LeftFootTarget"/>
        /// of the <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK"/>
        /// for each property modified.
        /// </summary>
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

        /// <summary>
        /// Validate that the <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK.GetFootTarget(Foot)"/>
        /// returns the expected foot target, either right or left (or null if improperly specified).
        /// </summary>
        [Test]
        public void Validate_HumanoidFootIK_GetFootTargets()
        {
            Assert.AreEqual(footIK.LeftFootTarget, footIK.GetFootTarget(Foot.LeftFoot));
            Assert.AreEqual(footIK.RightFootTarget, footIK.GetFootTarget(Foot.RightFoot));
            Assert.IsNull(footIK.GetFootTarget(Foot.Unspecified));
        }

        /// <summary>
        /// Tests to ensure that Validate that the <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK.GetTargetHipOffset"/>
        /// performs as expected.
        /// </summary>
        /// <param name="leftGrounded">Should left foot be grounded.</param>
        /// <param name="rightGrounded">Should right foot be grounded.</param>
        [Test]
        public void Validate_HumanoidFootIK_GetTargetHipOffset([Values] bool leftGrounded, [Values] bool rightGrounded)
        {
            GameObject floor = CreateGameObject();
            unityServiceMock.Setup(e => e.deltaTime).Returns(1.0f);

            // Setup each foot in expected grounded state.
            if (leftGrounded)
            {
                footIK.LeftFootTarget.StartStride(Vector3.down * 1.25f, Quaternion.identity, floor, Vector3.forward, Vector3.up, false);
                for (int i = 0; i < 10; i++)
                {
                    footIK.LeftFootTarget.LerpFootIKWeight();
                }
            }
            else
            {
                footIK.LeftFootTarget.ReleaseFoot();
            }
            
            if (rightGrounded)
            {
                footIK.RightFootTarget.StartStride(Vector3.down * 1.45f, Quaternion.identity, floor, Vector3.forward, Vector3.up, false);
                for (int i = 0; i < 10; i++)
                {
                    footIK.RightFootTarget.LerpFootIKWeight();
                }
            }
            else
            {
                footIK.RightFootTarget.ReleaseFoot();
            }

            unityServiceMock.Setup(e => e.time).Returns(100.0f);
            avatarBones[HumanBodyBones.RightFoot].transform.position = Vector3.zero;
            avatarBones[HumanBodyBones.LeftFoot].transform.position = Vector3.zero;

            // When getting hip offset, should be some large value if either left foot
            // or right foot is not grounded
            float offset = footIK.GetTargetHipOffset();

            if (!leftGrounded && !rightGrounded)
            {
                Assert.AreEqual(0, offset);
            }
            else if (leftGrounded && !rightGrounded)
            {
                TestUtils.AssertInBounds(-0.35f, offset);
            }
            else if (!leftGrounded && rightGrounded)
            {
                TestUtils.AssertInBounds(-0.55f, offset);
            }
            else if (leftGrounded && rightGrounded)
            {
                TestUtils.AssertInBounds(-0.55f, offset);
            }
        }

        /// <summary>
        /// Validate that the <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK.Update"/>
        /// properly invokes the <see cref="nickmaltbie.OpenKCC.Animation.FootTarget.LerpFootIKWeight"/>
        /// for the mocked delta time values.
        /// </summary>
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