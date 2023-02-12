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

using Cinemachine;
using nickmaltbie.OpenKCC.cinemachine.CameraControls;
using nickmaltbie.OpenKCC.cinemachine.UI;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using TMPro;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.Tests.cinemachine.EditMode.UI
{
    [TestFixture]
    public class DisplaySelectedCameraTests : TestBase
    {
        [Test]
        public void Validate_Camera_Update()
        {
            DisplaySelectedCamera displayCamera = CreateGameObject().AddComponent<DisplaySelectedCamera>();

            CameraSelector selector = CreateGameObject().AddComponent<CameraSelector>();
            selector.overrideToggleCamera = new InputAction();

            TMP_Text text = CreateGameObject().AddComponent<TextMeshPro>();
            displayCamera.text = text;

            CinemachineVirtualCamera vcam1 = CreateGameObject().AddComponent<CinemachineVirtualCamera>();
            CinemachineVirtualCamera vcam2 = CreateGameObject().AddComponent<CinemachineVirtualCamera>();

            vcam1.name = "VCam 1";
            vcam2.name = "VCam 2";

            selector.cameras = new [] {vcam1, vcam2};
            selector.Awake();

            displayCamera.Update();

            Assert.AreEqual("Selected Camera: " + vcam1.name, text.text);
        }
    }
}
