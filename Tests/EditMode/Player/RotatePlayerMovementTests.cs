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
using nickmaltbie.OpenKCC.CameraControls;
using nickmaltbie.OpenKCC.cinemachine.Player;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.cinemachine.EditMode.Player
{
    public class MockCameraControls : MonoBehaviour, ICameraControls
    {
        public Quaternion PlayerHeading => Quaternion.identity;
    }

    [TestFixture]
    public class RotatePlayerMovementTests : KCCStateMachineTestBase
    {
        private Mock<IUnityService> unityServiceMock;
        private RotatePlayerMovement rotatePlayer;
        private GameObject avatarBase;

        [SetUp]
        public override void Setup()
        {
            base.Setup();

            avatarBase = new GameObject();
            avatarBase.transform.SetParent(kccStateMachine.transform);
            avatarBase.transform.localPosition = Vector3.zero;
            avatarBase.transform.localRotation = Quaternion.identity;

            kccStateMachine.gameObject.AddComponent<MockCameraControls>();

            rotatePlayer = kccStateMachine.gameObject.AddComponent<RotatePlayerMovement>();
            rotatePlayer.avatarBase = avatarBase;

            unityServiceMock = new Mock<IUnityService>();
            rotatePlayer.unityService = unityServiceMock.Object;

            unityServiceMock.Setup(e => e.deltaTime).Returns(0.1f);
        }

        [Test]
        public void Validate_PlayerRotation()
        {
            // Set the kcc state machine input vector
            Set(moveStick, Vector2.up);
            kccStateMachine.ReadPlayerMovement();
            Assert.AreEqual(kccStateMachine.InputMovement, Vector3.forward);

            // When updating, assert that the player avatar rotates towards
            // the movement input vector
            rotatePlayer.LateUpdate();

            TestUtils.AssertInBounds(avatarBase.transform.forward, Vector3.forward);

            // If the player turns the movement to be right, avatar should
            // rotate towards the right
            Set(moveStick, Vector2.right);
            kccStateMachine.ReadPlayerMovement();
            Assert.AreEqual(kccStateMachine.InputMovement, Vector3.right);

            // When updating, assert that the player avatar rotates towards
            // the movement input vector
            for (int update = 0; update < 1000; update++)
            {
                float expectedYaw = Mathf.Min(update * rotatePlayer.rotationSpeed * 0.1f, 90.0f);
                TestUtils.AssertInBounds(avatarBase.transform.forward, Quaternion.Euler(0, expectedYaw, 0) * Vector3.forward, 0.01f);
                rotatePlayer.LateUpdate();
            }
        }

        [Test]
        public void Validate_NoRotatePlayer([Random(10)] int seed)
        {
            Random.InitState(seed);
            Set(moveStick, Random.onUnitSphere);
            kccStateMachine.ReadPlayerMovement();

            for (int i = 0; i < 10; i++)
            {
                rotatePlayer.LateUpdate();
            }

            // Assert that when the player is not moving, the caemera doesn't move either
            Quaternion lockedHeading = avatarBase.transform.rotation;
            Set(moveStick, Vector2.zero);
            kccStateMachine.ReadPlayerMovement();
            for (int update = 0; update < 100; update++)
            {
                rotatePlayer.LateUpdate();
                TestUtils.AssertInBounds(avatarBase.transform.forward, lockedHeading * Vector3.forward);
            }
        }
    }
}
