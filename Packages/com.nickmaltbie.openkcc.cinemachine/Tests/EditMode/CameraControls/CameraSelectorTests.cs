
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