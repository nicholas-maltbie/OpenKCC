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

using Moq;
using nickmaltbie.OpenKCC.Input;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Input
{
    /// <summary>
    /// Tests for the <see cref="nickmaltbie.OpenKCC.Input.BufferedInput"/> class.
    /// </summary>
    [TestFixture]
    public class BufferedInputTests : TestBase
    {
        private Gamepad gamepad;
        private InputActionMap actionMap;
        private InputAction testAction;
        private BufferedInput bufferedInput;
        private Mock<IUnityService> mockUnityService;

        [SetUp]
        public void SetUp()
        {
            (gamepad, _, actionMap) = base.SetupInputDevice<Gamepad>();
            testAction = actionMap.AddAction("testAction", InputActionType.Button, gamepad.aButton.path);

            bufferedInput = new BufferedInput();
            mockUnityService = new Mock<IUnityService>();
            mockUnityService.Setup(e => e.deltaTime).Returns(1.0f);
            bufferedInput.unityService = mockUnityService.Object;
            bufferedInput.inputActionReference = InputActionReference.Create(testAction);

            bufferedInput.cooldown = 1.0f;
            bufferedInput.bufferTime = 3.0f;

            testAction.Enable();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            InputActionReference.DestroyImmediate(bufferedInput.inputActionReference);
        }

        [Test]
        public void Validate_BufferedInput_False()
        {
            // Validate that input is disabled
            Assert.IsFalse(bufferedInput.Pressed);

            // set the value as true and update the input device
            Press(gamepad.aButton);
            bufferedInput.Update();
            Assert.IsTrue(bufferedInput.Pressed);
        }

        [Test]
        public void Validate_BufferedInput_BufferTime()
        {
            // set the value as true and update the input device
            Press(gamepad.aButton);
            bufferedInput.Update();
            Assert.IsTrue(bufferedInput.Pressed);

            Release(gamepad.aButton);
            bufferedInput.Update();
            Assert.IsTrue(bufferedInput.Pressed);

            mockUnityService.Setup(e => e.deltaTime).Returns(1000);
            bufferedInput.Update();
            Assert.IsFalse(bufferedInput.Pressed);
        }

        [Test]
        public void Validate_BufferedInput_Cooldown()
        {
            // set the value as true and update the input device
            Press(gamepad.aButton);
            bufferedInput.Update();
            Assert.IsTrue(bufferedInput.Pressed);

            bufferedInput.Reset();
            bufferedInput.cooldown = 3.0f;
            Assert.IsFalse(bufferedInput.Pressed);

            bufferedInput.Update();
            Assert.IsFalse(bufferedInput.Pressed);

            mockUnityService.Setup(e => e.deltaTime).Returns(1000);
            bufferedInput.Update();
            Assert.IsTrue(bufferedInput.Pressed);
        }
    }
}
