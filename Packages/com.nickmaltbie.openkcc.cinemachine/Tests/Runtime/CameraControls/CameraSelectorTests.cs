
using System.Collections;
using Cinemachine;
using nickmaltbie.OpenKCC.cinemachine.CameraControls;
using nickmaltbie.OpenKCC.cinemachine.UI;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Tests.cinemachine.Runtime.CameraControls
{
    [TestFixture]
    public class CameraSelectorTests : TestBase
    {
        [UnityTest]
        public IEnumerator Validate_CameraSelector_SingletonInit()
        {
            Gamepad gamepad = InputSystem.AddDevice<Gamepad>();
            CinemachineVirtualCamera vcam1 = CreateGameObject().AddComponent<CinemachineVirtualCamera>();
            CinemachineVirtualCamera vcam2 = CreateGameObject().AddComponent<CinemachineVirtualCamera>();

            CameraSelector selector1 = CreateGameObject().AddComponent<CameraSelector>();
            CameraSelector selector2 = CreateGameObject().AddComponent<CameraSelector>();

            yield return null;
            Assert.AreEqual(selector1, CameraSelector.Instance);
        }
    }
}