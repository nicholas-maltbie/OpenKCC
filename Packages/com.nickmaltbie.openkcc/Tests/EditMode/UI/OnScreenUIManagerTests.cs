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

using nickmaltbie.OpenKCC.UI.Actions;
using nickmaltbie.ScreenManager.Events;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.Tests.EditMode.UI
{
    [TestFixture]
    public class OnScreenUIManagerTests : TestBase
    {
        [Test]
        public void Verify_OnScreenUIManager([Values] bool state)
        {
            (Gamepad gamepad, _, InputActionMap actionMap) = SetupInputDevice<Gamepad>();
            InputAction testAction = actionMap.AddAction("testAction", InputActionType.Button, gamepad.aButton.path, interactions: "press");

            GameObject go = CreateGameObject();
            CursorStateOnMenuLoad cursorState = go.AddComponent<CursorStateOnMenuLoad>();
            OnScreenUIManager manager = go.AddComponent<OnScreenUIManager>();

            OnScreenUIManager.DefaultState = state;
            manager.lookAction = InputActionReference.Create(testAction);

            manager.Awake();
            Assert.AreEqual(OnScreenUIManager.Instance, manager);

            manager.Start();
            Assert.AreEqual(!state, cursorState.cursorVisible);
            Assert.AreEqual(state ? CursorLockMode.Locked : CursorLockMode.None, cursorState.cursorLockMode);

            manager.UpdateState(state);
            Assert.AreEqual(state, cursorState.cursorVisible);
            Assert.AreEqual(state ? CursorLockMode.None : CursorLockMode.Locked, cursorState.cursorLockMode);

            manager.UpdateState(!state);
            Assert.AreEqual(!state, cursorState.cursorVisible);
            Assert.AreEqual(state ? CursorLockMode.Locked : CursorLockMode.None, cursorState.cursorLockMode);
        }
    }
}
