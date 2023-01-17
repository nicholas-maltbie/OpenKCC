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
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.TestTools;

namespace nickmaltbie.openkcc.Tests.netcode.Runtime.CameraControls
{
    /// <summary>
    /// Simple tests for NetworkCameraController.
    /// </summary>
    [TestFixture]
    public class NetworkCameraControllerTests : NetcodeInputRuntimeTest<NetworkCameraController>
    {
        public const string ZoomActionName = "Zoom";
        public const string LookActionName = "Look";

        protected override int NumberOfClients => 2;

        public override void SetupInputs(Gamepad gamepad, TestableNetworkBehaviour b, NetworkCameraController e)
        {
            b.SetupControl(ZoomActionName, gamepad.leftStick);
            b.SetupControl(LookActionName, gamepad.rightStick);

            new InputAction("zoom", InputActionType.Button, b.GetControl(ZoomActionName).path).Enable();
            new InputAction("look", InputActionType.Value, b.GetControl(LookActionName).path).Enable();
        }

        public override void SetupPrefab(GameObject go)
        {
            NetworkCameraController cc = go.GetComponent<NetworkCameraController>();
            cc.config.cameraTransform = go.transform;
        }

        public override void SetupClient(NetworkCameraController e, int objectIdx, int clientIdx)
        {
            e.transform.position = Vector3.right * clientIdx * 2;
            e.config.thirdPersonCharacterBase = e.gameObject;
        }

        [UnityTest]
        public IEnumerator Verify_NetworkCameraController_Move()
        {
            SetupInputs();
            for (int i = 0; i <= NumberOfClients; i++)
            {
                NetworkCameraController cameraController = GetAttachedNetworkBehaviour(i, i);
                TestableNetworkBehaviour testable = GetTestableNetworkBehaviour(i, i);

                cameraController.PreviousOpacity = 0.0f;
                Assert.AreEqual(cameraController.PreviousOpacity, 0.0f);

                Assert.AreEqual(0, cameraController.Pitch);
                Assert.AreEqual(0, cameraController.Yaw);

                input.Set(testable.GetControl<StickControl>(LookActionName), Vector3.up);
                yield return null;

                Assert.LessOrEqual(-cameraController.config.rotationRate * 0.1f * 0.25f, cameraController.Pitch);
                Assert.AreEqual(0, cameraController.Yaw);

                input.Set(testable.GetControl<StickControl>(LookActionName), Vector3.left);
                yield return null;

                Assert.LessOrEqual(-cameraController.config.rotationRate * 0.1f * 0.25f, cameraController.Pitch);
                Assert.LessOrEqual(-cameraController.config.rotationRate * 0.1f * 0.25f, cameraController.Yaw);
            }
        }
    }
}
