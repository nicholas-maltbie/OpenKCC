// Copyright (C) 2022 Nicholas Maltbie
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

using System.Collections.Generic;
using System.Linq;
using Moq;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Utils
{
    public class MovingGroundComponent : MonoBehaviour, IMovingGround
    {
        public bool avoidTransferMomentum = false;
        public bool shouldAttach = true;
        public float movementWeight = 1.0f;
        public float transferMomentumWeight = 1.0f;
        public Vector3 velocity;

        public bool AvoidTransferMomentum()
        {
            return avoidTransferMomentum;
        }

        public float GetMovementWeight(Vector3 point, Vector3 playerVelocity)
        {
            return movementWeight;
        }

        public float GetTransferMomentumWeight(Vector3 point, Vector3 playerVelocity)
        {
            return transferMomentumWeight;
        }

        public Vector3 GetVelocityAtPoint(Vector3 point)
        {
            return velocity;
        }

        public bool ShouldAttach()
        {
            return shouldAttach;
        }
    }

    /// <summary>
    /// Basic tests for KCCUtils in edit mode.
    /// </summary>
    [TestFixture]
    public class KCCUtilsTests : TestBase
    {
        /// <summary>
        /// Collider cast mock used for testing.
        /// </summary>
        public Mock<IColliderCast> colliderCastMock;

        /// <summary>
        /// Mock for controlling character pushes used for testing.
        /// </summary>
        public Mock<ICharacterPush> characterPushMock;

        /// <summary>
        /// Setup test with basic collider cast mock.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            colliderCastMock = new Mock<IColliderCast>();
            characterPushMock = new Mock<ICharacterPush>();
        }

        /// <summary>
        /// Test maximum bounces by having the object hit something multiple times.
        /// </summary>
        [Test]
        public void Validate_KCCMaxBouncesTest([Values(0, 1, 5, 10)] int maxBounces)
        {
            // Have the object hit some collider and not move at all
            Vector3 initialPosition = Vector3.zero;
            Vector3 movement = Vector3.forward * 10;

            // Have collider return hitting something... but it shouldn't be called due to no movement
            SetupColliderCast(true, KCCTestUtils.SetupRaycastHitMock(distance: 0.1f, normal: (Vector3.right + Vector3.back).normalized));

            // Simulate bounces
            var bounces = GetBounces(initialPosition, movement, maxBounces: maxBounces, anglePower: 0.0f).ToList();

            // Should hit max bounces
            Debug.Log(string.Join("\n", bounces));
            Assert.IsTrue(bounces.Count == maxBounces + 2, $"Expected to find {maxBounces + 2} bounce but instead found {bounces.Count}");
            Enumerable.Range(0, maxBounces + 1).ToList().ForEach(idx => KCCValidation.ValidateKCCBounce(bounces[idx], KCCUtils.MovementAction.Bounce));
            KCCValidation.ValidateKCCBounce(bounces[maxBounces + 1], KCCUtils.MovementAction.Stop);
        }

        /// <summary>
        /// Test maximum bounces by having the object overlap with another.
        /// </summary>
        [Test]
        public void Validate_KCCOverlap()
        {
            // Have the object hit some collider and not move at all
            Vector3 initialPosition = Vector3.zero;
            Vector3 movement = Vector3.forward;

            // Have collider return hitting something... but it shouldn't be called due to no movement
            SetupColliderCast(true, KCCTestUtils.SetupRaycastHitMock(distance: 0));

            // Simulate bounces
            var bounces = GetBounces(initialPosition, movement, anglePower: 0.0f).ToList();

            // Should just return just one element with action stop
            NUnit.Framework.Assert.IsTrue(bounces.Count == 2, $"Expected to find {2} bounce but instead found {bounces.Count}");
            KCCValidation.ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Invalid, finalPosition: initialPosition, remainingMomentum: Vector3.zero);
            KCCValidation.ValidateKCCBounce(bounces[1], KCCUtils.MovementAction.Stop, finalPosition: initialPosition, remainingMomentum: Vector3.zero);
        }

        /// <summary>
        /// Test no movement from KCC.
        /// </summary>
        [Test]
        public void Validate_KCCNoMovement()
        {
            // Have collider return hitting something... but it shouldn't be called due to no movement
            SetupColliderCast(false, KCCTestUtils.SetupRaycastHitMock());

            // Simulate bounces
            var bounces = GetBounces(Vector3.zero, Vector3.zero).ToList();

            // Should just return just one element with action stop
            Assert.IsTrue(bounces.Count == 1, $"Expected to find {1} bounce but instead found {bounces.Count}");

            // Validate bounce properties
            KCCValidation.ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Stop, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
        }

        /// <summary>
        /// Validate the snap up behavior of the KCC second method
        /// </summary>
        [Test]
        public void Validate_KCCSecondSnapUpAction()
        {
            SetupColliderCast(new[]
            {
                // First hit should be simulating hitting a step slightly above foot position
                (true, KCCTestUtils.SetupRaycastHitMock(distance: KCCUtils.Epsilon, point: Vector3.up * 0.05f)),
                // Second hit should be simulating hitting a step and having to stop (hitting middle of step)
                (true, KCCTestUtils.SetupRaycastHitMock(distance: KCCUtils.Epsilon, point: Vector3.up * 0.05f)),
                // Next hit should not collide with anything as we are above the step
                (false, KCCTestUtils.SetupRaycastHitMock(distance: 0.2f, point: Vector3.up * 0.1f)),
            });

            // Have the snap up simulate hitting a step that is slightly above feet and has a normal perpendicular to up
            IRaycastHit wallCollision = KCCTestUtils.SetupRaycastHitMock(normal: Vector3.back, distance: float.Epsilon);
            colliderCastMock.Setup(mock => mock.DoRaycastInDirection(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), out wallCollision)).Returns(true);
            colliderCastMock.Setup(mock => mock.GetBottom(It.IsAny<Vector3>(), It.IsAny<Quaternion>())).Returns(Vector3.zero);

            // Simulate bounces
            var bounces = GetBounces(Vector3.zero, Vector3.forward, stepUpDepth: KCCUtils.Epsilon).ToList();

            // Validate bounce properties
            Assert.IsTrue(bounces.Count >= 2, $"Expected to find at least {2} bounce but instead found {bounces.Count}");
            KCCValidation.ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.SnapUp);

            // Assert that some forward momentum remained after hitting the step
            Assert.IsTrue(
                bounces[0].remainingMomentum.magnitude > KCCUtils.Epsilon,
                $"Expected remaining momentum to be grater than zero, but instead found {bounces[0].remainingMomentum.magnitude}.");
        }

        /// <summary>
        /// Validate the snap up behavior of the KCC Bounce method
        /// </summary>
        [Test]
        public void Validate_KCCSnapUpAction([NUnit.Framework.Range(0.1f, 2f, 0.1f)] float snapUpDistance)
        {
            SetupColliderCast(new[]
            {
                // First hit should be simulating hitting a step slightly above foot position
                (true, KCCTestUtils.SetupRaycastHitMock(distance: 0.1f, point: Vector3.up * snapUpDistance / 2, normal: Vector3.back)),
                // Next hit should not collide with anything as we are above the step
                (false, KCCTestUtils.SetupRaycastHitMock()),
            });

            // Have the snap up simulate hitting a step that is slightly above feet and has a normal perpendicular to up
            IRaycastHit wallCollision = KCCTestUtils.SetupRaycastHitMock(normal: Vector3.back, distance: float.Epsilon);
            colliderCastMock.Setup(mock => mock.DoRaycastInDirection(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<float>(), out wallCollision)).Returns(true);
            colliderCastMock.Setup(mock => mock.GetBottom(It.IsAny<Vector3>(), It.IsAny<Quaternion>())).Returns(Vector3.zero);

            // Simulate bounces
            var bounces = GetBounces(Vector3.zero, Vector3.forward, verticalSnapUp: snapUpDistance).ToList();

            // Validate bounce properties
            Assert.IsTrue(bounces.Count == 3, $"Expected to find {3} bounce but instead found {bounces.Count}");
            KCCValidation.ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.SnapUp);
            KCCValidation.ValidateKCCBounce(bounces[1], KCCUtils.MovementAction.Move);
            KCCValidation.ValidateKCCBounce(bounces[2], KCCUtils.MovementAction.Stop);
        }

        /// <summary>
        /// Test kcc hit nothing.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(TestDirections))]
        public void Validate_KCCMoveUnblocked(Vector3 movement)
        {
            Vector3 initialPosition = Vector3.zero;
            Vector3 expectedFinalPosition = initialPosition + movement;

            // Have collider return hitting something... but it shouldn't be called due to no movement
            SetupColliderCast(false, new Mock<IRaycastHit>().Object);

            // Simulate bounces
            var bounces = GetBounces(initialPosition, movement).ToList();

            // Validate bounce properties
            if (movement.magnitude > 0)
            {
                Assert.IsTrue(bounces.Count == 2, $"Expected to find {2} bounce but instead found {bounces.Count}");
                KCCValidation.ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Move, expectedFinalPosition, initialPosition, Vector3.zero, movement);
            }
            else
            {
                Assert.IsTrue(bounces.Count == 1, $"Expected to find {1} bounce but instead found {bounces.Count}");
            }

            KCCValidation.ValidateKCCBounce(bounces.Last(), KCCUtils.MovementAction.Stop, expectedFinalPosition, expectedFinalPosition, Vector3.zero, Vector3.zero);

        }

        /// <summary>
        /// Verify that player will snap down as expected.
        /// </summary>
        [Test]
        public void Verify_KCCSnapPlayerDown([Values(0.0f, 0.1f, 0.2f)] float minThreshold)
        {
            SetupColliderCast(true, KCCTestUtils.SetupRaycastHitMock(null, Vector3.zero, Vector3.up, 0.01f + minThreshold));

            Vector3 displacement = KCCUtils.SnapPlayerDown(Vector3.zero, Quaternion.identity, Vector3.down, 0.1f, minThreshold, colliderCastMock.Object);

            Assert.IsTrue(displacement.magnitude > 0.0f, $"Expected displacement to have a magnitude grater than zero but instead found {displacement.ToString("F3")}");
        }

        /// <summary>
        /// Verify that invalid projected momentum will retain its original magnitude
        /// </summary>
        /// <param name="move">Input player movement for invalid movement value.</param>
        [Test]
        [TestCaseSource(nameof(TestDirections))]
        public void Verify_KCCInvalidProjectedMomentum(Vector3 move)
        {
            Vector3 projected = KCCUtils.GetProjectedMomentumSafe(move, Vector3.forward, Vector3.up);
            Assert.IsTrue((move.magnitude - projected.magnitude) <= 0.001f, $"Expected projected vector to have magnitude of {move.magnitude} but instead found {projected.magnitude}");
        }

        /// <summary>
        /// Validate player won't bounce backwards when they hit a wall
        /// and could slide backwards of original direction.
        /// </summary>
        /// <param name="distance">Distance the player should move forward.</param>
        [Test]
        public void Verify_KCCNoJitterBackwards([Values(5, 10)] float distance)
        {
            // Have first hit hit a pushable object
            SetupColliderCast(new[]
            {
                // First hit should be simulating hitting an object and sliding to the left
                (true, KCCTestUtils.SetupRaycastHitMock(
                    distance: distance / 5,
                    normal: (Vector3.back * 2 + Vector3.left).normalized)),
                // Next hit should simulate hitting another wall and sliding back
                (true, KCCTestUtils.SetupRaycastHitMock(
                    distance: distance / 5,
                    normal: (Vector3.back * 2 + Vector3.right).normalized)),
            });

            // Simulate bounces
            var bounces = GetBounces(Vector3.zero, Vector3.forward * distance, anglePower: 1, canSnapUp: false).ToList();

            Debug.Log(string.Join("\n", bounces));

            // Validate bounce properties, should bounce once then stop before
            // moving backwards.
            Assert.IsTrue(bounces.Count == 3, $"Expected to find {3} bounce but instead found {bounces.Count}");
            ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Bounce);
            ValidateKCCBounce(bounces[1], KCCUtils.MovementAction.Bounce);
            ValidateKCCBounce(bounces[2], KCCUtils.MovementAction.Stop);
            foreach (KCCBounce bounce in bounces.AsEnumerable().Reverse().Skip(1))
            {
                Assert.IsTrue(
                    Vector3.Dot(bounce.Movement, Vector3.forward) >= 0,
                    $"Expected player to move forward but instead found movement {bounce.Movement.ToString("F3")}");
            }
        }

        /// <summary>
        /// Test the KCC Pushing a kinematic or no dynamic object.
        /// </summary>
        [Test]
        public void Validate_KCCPush()
        {
            // Have first hit hit a pushable object
            SetupColliderCast(new[]
            {
                // First hit should be simulating hitting a a pushable object
                (true, KCCTestUtils.SetupRaycastHitMock(
                    collider: null,
                    distance: 0.1f,
                    normal: (Vector3.back + Vector3.left).normalized)),
                // Next hit should not collide with anything as we are above the step
                (false, KCCTestUtils.SetupRaycastHitMock()),
            });
            characterPushMock.Setup(mock => mock.CanPushObject(It.IsAny<Collider>())).Returns(true);
            characterPushMock.Setup(mock => mock.PushObject(It.IsAny<IControllerColliderHit>())).Callback(() => { });

            // Simulate bounces
            var bounces = GetBounces(Vector3.zero, Vector3.forward).ToList();

            // Validate bounce properties
            Assert.IsTrue(bounces.Count == 3, $"Expected to find {3} bounce but instead found {bounces.Count}");
            ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Bounce);
            ValidateKCCBounce(bounces[1], KCCUtils.MovementAction.Move);
            ValidateKCCBounce(bounces[2], KCCUtils.MovementAction.Stop);
        }

        [Test]
        public void Validate_KCCGetGroundVelocity(
            [Values] bool movingGround,
            [Values] bool avoidTransferMomentum,
            [Values] bool rigidbody,
            [Values] bool isKinematic,
            [Values] bool onGround)
        {
            GameObject floor = CreateGameObject();
            MovingGroundComponent ground = null;
            Rigidbody rb = null;

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

            var kccConfig = new KCCConfig();
            var grounded = new Mock<IKCCGrounded>();

            kccConfig.MaxDefaultLaunchVelocity = 2.5f;
            grounded.Setup(e => e.StandingOnGround).Returns(onGround);
            grounded.Setup(e => e.Floor).Returns(floor);

            Vector3 velocity = KCCUtils.GetGroundVelocity(grounded.Object, kccConfig, Vector3.forward);

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
                Assert.AreEqual(Vector3.forward, velocity);
            }
            else
            {
                Assert.AreEqual(Vector3.zero, velocity);
            }
        }

        /// <summary>
        /// Validate a KCC bounce for a specified set of properties.
        /// </summary>
        /// <param name="bounce">Bounce to validate.</param>
        /// <param name="movementAction">Expected movement action, or null if not required to check.</param>
        /// <param name="finalPosition">Expected final position, or null if not required to check.</param>
        /// <param name="initialPosition">Expected initial position, or null if not required to check.</param>
        /// <param name="remainingMomentum">Expected remaining momentum, or null if not required to check.</param>
        /// <param name="initialMomentum">Expected initial momentum, or null if not required to check.</param>
        public void ValidateKCCBounce(
            KCCBounce bounce,
            KCCUtils.MovementAction? movementAction = null,
            Vector3? finalPosition = null,
            Vector3? initialPosition = null,
            Vector3? remainingMomentum = null,
            Vector3? initialMomentum = null)
        {
            Debug.Log($"Evaluating bounce {bounce} for properties finalPosition:{finalPosition}, initialPosition:{initialPosition}, remainingMomentum:{remainingMomentum}, initialMomentum:{initialMomentum}");

            Assert.IsTrue(movementAction == null || bounce.action == movementAction, $"Expected {nameof(bounce.action)} to be {movementAction} but instead found {bounce.action}");
            Assert.IsTrue(finalPosition == null || bounce.finalPosition == finalPosition, $"Expected {nameof(bounce.finalPosition)} to be {finalPosition} but instead found {bounce.finalPosition}");
            Assert.IsTrue(initialPosition == null || bounce.initialPosition == initialPosition, $"Expected {nameof(bounce.initialPosition)} to be {initialPosition} but instead found {bounce.initialPosition}");
            Assert.IsTrue(remainingMomentum == null || bounce.remainingMomentum == remainingMomentum, $"Expected {nameof(bounce.remainingMomentum)} to be {remainingMomentum} but instead found {bounce.remainingMomentum}");
            Assert.IsTrue(initialMomentum == null || bounce.initialMomentum == initialMomentum, $"Expected {nameof(bounce.initialMomentum)} to be {initialMomentum} but instead found {bounce.initialMomentum}");
        }

        private delegate void RaycastHitCallback(Vector3 pos, Quaternion rot, Vector3 dir, float dist, out IRaycastHit hit);

        private delegate void RaycastHitReturns(out IRaycastHit hit);

        /// <summary>
        /// Setup the collider cast for a given set of hits in a specific order.
        /// </summary>
        /// <param name="hitData">Enumerable set of didHit and raycastHit in the order they should be returned.</param>
        public void SetupColliderCast(IEnumerable<(bool, IRaycastHit)> hitData)
        {
            IEnumerator<(bool, IRaycastHit)> hitEnumerator = hitData.GetEnumerator();
            (bool, IRaycastHit) nextHit = (false, null);
            colliderCastMock.Setup(
                mock => mock.CastSelf(It.IsAny<Vector3>(), It.IsAny<Quaternion>(), It.IsAny<Vector3>(), It.IsAny<float>(), out It.Ref<IRaycastHit>.IsAny))
                .Callback(new RaycastHitCallback((Vector3 pos, Quaternion rot, Vector3 dir, float dist, out IRaycastHit hit) =>
                {
                    hit = nextHit.Item2;
                    if (hitEnumerator.MoveNext())
                    {
                        nextHit = hitEnumerator.Current;
                        hit = nextHit.Item2;
                    }
                }))
                .Returns(() => nextHit.Item1);
        }

        /// <summary>
        /// Setup the collider cast.
        /// </summary>
        /// <param name="didHit">Has this collided with anything.</param>
        /// <param name="raycastHit">Raycast hit object to return.</param>
        public void SetupColliderCast(bool didHit, IRaycastHit raycastHit)
        {
            colliderCastMock.Setup(mock => mock.CastSelf(It.IsAny<Vector3>(), It.IsAny<Quaternion>(), It.IsAny<Vector3>(), It.IsAny<float>(), out raycastHit)).Returns(didHit);
        }

        /// <summary>
        /// Get the bounces for a KCC Utils movement action with a set default behaviour.
        /// </summary>
        /// <param name="position">Position to start player movement from.</param>
        /// <param name="movement">Movement to move the player.</param>
        /// <param name="rotation">Rotation of the player during movement.</param>
        /// <param name="maxBounces">Maximum bounces when moving the player.</param>
        /// <param name="pushDecay">Push decay factor for player movement.</param>
        /// <param name="verticalSnapUp">Vertical snap up distance the player can snap up.</param>
        /// <param name="stepUpDepth">Minimum depth required for a stair when moving onto a step.</param>
        /// <param name="anglePower">Angle power for decaying momentum when bouncing off a surface.</param>
        /// <param name="canSnapUp">Can the player snap up steps.</param>
        /// <param name="up">Up direction relative to the player.</param>
        /// <param name="colliderCast">Collider cast for checking what the player is colliding with.</param>
        /// <param name="push">Character push for checking fi the character should push objects.</param>
        /// <returns>Bounces that the player makes when hitting objects as part of it's movement.</returns>
        private IEnumerable<KCCBounce> GetBounces(
            Vector3 position,
            Vector3 movement,
            Quaternion? rotation = null,
            int maxBounces = 5,
            float pushDecay = 0.1f,
            float verticalSnapUp = 0.1f,
            float stepUpDepth = 0.1f,
            float anglePower = 0.9f,
            bool canSnapUp = true,
            Vector3? up = null,
            IColliderCast colliderCast = null,
            ICharacterPush push = null)
        {
            rotation ??= Quaternion.Euler(Vector3.zero);
            up ??= Vector3.up;
            colliderCast ??= colliderCastMock.Object;
            push ??= characterPushMock.Object;

            return KCCUtils.GetBounces(
                position,
                movement,
                rotation.Value,
                new KCCConfig
                {
                    maxBounces = maxBounces,
                    pushDecay = pushDecay,
                    verticalSnapUp = verticalSnapUp,
                    stepUpDepth = stepUpDepth,
                    anglePower = anglePower,
                    canSnapUp = canSnapUp,
                    up = up.Value,
                    colliderCast = colliderCast,
                    push = push
                }
            );
        }
    }
}
