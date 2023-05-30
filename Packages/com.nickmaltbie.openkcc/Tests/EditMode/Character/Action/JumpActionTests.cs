﻿// Copyright (C) 2023 Nicholas Maltbie
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

using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Input;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character.Action
{
    /// <summary>
    /// Basic tests for ConditionalAction in edit mode.
    /// </summary>
    [TestFixture]
    public class JumpActionTests : TestBase
    {
        private MockUnityService unityServiceMock;
        private KCCConfig kccConfigMock;
        private MockKCCGrounded kccGroundedMock;
        private MockJumping jumpingMock;

        private Gamepad gamepad;
        private InputAction testAction;
        private JumpAction jumpAction;

        private bool condition = true;
        private Vector3 inputJump = Vector3.zero;

        [SetUp]
        public void SetUp()
        {
            base.Setup();

            unityServiceMock = new MockUnityService();
            unityServiceMock.deltaTime = 1.0f;

            jumpAction = new JumpAction();
            jumpAction.unityService = unityServiceMock;
            jumpAction.SetCondition(() => condition);

            InputActionMap actionMap;
            (gamepad, _, actionMap) = base.SetupInputDevice<Gamepad>();
            testAction = actionMap.AddAction("testAction", InputActionType.Button, gamepad.aButton.path, interactions: "press");

            var bufferedInput = new BufferedInput();
            bufferedInput.unityService = unityServiceMock;
            bufferedInput.inputActionReference = InputActionReference.Create(testAction);
            bufferedInput.cooldown = 1.0f;
            bufferedInput.bufferTime = 3.0f;

            jumpingMock = new MockJumping();
            kccConfigMock = new KCCConfig
            {
                Up = Vector3.up,
            };
            kccGroundedMock = new MockKCCGrounded();

            jumpingMock.OnJump = (Vector3 jump) => inputJump = jump;
            jumpAction.jumpInput = bufferedInput;
            jumpAction.Setup(kccGroundedMock, kccConfigMock, jumpingMock);

            testAction.Enable();
        }

        [Test]
        public void Verify_JumpAction_JumpDirection([NUnit.Framework.Range(0.0f, 1.0f, 0.2f)] float jumpWeight)
        {
            jumpAction.jumpAngleWeightFactor = jumpWeight;

            kccGroundedMock.StandingOnGround = true;
            kccGroundedMock.SurfaceNormal = Vector3.forward;
            jumpAction.Jump();

            Vector3 expected = Vector3.Lerp(Vector3.up, Vector3.forward, jumpWeight).normalized * jumpAction.jumpVelocity;

            TestUtils.AssertInBounds(inputJump, expected);
        }

        [Test]
        public void Verify_JumpAction_JumpedWhileSliding()
        {
            kccGroundedMock.Sliding = true;
            Assert.IsFalse(jumpAction.JumpedWhileSliding);

            jumpAction.Jump();
            Assert.IsTrue(jumpAction.JumpedWhileSliding);
            jumpAction.Update();
            Assert.IsTrue(jumpAction.JumpedWhileSliding);

            kccGroundedMock.StandingOnGround = true;
            kccGroundedMock.Sliding = false;
            jumpAction.Update();
            Assert.IsFalse(jumpAction.JumpedWhileSliding);
        }

        [Test]
        public void Verify_JumpAction_ApplyJumpIfPossible()
        {
            jumpAction.maxJumpAngle = 60.0f;
            kccGroundedMock.StandingOnGround = true;
            kccGroundedMock.Angle = 30;
            kccGroundedMock.Sliding = false;

            // Assert that can jump when standing on flat ground.
            Set(gamepad.aButton, 1);

            jumpAction.Update();
            Assert.IsTrue(jumpAction.AttemptingJump);
            Assert.IsTrue(jumpAction.ApplyJumpIfPossible(kccGroundedMock));
            Assert.IsFalse(jumpAction.ApplyJumpIfPossible(kccGroundedMock));
        }

        [Test]
        public void Verify_JumpAction_CanJump_Sliding()
        {
            jumpAction.maxJumpAngle = 60.0f;
            kccGroundedMock.StandingOnGround = true;
            kccGroundedMock.Angle = 30;
            kccGroundedMock.Sliding = false;

            // Assert that can jump when standing on flat ground.
            Assert.IsTrue(jumpAction.CanJump());

            // Assert that can jump while sliding.
            kccGroundedMock.Sliding = true;
            Assert.IsTrue(jumpAction.CanJump());

            // Assert that cannot jump two times while sliding.
            jumpAction.Jump();
            Assert.IsFalse(jumpAction.CanJump());
        }

        [Test]
        public void Verify_JumpAction_CanJump_SteepSlope()
        {
            jumpAction.maxJumpAngle = 60.0f;
            kccGroundedMock.StandingOnGround = true;
            kccGroundedMock.Angle = 30;
            kccGroundedMock.Sliding = false;

            // Assert that can jump when standing on flat ground.
            Assert.IsTrue(jumpAction.CanJump());

            // Assert that can jump while sliding.
            kccGroundedMock.Sliding = true;
            Assert.IsTrue(jumpAction.CanJump());

            jumpAction.maxJumpAngle = 20.0f;

            // Assert that cannot jump too steep slope
            Assert.IsFalse(jumpAction.CanJump());
        }

        [Test]
        public void Validate_JumpAction_AttemptingJump()
        {
            // set the value as true and update the input device
            Press(gamepad.aButton);
            jumpAction.Update();
            Assert.IsTrue(jumpAction.AttemptingJump);

            jumpAction.Update();
            Assert.IsTrue(jumpAction.AttemptingJump);

            unityServiceMock.deltaTime = 100;
            jumpAction.Update();
            Assert.IsFalse(jumpAction.CanJump());
        }

        [Test]
        public void Verify_JumpAction_CanJump_NotGrounded()
        {
            jumpAction.maxJumpAngle = 60.0f;
            kccGroundedMock.StandingOnGround = false;
            kccGroundedMock.Angle = 30;
            kccGroundedMock.Sliding = false;

            // Assert that can jump when standing on flat ground.
            Assert.IsFalse(jumpAction.CanJump());
        }
    }
}
