
using nickmaltbie.OpenKCC.Animation;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character.Config
{
    [TestFixture]
    public class RelativeParentConfigWithFeetTests : TestBase
    {
        private RelativeParentConfigWithFeet parent;
        private HumanoidFootIK footIK;
        private Transform player;

        [SetUp]
        public void SetUp()
        {
            GameObject go = CreateGameObject();
            player = go.transform;
            footIK = go.AddComponent<HumanoidFootIK>();
            parent = new RelativeParentConfigWithFeet();
        }

        [Test]
        public void Validate_RelativeParentConfigWithFeet_NoMovingGround()
        {
            GameObject.DestroyImmediate(footIK);
            parent.FollowGround(player);
        }

        [Test]
        public void Validate_RelativeParentConfigWithFeet_WithFeet()
        {
            // Attach to ground with relative position of zero
            Transform player = CreateGameObject().transform;
            Transform ground = CreateGameObject().transform;

            // Set both feet as grounded for test
            footIK.leftFootTarget.StartStride(Vector3.zero, Quaternion.identity, ground.gameObject, Vector3.forward, false);
            footIK.rightFootTarget.StartStride(Vector3.zero, Quaternion.identity, ground.gameObject, Vector3.forward, false);

            parent.UpdateMovingGround(
                Vector3.zero,
                Quaternion.identity,
                new KCCGroundedState(KCCUtils.Epsilon, true, 0, Vector3.up, Vector3.zero, ground.gameObject),
                Vector3.zero,
                0.0f);

            // Delta should initially be zero
            Assert.AreEqual(Vector3.zero, parent.DeltaPosition(player.position));
            Assert.AreEqual(Quaternion.identity, parent.DeltaRotation(player.rotation));
            parent.FollowGround(player);
            Assert.AreEqual(Vector3.zero, footIK.leftFootTarget.TargetFootPosition);
            Assert.AreEqual(Vector3.zero, footIK.rightFootTarget.TargetFootPosition);

            // Move the ground forward one unit
            ground.position += Vector3.forward;

            // Relative position should move forward one unit
            Assert.AreEqual(Vector3.forward, parent.DeltaPosition(player.position));
            Assert.AreEqual(Quaternion.identity, parent.DeltaRotation(player.rotation));
            parent.FollowGround(player);

            // Relative position should restore to zero
            Assert.AreEqual(Vector3.zero, parent.DeltaPosition(player.position));
            Assert.AreEqual(Quaternion.identity, parent.DeltaRotation(player.rotation));
            parent.FollowGround(player);
        }
    }
}