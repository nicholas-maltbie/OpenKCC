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
using nickmaltbie.ScreenManager.Actions;
using UnityEngine;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.UI.Actions
{
    /// <summary>
    /// Actions to adjust mouse Sensitivity via sliders
    /// </summary>
    public class ChangeMouseSensitivity : MonoBehaviour, IBindingControl
    {
        /// <summary>
        /// String key for saving mouse sensitivity to player preferences.
        /// </summary>
        public const string mouseSensitivityPlayerPref = "MouseSensitivity";

        /// <summary>
        /// Power value for scaling mouse sensitivity between minimum and maximum
        /// </summary>
        public const float powerValue = 0.75f;

        /// <summary>
        /// Slider to control mouse sensitivity.
        /// </summary>
        [SerializeField]
        [Tooltip("Slider to control mouse sensitivity.")]
        internal Slider slider;

        /// <summary>
        /// Get the value on a slider from a given mouse sensitivity level
        /// </summary>
        /// <param name="sensitivity">Currently selected mouse sensitivity</param>
        /// <returns>The float position of a slider based on the mouse sensitivity</returns>
        public static float GetSliderValue(float sensitivity)
        {
            if (sensitivity < PlayerInputUtils.minimumMouseSensitivity)
            {
                return 0.0f;
            }

            if (sensitivity > PlayerInputUtils.maximumMouseSensitivity)
            {
                return 1.0f;
            }

            return Mathf.Pow((PlayerInputUtils.mouseSensitivity - PlayerInputUtils.minimumMouseSensitivity) /
                (PlayerInputUtils.maximumMouseSensitivity - PlayerInputUtils.minimumMouseSensitivity), powerValue);
        }

        /// <summary>
        /// Gets the mouse sensitivity from a given slider position
        /// </summary>
        /// <param name="sliderPosition">Slider position between 0 and 1</param>
        /// <returns>The mouse sensitivity between min sensitivity and max sensitivity</returns>
        public static float GetMouseSensitivity(float sliderPosition)
        {
            return Mathf.Pow(sliderPosition, 1 / powerValue) *
                (PlayerInputUtils.maximumMouseSensitivity - PlayerInputUtils.minimumMouseSensitivity) + PlayerInputUtils.minimumMouseSensitivity;
        }

        /// <summary>
        /// Load saved values during startup
        /// </summary>
        public void Awake()
        {
            PlayerInputUtils.mouseSensitivity = PlayerPrefs.GetFloat(
                mouseSensitivityPlayerPref, PlayerInputUtils.DefaultMouseSensitivity);
            slider.SetValueWithoutNotify(GetSliderValue(PlayerInputUtils.mouseSensitivity));
            // Update saved and current value on player input
            slider.onValueChanged.AddListener(value =>
            {
                PlayerInputUtils.mouseSensitivity = GetMouseSensitivity(value);
                PlayerPrefs.SetFloat(mouseSensitivityPlayerPref, PlayerInputUtils.mouseSensitivity);
            });
        }

        /// <inheritdoc/>
        public void ResetBinding()
        {
            PlayerPrefs.DeleteKey(mouseSensitivityPlayerPref);
            PlayerInputUtils.mouseSensitivity = PlayerInputUtils.DefaultMouseSensitivity;
        }

        /// <inheritdoc/>
        public void UpdateDisplay()
        {
            slider.SetValueWithoutNotify(GetSliderValue(PlayerInputUtils.mouseSensitivity));
        }
    }
}
