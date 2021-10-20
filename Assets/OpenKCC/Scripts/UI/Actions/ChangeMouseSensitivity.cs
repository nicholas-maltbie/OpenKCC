using nickmaltbie.OpenKCC.Character;
using UnityEngine;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.UI.Actions
{
    /// <summary>
    /// Actions to adjust mouse Sensitivity via sliders
    /// </summary>
    public class ChangeMouseSensitivity : MonoBehaviour
    {

        public const string mouseSensitivityPlayerPref = "MouseSensitivity";

        /// <summary>
        /// Slider to control mouse sensitivity
        /// </summary>
        public Slider slider;

        /// <summary>
        /// Power value for scaling mouse sensitivity between minimum and maximum
        /// </summary>
        public const float powerValue = 0.75f;

        /// <summary>
        /// Get the value on a slider from a given mouse sensitivity level
        /// </summary>
        /// <param name="sensitivity">Currently selected mouse sensitivity</param>
        /// <returns>The float position of a slider based on the mouse sensitivity</returns>
        public static float GetSliderValue(float sensitivity)
        {
            if (sensitivity < PlayerInputManager.minimumMouseSensitivity)
            {
                return 0.0f;
            }
            if (sensitivity > PlayerInputManager.maximumMouseSensitivity)
            {
                return 1.0f;
            }
            return Mathf.Pow((PlayerInputManager.mouseSensitivity - PlayerInputManager.minimumMouseSensitivity) /
                (PlayerInputManager.maximumMouseSensitivity - PlayerInputManager.minimumMouseSensitivity), powerValue);
        }

        /// <summary>
        /// Gets the mouse sensitivity from a given slider position
        /// </summary>
        /// <param name="sliderPosition">Slider position between 0 and 1</param>
        /// <returns>The mouse sensitivity between min sensitivity and max sensitivity</returns>
        public static float GetMouseSensitivity(float sliderPosition)
        {
            return Mathf.Pow(sliderPosition, 1 / powerValue) *
                (PlayerInputManager.maximumMouseSensitivity - PlayerInputManager.minimumMouseSensitivity) + PlayerInputManager.minimumMouseSensitivity;
        }

        /// <summary>
        /// Load saved values during startup
        /// </summary>
        public void Awake()
        {
            PlayerInputManager.mouseSensitivity = PlayerPrefs.GetFloat(
                mouseSensitivityPlayerPref, PlayerInputManager.mouseSensitivity);
            slider.SetValueWithoutNotify(GetSliderValue(PlayerInputManager.mouseSensitivity));
            // Update saved and current value on player input
            slider.onValueChanged.AddListener(value =>
            {
                PlayerInputManager.mouseSensitivity = GetMouseSensitivity(value);
                PlayerPrefs.SetFloat(mouseSensitivityPlayerPref, PlayerInputManager.mouseSensitivity);
            });
        }
    }
}
