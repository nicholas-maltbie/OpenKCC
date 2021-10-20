using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.UI.Actions
{
    /// <summary>
    /// Actions to adjust sound levels via sliders
    /// </summary>
    public class SoundAdjustActions : MonoBehaviour
    {
        /// <summary>
        /// Prefix key for sound volume.
        /// </summary>
        public const string SoundVolumePrefixPlayerPrefKey = "SoundVolume";

        /// <summary>
        /// Groups of sliders and sound mixer groups they modify
        /// </summary>
        public AudioMixerSettingsGroup[] settingsGroups;

        /// <summary>
        /// Object containing both a mixer group and audio slider
        /// </summary>
        [Serializable]
        public struct AudioMixerSettingsGroup
        {
            /// <summary>
            /// Associated audio mixer group
            /// </summary>
            public AudioMixerGroup mixerGroup;
            /// <summary>
            /// Slider to control audio mixer group level
            /// </summary>
            public Slider slider;
        }
        /// <summary>
        /// Minimum volume for sliders
        /// </summary>
        public const float minVolume = -20;
        /// <summary>
        /// Maximum volume for sliders
        /// </summary>
        public const float maxVolume = 10;
        /// <summary>
        /// Volume of slider when muted
        /// </summary>
        public const float mutedVolume = -80f;

        /// <summary>
        /// Power value when scaling volume 
        /// </summary>
        public const float powerValue = 2.0f;

        /// <summary>
        /// Get the value on a slider from a given volume level
        /// </summary>
        /// <param name="volumeLevel">Volume level on the slider</param>
        /// <returns>The float position of a slider based on the volume level</returns>
        public static float GetSliderValue(float volumeLevel)
        {
            if (volumeLevel == mutedVolume || volumeLevel < minVolume)
            {
                return 0.0f;
            }
            if (volumeLevel > maxVolume)
            {
                return 1.0f;
            }
            return Mathf.Pow((volumeLevel - minVolume) / (maxVolume - minVolume), powerValue);
        }

        /// <summary>
        /// Gets the volume level from a given slider position
        /// </summary>
        /// <param name="sliderPosition">Slider position between 0 and 1</param>
        /// <returns>The volume level between min volume and max volume</returns>
        public static float GetVolumeLevel(float sliderPosition)
        {
            if (sliderPosition == 0.0f)
            {
                return mutedVolume;
            }
            return Mathf.Pow(sliderPosition, 1 / powerValue) * (maxVolume - minVolume) + minVolume;
        }

        public void Start()
        {
            // Setup sliders
            for (int i = 0; i < settingsGroups.Length; i++)
            {
                AudioMixerSettingsGroup settingsGroup = settingsGroups[i];
                AudioMixerGroup group = settingsGroup.mixerGroup;
                string soundKey = SoundVolumePrefixPlayerPrefKey + settingsGroup.mixerGroup.name;

                settingsGroup.mixerGroup.audioMixer.GetFloat($"{group.name} Volume", out float startingVolume);
                // Set the slider to match the saved value
                settingsGroup.slider.SetValueWithoutNotify(GetSliderValue(startingVolume));
                // Update saved and current value on player input
                settingsGroup.slider.onValueChanged.AddListener(value =>
                {
                    float currentVolume = GetVolumeLevel(value);
                    group.audioMixer.SetFloat($"{group.name} Volume", currentVolume);
#if !UNITY_EDITOR
                    PlayerPrefs.SetFloat(soundKey, currentVolume);
#endif
                });
            }
        }
    }
}
