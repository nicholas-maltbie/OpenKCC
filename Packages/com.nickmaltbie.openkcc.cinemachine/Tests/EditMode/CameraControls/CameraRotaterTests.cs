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
using nickmaltbie.OpenKCC.cinemachine.CameraControls;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace nickmaltbie.OpenKCC.Tests.cinemachine.EditMode.CameraControls
{
    [TestFixture]
    public class CameraRotaterTests : TestBase
    {
        private Mock<IUnityService> unityServiceMock;
        private CameraRotater cameraController;
        private Gamepad gamepad;
        private StickControl lookAction;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            GameObject go = CreateGameObject();
            cameraController = go.AddComponent<CameraRotater>();
            unityServiceMock = new Mock<IUnityService>();
            cameraController.followTarget = go;
            cameraController.unityService = unityServiceMock.Object;
            unityServiceMock.Setup(e => e.deltaTime).Returns(0.1f);

            gamepad = InputSystem.AddDevice<Gamepad>();
            lookAction = gamepad.leftStick;
            cameraController.LookAction = new InputAction("look", InputActionType.Value, lookAction.path);
            cameraController.LookAction.Enable();
            PlayerInputUtils.playerMovementState = PlayerInputState.Allow;
        }

        [Test]
        public void Validate_CameraController_DisallowMovement()
        {
            PlayerInputUtils.playerMovementState = PlayerInputState.Deny;

            Assert.AreEqual(0, cameraController.Pitch);
            Assert.AreEqual(0, cameraController.Yaw);

            Set(lookAction, Vector3.up);
            cameraController.Update();

            Assert.AreEqual(0, cameraController.Pitch);
            Assert.AreEqual(0, cameraController.Yaw);

            Set(lookAction, Vector3.left);
            cameraController.Update();

            Assert.AreEqual(0, cameraController.Pitch);
            Assert.AreEqual(0, cameraController.Yaw);
        }

        [Test]
        public void Validate_CameraController_BasicMovement()
        {
            Assert.AreEqual(0, cameraController.Pitch);
            Assert.AreEqual(0, cameraController.Yaw);

            Set(lookAction, Vector3.up);
            cameraController.Update();

            Assert.AreEqual(-cameraController.rotationRate * 0.1f * 0.25f, cameraController.Pitch);
            Assert.AreEqual(0, cameraController.Yaw);

            Set(lookAction, Vector3.left);
            cameraController.Update();

            Assert.AreEqual(-cameraController.rotationRate * 0.1f * 0.25f, cameraController.Pitch);
            Assert.AreEqual(-cameraController.rotationRate * 0.1f * 0.25f, cameraController.Yaw);
        }
    }
}
