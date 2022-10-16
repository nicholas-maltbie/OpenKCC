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
using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Environment;
using nickmaltbie.OpenKCC.Input;
using nickmaltbie.OpenKCC.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character.Action
{
    /// <summary>
    /// Basic tests for ConditionalAction in edit mode.
    /// </summary>
    [TestFixture]
    public class JumpActionTests : TestBase
    {
        private Mock<IUnityService> unityServiceMock;
        private Mock<IKCCConfig> kccConfigMock;
        private Mock<IKCCGrounded> kccGroundedMock;
        private Mock<IJumping> jumpingMock;

        private Gamepad gamepad;
        private InputAction testAction;
        private JumpAction jumpAction;

        private bool condition = true;
        private Vector3 inputJump = Vector3.zero;

        [SetUp]
        public void SetUp()
        {
            unityServiceMock = new Mock<IUnityService>();
            unityServiceMock.Setup(e => e.deltaTime).Returns(1.0f);

            jumpAction = new JumpAction();
            jumpAction.unityService = unityServiceMock.Object;
            jumpAction.SetCondition(() => condition);

            InputActionMap actionMap;
            (gamepad, _, actionMap) = base.SetupInputDevice<Gamepad>();
            testAction = actionMap.AddAction("testAction", InputActionType.Button, gamepad.aButton.path);

            var bufferedInput = new BufferedInput();
            bufferedInput.unityService = unityServiceMock.Object;
            bufferedInput.inputAction = InputActionReference.Create(testAction);
            bufferedInput.cooldown = 1.0f;
            bufferedInput.bufferTime = 3.0f;

            jumpingMock = new Mock<IJumping>();
            kccConfigMock = new Mock<IKCCConfig>();
            kccGroundedMock = new Mock<IKCCGrounded>();

            kccConfigMock.Setup(e => e.Up).Returns(Vector3.up);
            jumpingMock.Setup(e => e.ApplyJump(It.IsAny<Vector3>()))
                .Callback((Vector3 jump) => inputJump = jump);
            jumpAction.Setup(kccGroundedMock.Object, kccConfigMock.Object, jumpingMock.Object);

            jumpAction.jumpInput = bufferedInput;

            testAction.Enable();
        }

        [Test]
        public void Verify_JumpAction_CanJump_Sliding()
        {
            jumpAction.maxJumpAngle = 60.0f;
            kccGroundedMock.Setup(e => e.StandingOnGround).Returns(true);
            kccGroundedMock.Setup(e => e.Angle).Returns(30);
            kccGroundedMock.Setup(e => e.Sliding).Returns(false);

            // Assert that can jump when standing on flat ground.
            Assert.IsTrue(jumpAction.CanJump());

            // Assert that can jump while sliding.
            kccGroundedMock.Setup(e => e.Sliding).Returns(true);
            Assert.IsTrue(jumpAction.CanJump());

            // Assert that cannot jump two times while sliding.
            jumpAction.Jump();
            Assert.IsFalse(jumpAction.CanJump());
        }

        [Test]
        public void Verify_JumpAction_CanJump_SteepSlope()
        {
            jumpAction.maxJumpAngle = 60.0f;
            kccGroundedMock.Setup(e => e.StandingOnGround).Returns(true);
            kccGroundedMock.Setup(e => e.Angle).Returns(30);
            kccGroundedMock.Setup(e => e.Sliding).Returns(false);

            // Assert that can jump when standing on flat ground.
            Assert.IsTrue(jumpAction.CanJump());

            // Assert that can jump while sliding.
            kccGroundedMock.Setup(e => e.Sliding).Returns(true);
            Assert.IsTrue(jumpAction.CanJump());

            jumpAction.maxJumpAngle = 20.0f;

            // Assert that cannot jump too steep slope
            Assert.IsFalse(jumpAction.CanJump());
        }

        [UnityTest]
        public IEnumerator Validate_JumpAction_AttemptingJump()
        {
            // set the value as true and update the input device
            Set(gamepad.aButton, 1);
            yield return null;
            jumpAction.Update();
            Assert.IsTrue(jumpAction.AttemptingJump);

            Set(gamepad.aButton, 0);
            yield return null;
            jumpAction.Update();
            Assert.IsTrue(jumpAction.AttemptingJump);

            unityServiceMock.Setup(e => e.deltaTime).Returns(1000);
            jumpAction.Update();
            Assert.IsFalse(jumpAction.AttemptingJump);
        }

        [Test]
        public void Verify_JumpAction_CanJump_NotGrounded()
        {
            jumpAction.maxJumpAngle = 60.0f;
            kccGroundedMock.Setup(e => e.StandingOnGround).Returns(false);
            kccGroundedMock.Setup(e => e.Angle).Returns(30);
            kccGroundedMock.Setup(e => e.Sliding).Returns(false);

            // Assert that can jump when standing on flat ground.
            Assert.IsFalse(jumpAction.CanJump());
        }
    }
}
