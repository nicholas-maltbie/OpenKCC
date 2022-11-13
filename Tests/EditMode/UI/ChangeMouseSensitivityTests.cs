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

using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.UI.Actions;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.Tests.EditMode.UI
{
    /// <summary>
    /// Basic tests for ChangeMouseSensitivity in edit mode.
    /// </summary>
    [TestFixture]
    public class ChangeMouseSensitivityTests : TestBase
    {
        [Test]
        public void Verify_ChangeMouseSensitivity_ValueBounds()
        {
            Assert.AreEqual(0.0f, ChangeMouseSensitivity.GetSliderValue(PlayerInputUtils.minimumMouseSensitivity - 10.0f));
            Assert.AreEqual(1.0f, ChangeMouseSensitivity.GetSliderValue(PlayerInputUtils.maximumMouseSensitivity + 10.0f));
        }

        [Test]
        public void Verify_ChangeMouseSensitivityActions()
        {
            ChangeMouseSensitivity mouseSensitivity = CreateGameObject().AddComponent<ChangeMouseSensitivity>();
            Slider slider = CreateGameObject().AddComponent<Slider>();

            mouseSensitivity.slider = slider;

            slider.value = 0.0f;

            mouseSensitivity.Awake();
            Assert.AreEqual(PlayerInputUtils.mouseSensitivity, PlayerPrefs.GetFloat(
                ChangeMouseSensitivity.mouseSensitivityPlayerPref, PlayerInputUtils.DefaultMouseSensitivity));
            Assert.AreEqual(slider.value, ChangeMouseSensitivity.GetSliderValue(PlayerInputUtils.mouseSensitivity));

            // Set binding to some new value
            float sliderValue = 1 / Mathf.PI;
            slider.value = sliderValue;
            TestUtils.AssertInBounds(PlayerInputUtils.mouseSensitivity, ChangeMouseSensitivity.GetMouseSensitivity(sliderValue));
            TestUtils.AssertInBounds(PlayerInputUtils.mouseSensitivity, PlayerPrefs.GetFloat(
                ChangeMouseSensitivity.mouseSensitivityPlayerPref, ChangeMouseSensitivity.GetMouseSensitivity(sliderValue)));

            // Change slider with value update
            PlayerInputUtils.mouseSensitivity = PlayerInputUtils.minimumMouseSensitivity;
            mouseSensitivity.UpdateDisplay();
            Assert.AreEqual(slider.value, ChangeMouseSensitivity.GetSliderValue(PlayerInputUtils.minimumMouseSensitivity));

            PlayerInputUtils.mouseSensitivity = PlayerInputUtils.maximumMouseSensitivity;
            mouseSensitivity.UpdateDisplay();
            Assert.AreEqual(slider.value, ChangeMouseSensitivity.GetSliderValue(PlayerInputUtils.maximumMouseSensitivity));

            mouseSensitivity.ResetBinding();
            Assert.AreEqual(PlayerInputUtils.mouseSensitivity, PlayerInputUtils.DefaultMouseSensitivity);
            Assert.IsFalse(PlayerPrefs.HasKey(ChangeMouseSensitivity.mouseSensitivityPlayerPref));
        }
    }
}
