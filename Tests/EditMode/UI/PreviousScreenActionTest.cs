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

using System.Collections.Generic;
using nickmaltbie.OpenKCC.UI.Actions;
using nickmaltbie.ScreenManager;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.Tests.EditMode.UI
{
    [TestFixture]
    public class PreviousScreenActionTest : TestBase
    {
        [Test]
        public void Verify_PreviousScreenAction()
        {
            GameScreen screen1 = CreateGameObject().AddComponent<GameScreen>();
            GameScreen screen2 = CreateGameObject().AddComponent<GameScreen>();
            GameScreen screen3 = CreateGameObject().AddComponent<GameScreen>();

            screen1.gameObject.name = "1";
            screen2.gameObject.name = "2";
            screen3.gameObject.name = "3";

            UIManager uiManager = CreateGameObject().AddComponent<UIManager>();
            uiManager.screenPrefabs = new List<GameScreen>(new[] { screen1, screen2, screen3 });
            uiManager.Start();

            uiManager.SetScreen(screen2.gameObject.name);
            uiManager.SetScreen(screen3.gameObject.name);
            uiManager.SetScreen(screen1.gameObject.name);
            uiManager.SetScreen(screen2.gameObject.name);
            uiManager.SetScreen(screen3.gameObject.name);

            GameObject go = CreateGameObject();

            // Setup input action reference.
            (Gamepad gamepad, _, InputActionMap actionMap) = SetupInputDevice<Gamepad>();
            InputAction testAction = actionMap.AddAction("testAction", InputActionType.Button, gamepad.aButton.path, interactions: "press");
            PreviousScreenAction previousScreen = go.AddComponent<PreviousScreenAction>();
            MenuController controller = previousScreen.GetComponent<MenuController>();
            previousScreen.action = InputActionReference.Create(testAction);
            testAction.Enable();

            controller.actionDelay = Mathf.NegativeInfinity;

            previousScreen.OnScreenLoaded();
            Assert.AreEqual(screen3.gameObject.name, uiManager.CurrentScreen);

            PressAndRelease(gamepad.aButton);
            Assert.AreEqual(screen2.gameObject.name, uiManager.CurrentScreen);

            PressAndRelease(gamepad.aButton);
            Assert.AreEqual(uiManager.CurrentScreen, screen1.gameObject.name);

            previousScreen.OnScreenUnloaded();
            PressAndRelease(gamepad.aButton);
            Assert.AreEqual(uiManager.CurrentScreen, screen1.gameObject.name);

            PressAndRelease(gamepad.aButton);
            Assert.AreEqual(uiManager.CurrentScreen, screen1.gameObject.name);

            previousScreen.OnScreenLoaded();
            PressAndRelease(gamepad.aButton);
            Assert.AreEqual(uiManager.CurrentScreen, screen3.gameObject.name);
        }
    }
}
