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

using System.Collections;
using Moq;
using nickmaltbie.OpenKCC.CameraControls;
using nickmaltbie.OpenKCC.CameraControls.Config;
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Input;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.CameraControls.CameraController"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class CameraControlsTests : TestBase
    {
        private Mock<IUnityService> unityServiceMock;
        private CameraController cameraController;
        private Gamepad gamepad;
        private InputAction lookInputAction;
        private InputAction zoomInputAction;
        private StickControl lookAction;
        private StickControl zoomAction;
        private GameObject box;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            GameObject go = CreateGameObject();
            cameraController = go.AddComponent<CameraController>();
            unityServiceMock = new Mock<IUnityService>();
            cameraController.config.cameraTransform = cameraController.transform;
            cameraController.unityService = unityServiceMock.Object;
            unityServiceMock.Setup(e => e.deltaTime).Returns(0.1f);

            InputActionMap actionMap;
            var jumpInput = new BufferedInput();
            (gamepad, _, actionMap) = base.SetupInputDevice<Gamepad>();
            zoomAction = gamepad.rightStick;
            lookAction = gamepad.leftStick;
            zoomInputAction = actionMap.AddAction("zoomAction", InputActionType.Value, zoomAction.path);
            lookInputAction = actionMap.AddAction("lookAction", InputActionType.Value, lookAction.path);

            cameraController.config.ZoomAction = InputActionReference.Create(zoomInputAction);
            cameraController.config.LookAction = InputActionReference.Create(lookInputAction);

            lookInputAction.Enable();
            zoomInputAction.Enable();
            cameraController.Start();

            PlayerInputUtils.playerMovementState = PlayerInputState.Allow;

            cameraController.config.minCameraDistance = 0.0f;
            cameraController.config.maxCameraDistance = 10.0f;
        }

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            box = CreateGameObject();
            box.AddComponent<BoxCollider>();
            yield return null;
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

            Assert.AreEqual(-cameraController.config.rotationRate * 0.1f * 0.25f, cameraController.Pitch);
            Assert.AreEqual(0, cameraController.Yaw);

            Set(lookAction, Vector3.left);
            cameraController.Update();

            Assert.AreEqual(-cameraController.config.rotationRate * 0.1f * 0.25f, cameraController.Pitch);
            Assert.AreEqual(-cameraController.config.rotationRate * 0.1f * 0.25f, cameraController.Yaw);
        }

        [Test]
        public void Validate_CameraController_IgnoreObjects(
            [NUnit.Framework.Range(2, 10, 4)] float dist
        )
        {
            cameraController.transform.position = Vector3.forward * dist;
            cameraController.config.currentDistance = dist;
            cameraController.config.AddIgnoreObject(box);
            cameraController.Update();

            TestUtils.AssertInBounds(cameraController.config.CameraDistance, dist);
            cameraController.config.RemoveIgnoreObject(box);
        }

        [Test]
        public void Validate_CameraController_SphereCast(
            [NUnit.Framework.Range(2, 10, 4)] float dist
        )
        {
            cameraController.transform.position = Vector3.forward * dist;
            cameraController.config.currentDistance = dist;
            Debug.DrawRay(cameraController.transform.position, -cameraController.transform.forward * cameraController.config.currentDistance);
            cameraController.Update();

            TestUtils.AssertInBounds(cameraController.config.CameraDistance, dist - 0.5f, 0.25f);
        }

        [Test]
        public void Validate_CameraController_ThirdPerson(
            [NUnit.Framework.Range(2, 10, 4)] float dist
        )
        {
            cameraController.transform.position = Vector3.forward * dist;
            cameraController.config.currentDistance = dist;
            cameraController.config.thirdPersonCharacterBase = cameraController.gameObject;

            cameraController.Update();
        }

        [Test]
        public void Validate_CameraController_ThirdPerson_Dither(
            [NUnit.Framework.Range(0, 1, 0.1f)] float dist
        )
        {
            cameraController.transform.position = Vector3.forward * (0.5f + dist);
            cameraController.config.currentDistance = 2.0f;
            cameraController.config.thirdPersonCharacterBase = cameraController.gameObject;

            cameraController.Update();
        }

        [Test]
        public void Validate_CameraController_UpdateSerializationVersion()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            cameraController.config = new CameraConfig();
            cameraController.zoomAction = InputActionReference.Create(zoomInputAction);
            cameraController.lookAction = InputActionReference.Create(lookInputAction);
            cameraController.maxPitch = -1;
            cameraController.minPitch = -1;
            cameraController.rotationRate = 1000;
            cameraController.cameraTransform = CreateGameObject().transform;
            cameraController.baseCameraOffset = Vector3.forward * 100;
            cameraController.minCameraDistance = 10;
            cameraController.maxCameraDistance = 100;
            cameraController.currentDistance = 69;
            cameraController.zoomSpeed = 10;
            cameraController.cameraRaycastMask = 0b111111;
            cameraController.shadowOnlyDistance = 0.5f;
            cameraController.ditherDistance = 1.5f;
            cameraController.thirdPersonCharacterBase = CreateGameObject();
            cameraController.transitionTime = 10.0f;
            cameraController.serializationVersion = "";

            cameraController.OnBeforeSerialize();
            cameraController.OnAfterDeserialize();

            Assert.AreEqual(cameraController.config.zoomActionReference, cameraController.zoomAction);
            Assert.AreEqual(cameraController.config.lookActionReference, cameraController.lookAction);
            Assert.AreEqual(cameraController.config.maxPitch, cameraController.maxPitch);
            Assert.AreEqual(cameraController.config.minPitch, cameraController.minPitch);
            Assert.AreEqual(cameraController.config.rotationRate, cameraController.rotationRate);
            Assert.AreEqual(cameraController.config.cameraTransform, cameraController.cameraTransform);
            Assert.AreEqual(cameraController.config.baseCameraOffset, cameraController.baseCameraOffset);
            Assert.AreEqual(cameraController.config.minCameraDistance, cameraController.minCameraDistance);
            Assert.AreEqual(cameraController.config.maxCameraDistance, cameraController.maxCameraDistance);
            Assert.AreEqual(cameraController.config.currentDistance, cameraController.currentDistance);
            Assert.AreEqual(cameraController.config.zoomSpeed, cameraController.zoomSpeed);
            Assert.AreEqual(cameraController.config.cameraRaycastMask, cameraController.cameraRaycastMask);
            Assert.AreEqual(cameraController.config.shadowOnlyDistance, cameraController.shadowOnlyDistance);
            Assert.AreEqual(cameraController.config.ditherDistance, cameraController.ditherDistance);
            Assert.AreEqual(cameraController.config.thirdPersonCharacterBase, cameraController.thirdPersonCharacterBase);
            Assert.AreEqual(cameraController.config.transitionTime, cameraController.transitionTime);
            Assert.AreEqual(cameraController.serializationVersion, CameraController.CurrentSerializationVersion);

            Assert.AreEqual(cameraController.CameraDistance, cameraController.config.CameraDistance);
            Assert.AreEqual(cameraController.CameraSource, cameraController.config.CameraSource(cameraController.transform));
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}
