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
using nickmaltbie.openkcc.Tests.netcode.TestCommon;
using nickmaltbie.OpenKCC.netcode.CameraControls;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace nickmaltbie.openkcc.Tests.netcode.Runtime.CameraControls
{
    /// <summary>
    /// Simple tests for NetworkCameraFollow.
    /// </summary>
    [TestFixture]
    public class NetworkCameraFollowTests : NetcodeInputRuntimeTest<NetworkCameraFollow>
    {
        protected override int NumberOfClients => 2;
        protected GameObject camera;

        [UnityTearDown]
        public override IEnumerator UnityTearDown()
        {
            yield return base.UnityTearDown();
            Object.Destroy(camera);
        }

        [UnitySetUp]
        public override IEnumerator UnitySetUp()
        {
            yield return base.UnitySetUp();

            // Create main camera
            var go = new GameObject();
            go.tag = "MainCamera";
            go.AddComponent<Camera>();
        }

        public override void SetupInputs(Gamepad gamepad, TestableNetworkBehaviour b, NetworkCameraFollow e) { }
        public override void SetupPrefab(GameObject go)
        {
            NetworkCameraController ncc = go.AddComponent<NetworkCameraController>();
            ncc.config.cameraTransform = go.transform;
        }

        public override void SetupClient(NetworkCameraFollow e, int objectIdx, int clientIdx)
        {
            e.transform.position = Vector3.right * clientIdx * 2;
            NetworkCameraController ncc = e.GetComponent<NetworkCameraController>();
            ncc.config.cameraTransform = ncc.transform;
        }

        [UnityTest]
        public IEnumerator Verify_NetworkCameraFollow()
        {
            // Disable all owners by default
            base.ForEachOwner((follow, _) => follow.enabled = false);

            SetupInputs();
            for (int i = 0; i <= NumberOfClients; i++)
            {
                NetworkCameraFollow cameraFollow = GetAttachedNetworkBehaviour(i, i);
                TestableNetworkBehaviour testable = GetTestableNetworkBehaviour(i, i);

                // Enable this behaviour
                cameraFollow.enabled = true;

                // Wait until camera moves to camera follow's position
                yield return TestUtils.WaitUntil(() =>
                {
                    try
                    {
                        TestUtils.AssertInBounds(Camera.main.transform.position, cameraFollow.GetComponent<NetworkCameraController>().config.cameraTransform.position);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                });

                // Disable
                cameraFollow.enabled = false;
            }
        }
    }
}
