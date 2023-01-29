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
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Tests.EditMode.Utils;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.Character.KCCMovementEngine"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class KCCMovementEngineTests : TestBase
    {
        private Mock<IColliderCast> colliderCastMock;
        private KCCMovementEngine engine;

        [SetUp]
        public override void Setup()
        {
            engine = CreateGameObject().AddComponent<KCCMovementEngine>();
            colliderCastMock = new Mock<IColliderCast>();
            engine._colliderCast = colliderCastMock.Object;
        }

        [Test]
        public void Verify_KCCMovementEngine_Properties()
        {
            Assert.AreEqual(Vector3.up, engine.Up);
            Assert.AreEqual(colliderCastMock.Object, engine.ColliderCast);
            Assert.AreEqual(engine.GroundedDistance, KCCMovementEngine.DefaultGroundedDistance);
            Assert.AreEqual(engine.GroundCheckDistance, KCCMovementEngine.DefaultGroundCheckDistance);
            Assert.AreEqual(engine.MaxWalkAngle, engine.maxWalkAngle);
            Assert.AreEqual(engine.MaxBounces, KCCMovementEngine.DefaultMaxBounces);
            Assert.AreEqual(engine.VerticalSnapUp, engine.stepHeight);
            Assert.AreEqual(engine.StepUpDepth, KCCMovementEngine.DefaultStepUpDepth);
            Assert.AreEqual(engine.AnglePower, KCCMovementEngine.DefaultAnglePower);
            Assert.AreEqual(engine.MaxPushSpeed, KCCMovementEngine.DefaultMaxPushSpeed);
            Assert.AreEqual(engine.CanSnapUp, engine.GroundedState.OnGround);
            Assert.AreEqual(engine.SnapDown, engine.stepHeight * KCCMovementEngine.SnapDownModifier);
            Assert.AreEqual(engine.MaxDefaultLaunchVelocity, KCCMovementEngine.DefaultMaxLaunchVelocity);
        }

        [Test]
        public void Validate_KCCMovementEngine_KCCGetGroundVelocity(
            [Values] bool movingGround,
            [Values] bool avoidTransferMomentum,
            [Values] bool rigidbody,
            [Values] bool isKinematic,
            [Values] bool onGround,
            [Values] bool loadVelocity)
        {
            GameObject floor = CreateGameObject();
            BoxCollider box = floor.AddComponent<BoxCollider>();
            MovingGroundComponent ground = null;
            Rigidbody rb = null;

            Vector3 loadedVelocity = Vector3.left;

            // Load values of smoothed vector to simulate movement forward
            if (loadVelocity)
            {
                engine.worldVelocity.AddSample(loadedVelocity);
            }

            if (movingGround)
            {
                ground = floor.AddComponent<MovingGroundComponent>();
                ground.avoidTransferMomentum = avoidTransferMomentum;
            }

            if (rigidbody)
            {
                rb = floor.AddComponent<Rigidbody>();
                rb.isKinematic = isKinematic;
            }

            var raycastHitMock = new Mock<IRaycastHit>();
            raycastHitMock.Setup(hit => hit.collider).Returns(box);
            raycastHitMock.Setup(hit => hit.point).Returns(Vector3.zero);
            raycastHitMock.Setup(hit => hit.distance).Returns(KCCUtils.Epsilon);
            raycastHitMock.Setup(hit => hit.normal).Returns(Vector3.up);

            colliderCastMock.Setup(e => e.CastSelf(
                It.IsAny<Vector3>(),
                It.IsAny<Quaternion>(),
                It.IsAny<Vector3>(),
                It.IsAny<float>(),
                out It.Ref<IRaycastHit>.IsAny))
                .Callback(new KCCTestUtils.CastSelfCallback((Vector3 pos, Quaternion rot, Vector3 dir, float dist, out IRaycastHit hit) =>
                {
                    hit = raycastHitMock.Object;
                }))
                .Returns(true);

            engine.CheckGrounded(false);

            Vector3 velocity = engine.GetGroundVelocity();

            if (movingGround)
            {
                if (avoidTransferMomentum)
                {
                    Assert.AreEqual(Vector3.zero, velocity);
                }
                else
                {
                    Assert.AreEqual(ground.GetVelocityAtPoint(Vector3.zero), velocity);
                }
            }
            else if (rigidbody && !isKinematic)
            {
                Assert.AreEqual(rb.GetPointVelocity(Vector3.zero), velocity);
            }
            else if (onGround)
            {
                if (loadVelocity)
                {
                    Assert.AreEqual(loadedVelocity, velocity);
                }
                else
                {
                    Assert.AreEqual(Vector3.zero, velocity);
                }
            }
            else
            {
                if (loadVelocity)
                {
                    Assert.AreEqual(loadedVelocity, velocity);
                }
                else
                {
                    Assert.AreEqual(Vector3.zero, velocity);
                }
            }
        }

        [Test]
        public void Verify_KCCMovementEngine_GroundedWithSnap([Values] bool snapped)
        {
            GameObject box = CreateGameObject();
            BoxCollider collider = box.AddComponent<BoxCollider>();
            Vector3 startingNormal = Vector3.up;
            Vector3 newNormal = Vector3.down;
            Vector3 currentNormal = startingNormal;

            var raycastHitMock = new Mock<IRaycastHit>();
            raycastHitMock.Setup(hit => hit.collider).Returns(collider);
            raycastHitMock.Setup(hit => hit.point).Returns(Vector3.zero);
            raycastHitMock.Setup(hit => hit.distance).Returns(KCCUtils.Epsilon);
            raycastHitMock.Setup(hit => hit.normal).Returns(() => currentNormal);

            colliderCastMock.Setup(e => e.CastSelf(
                It.IsAny<Vector3>(),
                It.IsAny<Quaternion>(),
                It.IsAny<Vector3>(),
                It.IsAny<float>(),
                out It.Ref<IRaycastHit>.IsAny))
                .Callback(new KCCTestUtils.CastSelfCallback((Vector3 pos, Quaternion rot, Vector3 dir, float dist, out IRaycastHit hit) =>
                {
                    hit = raycastHitMock.Object;
                }))
                .Returns(true);

            engine.CheckGrounded(false);

            Assert.AreEqual(currentNormal, engine.GroundedState.SurfaceNormal);
            currentNormal = newNormal;

            engine.CheckGrounded(snapped);

            if (snapped)
            {
                Assert.AreEqual(startingNormal, engine.GroundedState.SurfaceNormal);
            }
            else
            {
                Assert.AreEqual(newNormal, engine.GroundedState.SurfaceNormal);
            }
        }
    }
}
