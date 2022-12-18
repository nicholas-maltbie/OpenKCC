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

using nickmaltbie.OpenKCC.CameraControls;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.CameraControls.CameraFollow"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class CameraFollowTests : TestBase
    {
        private CameraFollow cameraFollow;
        private AudioListener audioListener;
        private Camera camera;

        [SetUp]
        public override void Setup()
        {
            GameObject cameraGo = CreateGameObject();
            GameObject followGo = CreateGameObject();
            CameraController cameraController = followGo.AddComponent<CameraController>();

            audioListener = CreateGameObject().AddComponent<AudioListener>();
            cameraFollow = followGo.AddComponent<CameraFollow>();
            cameraController.config.cameraTransform = cameraController.transform;

            camera = cameraGo.AddComponent<Camera>();
            camera.tag = "MainCamera";
            Assert.AreEqual(Camera.main, camera);

            cameraFollow.Start();
        }

        [Test]
        public void Verify_CameraFollow_NullCamera()
        {
            GameObject.DestroyImmediate(camera);
            cameraFollow.LateUpdate();
        }

        [Test]
        public void Verify_CameraFollow_Following(
            [ValueSource(nameof(TestDirections))] Vector3 dir
        )
        {
            cameraFollow.transform.position = dir;
            Assert.AreEqual(cameraFollow.transform.position, dir);
            Assert.AreEqual(camera.transform.position, Vector3.zero);
            Assert.AreEqual(audioListener.transform.position, Vector3.zero);

            cameraFollow.LateUpdate();

            Assert.AreEqual(cameraFollow.transform.position, dir);
            Assert.AreEqual(camera.transform.position, dir);
            Assert.AreEqual(audioListener.transform.position, dir);
        }
    }
}
