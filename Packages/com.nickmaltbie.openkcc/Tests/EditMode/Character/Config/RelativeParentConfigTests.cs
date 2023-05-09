
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character.Config
{
    [TestFixture]
    public class RelativeParentConfigTests : TestBase
    {
        private RelativeParentConfig parent;

        [SetUp]
        public void SetUp()
        {
            parent = new RelativeParentConfig();
        }

        [Test]
        public void Validate_RelativeParentConfig_NoDelta()
        {
            Assert.AreEqual(parent.DeltaPosition(Vector3.zero), Vector3.zero);
            Assert.AreEqual(parent.DeltaRotation(Quaternion.identity), Quaternion.identity);
        }

        [Test]
        public void Validate_RelativeParentConfig_AttachDetach()
        {
            Transform ground = CreateGameObject().transform;
            Assert.IsFalse(parent.OnMovingGround);
            Assert.AreEqual(null, parent.PreviousParent);

            parent.UpdateMovingGround(
                Vector3.zero,
                Quaternion.identity,
                new KCCGroundedState(KCCUtils.Epsilon, true, 0, Vector3.up, Vector3.zero, ground.gameObject),
                Vector3.zero,
                0.0f);

            Assert.IsTrue(parent.OnMovingGround);
            Assert.AreEqual(ground, parent.PreviousParent);

            parent.UpdateMovingGround(
                Vector3.zero,
                Quaternion.identity,
                new KCCGroundedState(KCCUtils.Epsilon, true, 0, Vector3.up, Vector3.zero, ground.gameObject),
                Vector3.zero,
                0.0f);

            Assert.IsTrue(parent.OnMovingGround);
            Assert.AreEqual(ground, parent.PreviousParent);

            parent.UpdateMovingGround(
                Vector3.zero,
                Quaternion.identity,
                new KCCGroundedState(Mathf.Infinity, false, 0, Vector3.up, Vector3.zero, null),
                Vector3.zero,
                0.0f);

            Assert.IsFalse(parent.OnMovingGround);
            Assert.AreEqual(null, parent.PreviousParent);
        }

        [Test]
        public void Validate_RelativeParentConfig_MovingGround()
        {
            GameObject go = CreateGameObject();
            MovementTracking tracking = go.AddComponent<MovementTracking>();
            tracking.shouldAttach = false;
            Transform ground = go.transform;

            Assert.IsFalse(parent.OnMovingGround);
            Assert.AreEqual(null, parent.PreviousParent);

            parent.UpdateMovingGround(
                Vector3.zero,
                Quaternion.identity,
                new KCCGroundedState(KCCUtils.Epsilon, true, 0, Vector3.up, Vector3.zero, ground.gameObject),
                Vector3.zero,
                0.0f);

            Assert.IsFalse(parent.OnMovingGround);
            Assert.AreEqual(null, parent.PreviousParent);
        }

        [Test]
        public void Validate_RelativeParentConfig_DistanceDelta()
        {
            // Attach to ground with relative position of zero
            Transform player = CreateGameObject().transform;
            Transform ground = CreateGameObject().transform;
            parent.UpdateMovingGround(
                Vector3.zero,
                Quaternion.identity,
                new KCCGroundedState(KCCUtils.Epsilon, true, 0, Vector3.up, Vector3.zero, ground.gameObject),
                Vector3.zero,
                0.0f);
            
            // Delta should initially be zero
            Assert.AreEqual(Vector3.zero, parent.DeltaPosition(Vector3.zero));
            Assert.AreEqual(Quaternion.identity, parent.DeltaRotation(Quaternion.identity));

            // Move the ground forward one unit
            ground.position += Vector3.forward;

            // Relative position should move forward one unit
            Assert.AreEqual(Vector3.forward, parent.DeltaPosition(Vector3.zero));
            Assert.AreEqual(Quaternion.identity, parent.DeltaRotation(Quaternion.identity));

            // Moving the player to follow ground should reset this
            parent.FollowGround(player);
            Assert.AreEqual(Vector3.forward, player.transform.position);
            Assert.AreEqual(Quaternion.identity, player.transform.rotation);
            
            // Delta from this new position should be zero
            Assert.AreEqual(Vector3.zero, parent.DeltaPosition(player.position));
            Assert.AreEqual(Quaternion.identity, parent.DeltaRotation(player.rotation));
        }
    }
}