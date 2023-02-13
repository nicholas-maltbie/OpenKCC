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

using System.Collections;
using Cinemachine;
using nickmaltbie.OpenKCC.cinemachine.CameraControls;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Tests.cinemachine.EditMode.CameraControls
{
    [TestFixture]
    public class CameraSelectorTests : TestBase
    {
        [UnityTest]
        public IEnumerator Validate_CameraSelectorChange()
        {
            CameraSelector selector = CreateGameObject().AddComponent<CameraSelector>();
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            selector.ToggleCameraAction = new InputAction("toggle", InputActionType.Button, gamepad.aButton.path);
            CinemachineVirtualCamera vcam1 = CreateGameObject().AddComponent<CinemachineVirtualCamera>();
            CinemachineVirtualCamera vcam2 = CreateGameObject().AddComponent<CinemachineVirtualCamera>();

            selector.cameras = new[] { vcam1, vcam2 };
            selector.Awake();
            Assert.AreEqual(selector, CameraSelector.Instance);

            // Verify that vcam1 is enabled and vcam2 is disabled.
            Assert.IsTrue(vcam1.gameObject.activeSelf);
            Assert.IsFalse(vcam2.gameObject.activeSelf);

            // Trigger input action
            Set(gamepad.aButton, 1);
            yield return null;

            Assert.IsFalse(vcam1.gameObject.activeSelf);
            Assert.IsTrue(vcam2.gameObject.activeSelf);

            // Verify cleanup of singleton
            selector.OnDestroy();
            Assert.IsNull(CameraSelector.Instance);
        }
    }
}
