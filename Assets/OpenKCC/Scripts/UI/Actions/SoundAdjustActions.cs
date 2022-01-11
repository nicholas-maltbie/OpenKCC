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
//

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
            for (var i = 0; i < settingsGroups.Length; i++)
            {
                var settingsGroup = settingsGroups[i];
                var group = settingsGroup.mixerGroup;
                var soundKey = SoundVolumePrefixPlayerPrefKey + settingsGroup.mixerGroup.name;

                settingsGroup.mixerGroup.audioMixer.GetFloat($"{group.name} Volume", out var startingVolume);
                // Set the slider to match the saved value
                settingsGroup.slider.SetValueWithoutNotify(GetSliderValue(startingVolume));
                // Update saved and current value on player input
                settingsGroup.slider.onValueChanged.AddListener(value =>
                {
                    var currentVolume = GetVolumeLevel(value);
                    group.audioMixer.SetFloat($"{group.name} Volume", currentVolume);
#if !UNITY_EDITOR
                    PlayerPrefs.SetFloat(soundKey, currentVolume);
#endif
                });
            }
        }
    }
}
