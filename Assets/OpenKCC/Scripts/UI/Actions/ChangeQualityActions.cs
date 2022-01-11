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

using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.UI.Actions
{
    /// <summary>
    /// Class to setup dropdown that controls various quality level 
    /// selection and save settings between player session loadings
    /// </summary>
    public class ChangeQualityActions : MonoBehaviour
    {
        /// <summary>
        /// String key to save quality level under
        /// </summary>
        public const string qualityLevelPlayerPrefKey = "qualityLevel";

        /// <summary>
        /// Dropdown that modifies the quality level
        /// </summary>
        public Dropdown qualityDropdown;

        public void Awake()
        {
            // Load saved settings
            var currentLevel = PlayerPrefs.GetInt(qualityLevelPlayerPrefKey, QualitySettings.GetQualityLevel());
            QualitySettings.SetQualityLevel(currentLevel);
            UnityEngine.Debug.Log($"Setting quality level to {QualitySettings.names[currentLevel]}");

            // Setup dropdown information
            qualityDropdown.ClearOptions();
            qualityDropdown.AddOptions(QualitySettings.names.ToList());
            qualityDropdown.SetValueWithoutNotify(currentLevel);
            qualityDropdown.onValueChanged.AddListener(OnQualityLevelChange);
        }

        /// <summary>
        /// Quality level change event
        /// </summary>
        /// <param name="level"></param>
        public void OnQualityLevelChange(int level)
        {
            QualitySettings.SetQualityLevel(level);
            PlayerPrefs.SetInt(qualityLevelPlayerPrefKey, level);
        }
    }
}
