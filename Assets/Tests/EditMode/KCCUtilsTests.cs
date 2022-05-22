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
using nickmaltbie.OpenKCC.Utils;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode
{
    [TestFixture]
    public class KCCUtilsTests
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
        [Test, Sequential]
        public void KCCMaxBouncesTest([Values(0, 1, 5, 10)] int maxBounces)
        {
            // Have the object hit some collider and not move at all
            Vector3 initialPosition = Vector3.zero;
            Vector3 movement = Vector3.forward;

            // Have collider return hitting something... but it shouldn't be called due to no movement
            var hit = new RaycastHit
            {
                point = Vector3.zero,
                distance = KCCUtils.Epsilon,
                normal = Vector3.zero
            };
            colliderCastMock.Setup(mock => mock.CastSelf(It.IsAny<Vector3>(), It.IsAny<Quaternion>(), It.IsAny<Vector3>(), It.IsAny<float>(), out hit)).Returns(true);

            // Simulate bounces
            var bounces = GetBounces(initialPosition, movement, maxBounces: maxBounces, anglePower: 0.0f, usePush: false).ToList();

            // Should hit max bounces
            Assert.IsTrue(bounces.Count == maxBounces + 2, $"Expected to find {maxBounces + 2} bounce but instead found {bounces.Count}");
            Enumerable.Range(0, maxBounces + 1).ToList().ForEach(idx => ValidateKCCBounce(bounces[idx], KCCUtils.MovementAction.Bounce));
            ValidateKCCBounce(bounces[maxBounces + 1], KCCUtils.MovementAction.Stop);
        }

        /// <summary>
        /// Test maximum bounces by having the object overlap with another.
        /// </summary>
        [Test]
        public void KCCOverlap()
        {
            // Have the object hit some collider and not move at all
            Vector3 initialPosition = Vector3.zero;
            Vector3 movement = Vector3.forward;

            // Have collider return hitting something... but it shouldn't be called due to no movement
            var hit = new RaycastHit
            {
                point = Vector3.zero,
                distance = 0,
                normal = Vector3.zero
            };
            colliderCastMock.Setup(mock => mock.CastSelf(It.IsAny<Vector3>(), It.IsAny<Quaternion>(), It.IsAny<Vector3>(), It.IsAny<float>(), out hit)).Returns(true);

            // Simulate bounces
            var bounces = GetBounces(initialPosition, movement, anglePower: 0.0f, usePush: false).ToList();

            // Should just return just one element with action stop
            NUnit.Framework.Assert.IsTrue(bounces.Count == 1, $"Expected to find {1} bounce but instead found {bounces.Count}");

            // Validate bounce properties
            ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Stop, finalPosition: initialPosition, remainingMomentum: Vector3.zero);
        }

        /// <summary>
        /// Test no movement from KCC.
        /// </summary>
        [Test]
        public void KCCNoMovement()
        {
            // Have collider return hitting something... but it shouldn't be called due to no movement
            var hit = new RaycastHit();
            colliderCastMock.Setup(mock => mock.CastSelf(It.IsAny<Vector3>(), It.IsAny<Quaternion>(), It.IsAny<Vector3>(), It.IsAny<float>(), out hit)).Returns(false);

            // Simulate bounces
            var bounces = GetBounces(Vector3.zero, Vector3.zero).ToList();

            // Should just return just one element with action stop
            Assert.IsTrue(bounces.Count == 1, $"Expected to find {1} bounce but instead found {bounces.Count}");

            // Validate bounce properties
            ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Stop, Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
        }

        public static IEnumerable<Vector3> MovementGenerator()
        {
            return new[]
            {
                Vector3.forward,
                Vector3.right,
                Vector3.left,
                Vector3.back
            };
        }

        /// <summary>
        /// Test kcc hit nothing.
        /// </summary>
        [Test]
        [TestCaseSource(nameof(MovementGenerator))]
        public void KCCMoveUnblocked(Vector3 movement)
        {
            Vector3 initialPosition = Vector3.zero;
            Vector3 expectedFinalPosition = initialPosition + movement;

            // Have collider return hitting something... but it shouldn't be called due to no movement
            var hit = new RaycastHit();
            colliderCastMock.Setup(mock => mock.CastSelf(It.IsAny<Vector3>(), It.IsAny<Quaternion>(), It.IsAny<Vector3>(), It.IsAny<float>(), out hit)).Returns(false);

            // Simulate bounces
            var bounces = GetBounces(initialPosition, movement).ToList();

            // Validate bounce properties
            Assert.IsTrue(bounces.Count == 2, $"Expected to find {2} bounce but instead found {bounces.Count}");
            ValidateKCCBounce(bounces[0], KCCUtils.MovementAction.Move, expectedFinalPosition, initialPosition, Vector3.zero, movement);
            ValidateKCCBounce(bounces[1], KCCUtils.MovementAction.Stop, expectedFinalPosition, expectedFinalPosition, Vector3.zero, Vector3.zero);
        }

        /// <summary>
        /// Validate a KCC bounce for a specified set of properties.
        /// </summary>
        /// <param name="bounce"></param>
        /// <param name="movementAction"></param>
        /// <param name="finalPosition"></param>
        /// <param name="initialPosition"></param>
        /// <param name="remainingMomentum"></param>
        /// <param name="initialMomentum"></param>
        public void ValidateKCCBounce(
            KCCBounce bounce,
            KCCUtils.MovementAction? movementAction = null,
            Vector3? finalPosition = null,
            Vector3? initialPosition = null,
            Vector3? remainingMomentum = null,
            Vector3? initialMomentum = null)
        {
            Assert.IsTrue(movementAction == null || bounce.action == movementAction, $"Expected {nameof(bounce.action)} to be {movementAction} but instead found {bounce.action}");
            Assert.IsTrue(finalPosition == null || bounce.finalPosition == finalPosition, $"Expected {nameof(bounce.finalPosition)} to be {finalPosition} but instead found {bounce.finalPosition}");
            Assert.IsTrue(initialPosition == null || bounce.initialPosition == initialPosition, $"Expected {nameof(bounce.initialPosition)} to be {initialPosition} but instead found {bounce.initialPosition}");
            Assert.IsTrue(remainingMomentum == null || bounce.remainingMomentum == remainingMomentum, $"Expected {nameof(bounce.remainingMomentum)} to be {remainingMomentum} but instead found {bounce.remainingMomentum}");
            Assert.IsTrue(initialMomentum == null || bounce.initialMomentum == initialMomentum, $"Expected {nameof(bounce.initialMomentum)} to be {initialMomentum} but instead found {bounce.initialMomentum}");
        }

        /// <summary>
        /// Get the bounces for a KCC Utils movement action with a set default behaviour.
        /// </summary>
        /// <param name="position"></param>
        /// <param name="movement"></param>
        /// <param name="rotation"></param>
        /// <param name="maxBounces"></param>
        /// <param name="pushDecay"></param>
        /// <param name="verticalSnapUp"></param>
        /// <param name="stepUpDepth"></param>
        /// <param name="anglePower"></param>
        /// <param name="attemptingJump"></param>
        /// <param name="canSnapUp"></param>
        /// <param name="up"></param>
        /// <param name="colliderCast"></param>
        /// <param name="push"></param>
        /// <returns></returns>
        private IEnumerable<KCCBounce> GetBounces(
            Vector3 position,
            Vector3 movement,
            Quaternion? rotation = null,
            int maxBounces = 5,
            float pushDecay = 0.0f,
            float verticalSnapUp = 0.0f,
            float stepUpDepth = 0.0f,
            float anglePower = 0.9f,
            bool attemptingJump = false,
            bool canSnapUp = false,
            Vector3? up = null,
            IColliderCast colliderCast = null,
            ICharacterPush push = null,
            bool usePush = true)
        {
            rotation ??= Quaternion.Euler(Vector3.zero);
            up ??= Vector3.up;
            colliderCast ??= colliderCastMock.Object;
            push ??= usePush ? characterPushMock.Object : null;

            return KCCUtils.GetBounces(
                maxBounces,
                pushDecay,
                verticalSnapUp,
                stepUpDepth,
                anglePower,
                attemptingJump,
                canSnapUp,
                position,
                movement,
                rotation.Value,
                up.Value,
                colliderCast,
                push
            );
        }
    }
}
