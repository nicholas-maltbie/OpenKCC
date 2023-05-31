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
using nickmaltbie.OpenKCC.cinemachine.Player;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Tests.cinemachine.EditMode.Player
{
    [TestFixture]
    public class PlayerFadeTests : TestBase
    {
        private MockUnityService unityServiceMock;
        private PlayerFade playerFade;
        private GameObject camera;
        private GameObject box;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            GameObject go = CreateGameObject();
            playerFade = go.AddComponent<PlayerFade>();
            unityServiceMock = new MockUnityService();
            playerFade.followTarget = go;
            playerFade.avatarBase = go;
            playerFade.unityService = unityServiceMock;
            unityServiceMock.deltaTime = 0.1f;

            camera = CreateGameObject();
            camera.AddComponent<Camera>();
            camera.tag = "MainCamera";
        }

        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            box = CreateGameObject();
            box.AddComponent<BoxCollider>();
            yield return null;
        }

        [Test]
        public void Validate_CameraController_SphereCast(
            [NUnit.Framework.Range(2, 10, 4)] float dist
        )
        {
            camera.transform.position = Vector3.forward * dist;
            playerFade.Update();
        }

        [Test]
        public void Validate_CameraController_ThirdPerson_Dither(
            [NUnit.Framework.Range(0, 1, 0.1f)] float dist
        )
        {
            camera.transform.position = Vector3.forward * (0.5f + dist);
            playerFade.Update();
        }

        [Test]
        public void Validate_CameraController_ThirdPerson_ShadowOnly(
            [NUnit.Framework.Range(0, 1, 0.1f)] float dist
        )
        {
            camera.transform.position = Vector3.forward * (dist);
            playerFade.Update();
        }
    }
}
