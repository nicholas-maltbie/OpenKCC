using System;
using System.Collections.Generic;
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
            int currentLevel = PlayerPrefs.GetInt(qualityLevelPlayerPrefKey, QualitySettings.GetQualityLevel());
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
