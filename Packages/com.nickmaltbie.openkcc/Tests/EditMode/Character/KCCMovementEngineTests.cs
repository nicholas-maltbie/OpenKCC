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
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Tests.EditMode.Utils;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.OpenKCC.Utils.ColliderCast;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character
{
    public class VerifyTeleport : MonoBehaviour, IOnPlayerTeleport
    {
        public Vector3 teleportPos = Vector3.zero;

        public void OnPlayerTeleport(Vector3 destPos, Quaternion destRot)
        {
            teleportPos = destPos;
        }
    }

    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.Character.KCCMovementEngine"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class KCCMovementEngineTests : TestBase
    {
        private MockColliderCast colliderCastMock;
        private KCCMovementEngine engine;

        [SetUp]
        public override void Setup()
        {
            GameObject go = CreateGameObject();
            colliderCastMock = go.AddComponent<MockColliderCast>();
            engine = go.AddComponent<KCCMovementEngine>();
            engine._colliderCast = colliderCastMock;

            engine.Awake();
        }

        [Test]
        public void Verify_KCCMovementEngine_SnapDownOverrideNormal()
        {
            // Setup the normal to originally be Vector3.up
            KCCTestUtils.SetupCastSelf(colliderCastMock, default, default, Vector3.up, KCCUtils.Epsilon, true);
            KCCGroundedState groundedState = engine.CheckGrounded(false, true);
            Assert.AreEqual(Vector3.up, groundedState.SurfaceNormal);

            // Setup the step hit to return a different normal
            KCCTestUtils.SetupCastSelf(colliderCastMock, default, default, Vector3.forward, KCCUtils.Epsilon, true);
            groundedState = engine.CheckGrounded(false, true);
            Assert.AreEqual(Vector3.forward, groundedState.SurfaceNormal);
        }

        [Test]
        public void Verify_KCCMovementEngine_OnPlayerTeleport([NUnit.Framework.Range(0, 100, 10)] float dist)
        {
            VerifyTeleport verify = engine.gameObject.AddComponent<VerifyTeleport>();
            engine.TeleportPlayer(Vector3.forward * dist);
            Assert.AreEqual(verify.teleportPos, Vector3.forward * dist);
        }

        [Test]
        public void Verify_KCCMovementEngine_Properties()
        {
            Assert.AreEqual(Vector3.up, engine.Up);
            Assert.AreEqual(colliderCastMock, engine.ColliderCast);
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

            KCCTestUtils.SetupCastSelf(colliderCastMock, box, Vector3.zero, Vector3.up, KCCUtils.Epsilon, true);
            engine.CheckGrounded(false, false);
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
            Vector3 initialNormal = Vector3.up;
            Vector3 newNormal = Vector3.down;
            KCCTestUtils.SetupCastSelf(colliderCastMock, collider, Vector3.zero, initialNormal, KCCUtils.Epsilon, true);

            engine.CheckGrounded(false, false);
            Assert.AreEqual(initialNormal, engine.GroundedState.SurfaceNormal);

            KCCTestUtils.SetupCastSelf(colliderCastMock, collider, Vector3.zero, newNormal, KCCUtils.Epsilon, true);
            engine.CheckGrounded(snapped, snapped);

            if (snapped)
            {
                Assert.AreEqual(initialNormal, engine.GroundedState.SurfaceNormal);
            }
            else
            {
                Assert.AreEqual(newNormal, engine.GroundedState.SurfaceNormal);
            }
        }

        /// <summary>
        /// Basic test to verify that the KCCMovementEngine won't
        /// move into the ground when moving into the ground
        /// with the relative parent config position.
        /// </summary>
        [Test]
        public void Verify_KCCMovementEngine_NoMoveIntoGround()
        {
            // For this test pretend the player is a sphere
            // with a radius of 0.5 units.
            GameObject ground = CreateGameObject();
            BoxCollider box = ground.AddComponent<BoxCollider>();

            // Setup positions of ground and player.
            ground.transform.position = new Vector3(0, -0.5f, 0);
            engine.transform.position = new Vector3(0, 1.5f, 0) + Vector3.up * KCCUtils.Epsilon;

            // There should be three calls to the CastSelf
            // First call is for computing movement, simply return no hit
            // Second call is for snapping to ground, should also return no hit
            //   because I don't want to also test that code.
            // Third call is for CheckGrounded, this is the important one.
            // To make this similar to the real case, we need to return that the player
            //   is just floating a little bit off the ground (0.001 units) and we should
            //   be fine.
            // We should compute the relative position of the player to be
            // 0.5 units above the center of the box.
            IRaycastHit noHit = KCCTestUtils.SetupRaycastHitMock(default(Collider), Vector3.zero, Vector3.zero, Mathf.Infinity);
            IRaycastHit groundCheckHit = KCCTestUtils.SetupRaycastHitMock(box, Vector3.zero, Vector3.up, KCCUtils.Epsilon);
            int hitIdx = 0;

            colliderCastMock.OnCastSelf = (Vector3 position, Quaternion rotation, Vector3 direction, float distance, out IRaycastHit hit, int layerMask, QueryTriggerInteraction queryTriggerInteraction) =>
            {
                // Only return hit past second cast
                if (++hitIdx < 2)
                {
                    hit = noHit;
                    return false;
                }

                hit = groundCheckHit;
                return true;
            };

            // Setup a basic collision between the player and the ground
            // one meter below the player.
            engine.MovePlayer(Vector3.down);

            // Now assert that the absolute position of the player
            // is about (0, 0.5, 0)
            TestUtils.AssertInBounds(engine.transform.position, Vector3.up * 0.5f, 2 * KCCUtils.Epsilon);

            // After calling Update this should be true as well.
            engine.Update();
            TestUtils.AssertInBounds(engine.transform.position, Vector3.up * 0.5f, 2 * KCCUtils.Epsilon);
        }

        [Test]
        public void SerializationValidationTests()
        {
            GameObject go = CreateGameObject();
            go.AddComponent<MockColliderCast>();
            KCCMovementEngine movementEngine = go.AddComponent<KCCMovementEngine>();
            movementEngine.serializationVersion = "";
            movementEngine.layerMask = 0;

            movementEngine.OnBeforeSerialize();
            movementEngine.OnAfterDeserialize();

            Assert.AreEqual(movementEngine.serializationVersion, KCCMovementEngine.CurrentSerializationVersion);
            Assert.AreEqual(movementEngine.layerMask.value, IColliderCast.DefaultLayerMask);
        }
    }
}
