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

using Moq;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character.Action
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.Character.Config.KCCGroundedState"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class KCCGroundedStateTests : TestBase
    {
        private Mock<IColliderCast> colliderCastMock;
        private Mock<IKCCConfig> kccConfigMock;
        private KCCGroundedState kccGroundedState;

        [SetUp]
        public void SetUp()
        {
            colliderCastMock = new Mock<IColliderCast>();
            kccConfigMock = new Mock<IKCCConfig>();
            kccGroundedState = new KCCGroundedState();
            kccConfigMock.Setup(e => e.ColliderCast).Returns(colliderCastMock.Object);
            kccConfigMock.Setup(e => e.Up).Returns(Vector3.up);
            kccGroundedState.groundedDistance = 0.01f;
            kccGroundedState.maxWalkAngle = 60.0f;
        }

        [Test]
        public void Validate_KCCGroundedState_CheckGrounded_StandingOnGround()
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, normal: Vector3.up, distance: 0.001f, didHit: true);
            kccGroundedState.CheckGrounded(kccConfigMock.Object, Vector3.zero, Quaternion.identity);

            TestUtils.AssertInBounds(kccGroundedState.Angle, 0.0f);
            Assert.IsTrue(kccGroundedState.StandingOnGround);
            Assert.IsTrue(kccGroundedState.DistanceToGround == 0.001f);
            Assert.IsFalse(kccGroundedState.Sliding);
        }

        [Test]
        public void Validate_KCCGroundedState_CheckGrounded_Falling(
            [Values(10.0f, 100.0f, Mathf.Infinity)] float distance
        )
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, normal: Vector3.up, distance: distance, didHit: distance < Mathf.Infinity);
            kccGroundedState.CheckGrounded(kccConfigMock.Object, Vector3.zero, Quaternion.identity);

            Assert.IsFalse(kccGroundedState.StandingOnGround);
            Assert.IsFalse(kccGroundedState.Sliding);
            Assert.IsTrue(kccGroundedState.DistanceToGround == distance);
            Assert.IsTrue(kccGroundedState.Falling);
        }

        [Test]
        public void Validate_KCCGroundedState_GetProjectdMovement(
            [Values] bool falling)
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, normal: Vector3.up, distance: 0.001f, didHit: falling);
            kccGroundedState.CheckGrounded(kccConfigMock.Object, Vector3.zero, Quaternion.identity);

            if (falling)
            {
                Assert.AreEqual(Vector3.forward, kccGroundedState.GetProjectedMovement(Vector3.forward));
            }
            else
            {
                Assert.AreEqual(Vector3.forward, kccGroundedState.GetProjectedMovement(Vector3.forward));
                Assert.AreEqual(Vector3.right, kccGroundedState.GetProjectedMovement(Vector3.right));
                Assert.AreEqual(Vector3.up, kccGroundedState.GetProjectedMovement(Vector3.up));
            }
        }

        [Test]
        public void Validate_KCCGroundedState_CheckGrounded_Sliding()
        {
            KCCTestUtils.SetupCastSelf(colliderCastMock, distance: 0.001f, normal: (Vector3.up + Vector3.right * 10).normalized, didHit: true);
            kccGroundedState.CheckGrounded(kccConfigMock.Object, Vector3.zero, Quaternion.identity);

            Assert.IsTrue(kccGroundedState.StandingOnGround);
            Assert.IsTrue(kccGroundedState.Sliding);
            Assert.IsTrue(kccGroundedState.DistanceToGround == 0.001f);
        }
    }
}
