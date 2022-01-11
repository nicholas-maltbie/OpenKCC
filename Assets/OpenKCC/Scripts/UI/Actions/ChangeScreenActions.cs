using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.UI.Actions
{
    /// <summary>
    /// Screen loading status information
    /// </summary>
    public static class ScreenLoading
    {
        /// <summary>
        /// Name of fullscreen mode
        /// </summary>
        public const string fullScreenModeName = "FullScreen";

        /// <summary>
        /// Name of windowed mode
        /// </summary>
        public const string windowedModeName = "Windowed";

        /// <summary>
        /// Name of borderless window mode
        /// </summary>
        public const string borderlessWindowModeName = "Borderless Windowed";

        /// <summary>
        /// Has the display been setup from saved settings
        /// </summary>
        public static bool setupDisplay = false;

        /// <summary>
        /// Windowed states 
        /// </summary>
        public static readonly string[] windowedDropdownText = { fullScreenModeName, windowedModeName, borderlessWindowModeName };


    }

    /// <summary>
    /// Change screen actions to modify resolution and quality settings
    /// </summary>
    public class ChangeScreenActions : MonoBehaviour
    {
        public struct ScreenSettingsGroup
        {
            public Resolution resolution;
            public FullScreenMode fullScreenMode;
            public int monitor;
        }

        private const string resolutionWidthPlayerPrefKey = "ResolutionWidth";
        private const string resolutionHeightPlayerPrefKey = "ResolutionHeight";
        private const string resolutionRefreshRatePlayerPrefKey = "RefreshRate";
        private const string fullScreenPlayerPrefKey = "FullScreen";
        private const string targetDisplayPlayerPrefKey = "TargetDisplay";
        private const string vsyncPlayerPrefKey = "vsync";

        /// <summary>
        /// Dropdown with windowed/fullScreen/borderless options
        /// </summary>
        public Dropdown windowedDropdown;

        /// <summary>
        /// Options to set screen resolution
        /// </summary>
        public Dropdown resolutionDropdown;

        /// <summary>
        /// Options to set selected monitor/display
        /// </summary>
        public Dropdown displayDropdown;

        /// <summary>
        /// Toggle to control vysnc settings
        /// </summary>
        public Toggle vsyncToggle;

        /// <summary>
        /// Currently selected fullscreen mode
        /// </summary>
        public FullScreenMode currentFullScreen { get; private set; }
        /// <summary>
        /// Currently selected resolution
        /// </summary>
        public Resolution currentResolution;
        /// <summary>
        /// Set of supported screen resolutions
        /// </summary>
        private Resolution[] resolutions;
        /// <summary>
        /// Currently selected display
        /// </summary>
        public int currentDisplay { get; private set; }

        /// <summary>
        /// Settings panel (disabled when confirming operations)
        /// </summary>
        public CanvasGroup settingsPage;

        /// <summary>
        /// Confirmation panel (enabled when confirming operations)
        /// </summary>
        public CanvasGroup confirmPanel;

        /// <summary>
        /// Settings menu controller (disable when the confirmation dialog is open)
        /// </summary>
        public MenuController settingsMenuController;

        /// <summary>
        /// Accept confirm dialog button
        /// </summary>
        public Button confirmDialogYes;

        /// <summary>
        /// Reject confirm dialog button
        /// </summary>
        public Button confirmDialogNo;

        /// <summary>
        /// Text for the confirm dialog information
        /// </summary>
        public UnityEngine.UI.Text confirmDialogText;

        /// <summary>
        /// Get the fullscreen mode integer based on a selected fullscreen mode
        /// </summary>
        /// <param name="mode">Fullscreen mode to load</param>
        /// <returns>Index of fullscreen mode in the windowed dropdown selector</returns>
        public static int GetFullScreenModeDropdownIndex(FullScreenMode mode)
        {
            for (int i = 0; i < ScreenLoading.windowedDropdownText.Length; i++)
            {
                if (GetFullScreenMode(ScreenLoading.windowedDropdownText[i]) == mode)
                {
                    return i;
                }
            }
            return 0;
        }

        /// <summary>
        /// Get the selected fullscreen mode from a selected fullscreen name
        /// </summary>
        /// <param name="selectedMode">Name of selected mode</param>
        /// <returns>Fullscreen mode associated with this name</returns>
        public static FullScreenMode GetFullScreenMode(string selectedMode)
        {
            switch (selectedMode)
            {
                case ScreenLoading.fullScreenModeName:
                    return FullScreenMode.ExclusiveFullScreen;
                case ScreenLoading.windowedModeName:
                    return FullScreenMode.Windowed;
                case ScreenLoading.borderlessWindowModeName:
                    return FullScreenMode.FullScreenWindow;
                default:
                    return FullScreenMode.ExclusiveFullScreen;
            }
        }

        public void Awake()
        {
            // Skip setup on WebGL Build
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                return;
            }

            // Load settings if it hasn't already been configured
            if (!ScreenLoading.setupDisplay)
            {
                LoadSettings();
            }

            // Setup dropdowns
            SetupFullScreenDropdown();
            SetupResolutionDropdown();
            SetupDisplayDropdown();
            SetupVsyncToggle();
        }

        /// <summary>
        /// Setup the dropdown for the fullscreen settings
        /// </summary>
        private void SetupFullScreenDropdown()
        {
            windowedDropdown.ClearOptions();
            windowedDropdown.AddOptions(ScreenLoading.windowedDropdownText.ToList());
            windowedDropdown.onValueChanged.AddListener(SetFullScreen);
            windowedDropdown.SetValueWithoutNotify(GetFullScreenModeDropdownIndex(currentFullScreen));
            windowedDropdown.RefreshShownValue();
        }

        /// <summary>
        /// Setup the vsync toggle
        /// </summary>
        private void SetupVsyncToggle()
        {
            vsyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount == 1);
            vsyncToggle.onValueChanged.AddListener(SetVsync);
        }

        /// <summary>
        /// Setup the dropdown for the display settings
        /// </summary>
        private void SetupDisplayDropdown()
        {
            displayDropdown.ClearOptions();
            displayDropdown.AddOptions(
                Enumerable.Range(1, Display.displays.Length)
                .Select(i => $"Monitor {i}").ToList());
            displayDropdown.onValueChanged.AddListener(SetMonitor);
            displayDropdown.SetValueWithoutNotify(currentDisplay < Display.displays.Length ? currentDisplay : 0);
            displayDropdown.RefreshShownValue();
        }

        /// <summary>
        /// Filter duplicate resolutions from a set of resolutions. Removes refresh rate from the data
        /// </summary>
        /// <param name="resolutions">Various resolution options</param>
        /// <returns>Filters duplicates with the same width and height</returns>
        public static Resolution[] FilterResolutions(Resolution[] resolutions)
        {
            HashSet<Resolution> resolutionSet = new HashSet<Resolution>();
            resolutions.ToList().ForEach(resolution =>
                resolutionSet.Add(new Resolution
                {
                    width = resolution.width,
                    height = resolution.height
                })
            );
            return resolutionSet.OrderBy(i => new Tuple<int, int>(-i.width, -i.height)).ToArray();
        }

        /// <summary>
        /// Refresh the list of resolutions from the Screen settings
        /// </summary>
        private void RefreshResolutionDropdown()
        {
            this.resolutions = Screen.resolutions.OrderBy(i => new Tuple<int, int>(-i.width, -i.height)).ToArray();
            // Default resolutions if none are provided
            this.resolutions = this.resolutions.Length == 0 ? new Resolution[] {
                new Resolution { width = 1920, height = 1080, refreshRate = 60 },
                new Resolution { width = 1280, height = 720, refreshRate = 60 }
            } : this.resolutions;
            this.resolutions = FilterResolutions(this.resolutions);
            List<string> options = new List<string>();
            for (int i = 0; i < resolutions.Length; i++)
            {
                string option = resolutions[i].width + " x " + resolutions[i].height;
                options.Add(option);
                // Set selected resolution to current (or element 0 if not specificed)
                if (i == 0 || Mathf.Approximately(resolutions[i].width, currentResolution.width)
                    && Mathf.Approximately(resolutions[i].height, currentResolution.height))
                {
                    resolutionDropdown.SetValueWithoutNotify(i);
                }
            }

            resolutionDropdown.ClearOptions();
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.RefreshShownValue();
        }

        public IEnumerator OpenConfirmChangesDialog(int timeout = 15)
        {
            return OpenConfirmChangesDialog(new ScreenSettingsGroup
            {
                resolution = currentResolution,
                fullScreenMode = currentFullScreen,
                monitor = currentDisplay
            }, timeout);
        }

        public IEnumerator OpenConfirmChangesDialog(ScreenSettingsGroup previousSettings,
            int timeout = 15)
        {
            // Disable the current panel
            settingsPage.blocksRaycasts = false;
            settingsPage.interactable = false;
            settingsPage.alpha = 0.8f;
            // Change to confirm graphics screen
            confirmPanel.blocksRaycasts = true;
            confirmPanel.interactable = true;
            confirmPanel.alpha = 1.0f;

            // Wait for the user to answer (either by button or by timeout)
            bool answered = false;
            // Add listeners to yes and no buttons on confirm dialog
            confirmDialogYes.onClick.AddListener(() =>
            {
                CloseConfirmChangesDialog(false, previousSettings);
                answered = true;
            });
            confirmDialogNo.onClick.AddListener(() =>
            {
                CloseConfirmChangesDialog(true, previousSettings);
                answered = true;
            });
            // Don't allow the user to exit the settings panel until this is confirmed
            settingsMenuController.allowInputChanges = false;

            // Wait for timeout... or exit early if the user has answered the question
            for (int i = timeout; i >= 0 && !answered; i--)
            {
                // Update the shown value
                confirmDialogText.text = i.ToString();
                // Wait for a second
                yield return new WaitForSeconds(1.0f);
            }

            // If they timed out, close the confirm dialog and revert the settings
            if (!answered)
            {
                CloseConfirmChangesDialog(true, previousSettings);
            }
            // Remove listeners from the confirm dialog
            confirmDialogYes.onClick.RemoveAllListeners();
            confirmDialogNo.onClick.RemoveAllListeners();
            // Reset the settings panel controls to allow user to leave panel
            settingsMenuController.allowInputChanges = true;
        }

        private void CloseConfirmChangesDialog(bool revert, ScreenSettingsGroup previousSettings)
        {
            if (revert)
            {
                // Revert to previous settings
                currentResolution = previousSettings.resolution;
                currentFullScreen = previousSettings.fullScreenMode;
                currentDisplay = previousSettings.monitor;
                // Change display to match selected settings
                UpdateDisplayInfo();

                // Refresh all shown values in dropdowns
                RefreshResolutionDropdown();
                windowedDropdown.SetValueWithoutNotify(GetFullScreenModeDropdownIndex(currentFullScreen));
                windowedDropdown.RefreshShownValue();
                displayDropdown.SetValueWithoutNotify(currentDisplay);
                displayDropdown.RefreshShownValue();
            }
            // Close the confirm dialog
            // Enable the settings panel
            settingsPage.interactable = true;
            settingsPage.blocksRaycasts = true;
            settingsPage.alpha = 1.0f;
            // Close the confirm panel
            confirmPanel.interactable = false;
            confirmPanel.blocksRaycasts = false;
            confirmPanel.alpha = 0.0f;
        }

        /// <summary>
        /// Setup the resolution dropdown on initial startup
        /// </summary>
        private void SetupResolutionDropdown()
        {
            RefreshResolutionDropdown();
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }

        /// <summary>
        /// Load saved settings from player preferences
        /// </summary>
        private void LoadSettings()
        {
            currentResolution = new Resolution();
            currentResolution.width = PlayerPrefs.GetInt(resolutionWidthPlayerPrefKey, Screen.currentResolution.width);
            currentResolution.height = PlayerPrefs.GetInt(resolutionHeightPlayerPrefKey, Screen.currentResolution.height);
            currentResolution.refreshRate = PlayerPrefs.GetInt(resolutionRefreshRatePlayerPrefKey, Screen.currentResolution.refreshRate);
            currentFullScreen = (FullScreenMode)PlayerPrefs.GetInt(fullScreenPlayerPrefKey, (int)Screen.fullScreenMode);
            currentDisplay = PlayerPrefs.GetInt(targetDisplayPlayerPrefKey, 0);
            QualitySettings.vSyncCount = PlayerPrefs.GetInt(vsyncPlayerPrefKey, QualitySettings.vSyncCount);

            UpdateDisplayInfo();
        }

        public void SetVsync(bool isChecked)
        {
            QualitySettings.vSyncCount = isChecked ? 1 : 0;
            PlayerPrefs.SetInt(vsyncPlayerPrefKey, QualitySettings.vSyncCount);
        }

        public void SetResolution(int resolutionIndex)
        {
            IEnumerator confirm = OpenConfirmChangesDialog();

            currentResolution = resolutions[resolutionIndex];
            UpdateDisplayInfo();
            StartCoroutine(confirm);
        }

        public void SetFullScreen(int fullScreenIndex)
        {
            IEnumerator confirm = OpenConfirmChangesDialog();

            currentFullScreen = GetFullScreenMode(windowedDropdown.options[windowedDropdown.value].text.Trim());
            UpdateDisplayInfo();
            StartCoroutine(confirm);
        }

        public void SetMonitor(int targetMonitor)
        {
            IEnumerator confirm = OpenConfirmChangesDialog();

            this.currentDisplay = targetMonitor;
            UpdateDisplayInfo();

            // Update the dropdowns for resolution based on new screen
            RefreshResolutionDropdown();
            StartCoroutine(confirm);
        }

        public void UpdateDisplayInfo()
        {
#if UNITY_EDITOR
            UnityEngine.Debug.Log($"Setting {currentFullScreen} with resolution {currentResolution.width}x{currentResolution.height} * {currentResolution.refreshRate}, monitor {currentDisplay}");
#endif
            Screen.SetResolution(currentResolution.width, currentResolution.height, currentFullScreen, currentResolution.refreshRate);
            // Then set target monitor
            // This will be added in 2021.2 so will have that to look forward to in future
            // Example: https://github.com/Unity-Technologies/DesktopSamples/tree/master/MoveWindowSample
            //   Screen.MoveMainWindowTo(display, targetCoordinates);
            // Related forum thread https://forum.unity.com/threads/switch-monitor-at-runtime.501336/
            // Will update and add feature since it should be released soon

            PlayerPrefs.SetInt(resolutionWidthPlayerPrefKey, currentResolution.width);
            PlayerPrefs.SetInt(resolutionHeightPlayerPrefKey, currentResolution.height);
            PlayerPrefs.SetInt(resolutionRefreshRatePlayerPrefKey, currentResolution.refreshRate);
            PlayerPrefs.SetInt(fullScreenPlayerPrefKey, (int)currentFullScreen);
            PlayerPrefs.SetInt(targetDisplayPlayerPrefKey, currentDisplay);
        }
    }
}
