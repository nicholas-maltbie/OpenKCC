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

using System.Linq;
using nickmaltbie.OpenKCC.UI.Actions;
using nickmaltbie.ScreenManager;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.Tests.EditMode.UI
{
    [TestFixture]
    public class SelectiveRebindInputTests
    {
        private SelectiveRebindInput rebinding;
        private Keyboard keyboard;
        private InputActionAsset inputActionAsset;

        [SetUp]
        public void SetUp()
        {
            rebinding = new GameObject().AddComponent<SelectiveRebindInput>();

            // Create a sample player input to override
            keyboard = InputSystem.AddDevice<Keyboard>();
            PlayerInput input = rebinding.gameObject.AddComponent<PlayerInput>();
            inputActionAsset = ScriptableObject.CreateInstance<InputActionAsset>();
            InputAction testAction = inputActionAsset.AddActionMap("testMap").AddAction("testAction", InputActionType.Button, keyboard.qKey.path, interactions: "Hold");
            input.actions = inputActionAsset;

            // Setup rebinding object
            rebinding.inputAction = InputActionReference.Create(testAction);
            rebinding.menuController = rebinding.gameObject.AddComponent<MenuController>();
            rebinding.bindingDisplayNameText = rebinding.gameObject.AddComponent<UnityEngine.UI.Text>();
            rebinding.startRebinding = new GameObject().AddComponent<Button>();
            rebinding.waitingForInputObject = new GameObject();
            rebinding.startRebinding.transform.parent = rebinding.transform;
            rebinding.waitingForInputObject.transform.parent = rebinding.transform;

            rebinding.gameObject.SetActive(true);

            // Test by reading the settings
            rebinding.Awake();
            rebinding.Start();
        }

        [TearDown]
        public void TearDown()
        {
            GameObject.DestroyImmediate(inputActionAsset);
            GameObject.DestroyImmediate(rebinding.gameObject);

            // Remove rebinding override
            InputSystem.RemoveDevice(keyboard);
            PlayerPrefs.DeleteKey(rebinding.InputMappingKey);
        }

        [Test]
        public void TestRebindingUpdateDisplay()
        {
            rebinding.ResetBinding();
            rebinding.UpdateDisplay();
            string previousName = rebinding.GetKeyReadableName();
            Assert.IsTrue(rebinding.bindingDisplayNameText.text == rebinding.GetKeyReadableName());

            // Simulate hitting key to override binding path
            rebinding.inputAction.action.ApplyBindingOverride(0, keyboard.rKey.path);
            rebinding.UpdateDisplay();
            Assert.IsTrue(rebinding.bindingDisplayNameText.text == rebinding.GetKeyReadableName());

            string overrideName = rebinding.GetKeyReadableName();

            Assert.IsTrue(previousName != overrideName);
        }

        [Test]
        public void TestRebindingReset()
        {
            // Simulate hitting key to override binding path
            rebinding.inputAction.action.ApplyBindingOverride(0, keyboard.rKey.path);

            Assert.IsTrue(rebinding.inputAction.action.bindings.Any(
                binding => binding.overridePath == keyboard.rKey.path
            ));

            rebinding.ResetBinding();

            // Assert that no override path has been set
            Assert.IsTrue(string.IsNullOrEmpty(rebinding.inputAction.action.bindings[0].overridePath));
        }

        [Test]
        public void TestRebindInputSettingsCancel()
        {
            // Start a test rebinding
            rebinding.startRebinding.onClick?.Invoke();

            // Cancel the test rebinding
            rebinding.rebindingOperation.Cancel();

            // Assert that no override path has been set
            Assert.IsTrue(string.IsNullOrEmpty(rebinding.inputAction.action.bindings[0].overridePath));
        }

        [Test]
        public void TestRebindingInputLoadSettings()
        {
            // Save a sample rebinding information
            PlayerPrefs.SetString(rebinding.InputMappingKey, keyboard.eKey.path);

            // Test by reading the settings
            rebinding.Awake();
            rebinding.Start();

            Assert.IsTrue(rebinding.inputAction.action.bindings.Any(
                binding => binding.overridePath == keyboard.eKey.path
            ));
        }

        [Test]
        public void TestRebindInputRebindAction()
        {
            // Start a test rebinding
            rebinding.startRebinding.onClick?.Invoke();

            // Simulate hitting key to override binding path
            rebinding.inputAction.action.ApplyBindingOverride(0, keyboard.rKey.path);

            Assert.IsTrue(rebinding.rebindingOperation != null);

            // End the test rebinding
            rebinding.rebindingOperation.Complete();

            Assert.IsTrue(rebinding.inputAction.action.bindings.Any(
                binding => binding.overridePath == keyboard.rKey.path
            ));
        }
    }
}
