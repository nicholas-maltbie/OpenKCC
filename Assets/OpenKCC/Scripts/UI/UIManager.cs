using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace nickmaltbie.OpenKCC.UI
{
    /// <summary>
    /// Class to hold arguments for showing a new screen
    /// </summary>
    public class RequestScreenChangeEventArgs : EventArgs
    {
        /// <summary>
        /// String identifier for the new screen to load
        /// </summary>
        public string newScreen;
    }

    /// <summary>
    /// Class to hold screen change in state
    /// </summary>
    public class ScreenChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Name of the previous screen shown
        /// </summary>
        public string oldScreen;

        /// <summary>
        /// Name of the new screen being changed to
        /// </summary>
        public string newScreen;
    }

    /// <summary>
    /// Class to manager various UI Screens
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        /// <summary>
        /// Events for requesting a screen change
        /// </summary>
        public static event EventHandler<RequestScreenChangeEventArgs> RequestScreenChange;

        /// <summary>
        /// Events for when a screen change has ocurred
        /// </summary>
        public static event EventHandler<ScreenChangeEventArgs> ScreenChangeOccur;

        /// <summary>
        /// Globally created instance of the UIManager
        /// </summary>
        public static UIManager Instance;

        /// <summary>
        /// Various screens to add into the scene
        /// </summary>
        public List<GameScreen> screenPrefabs = new List<GameScreen>();

        /// <summary>
        /// Index of the first screen to show
        /// </summary>
        public int initialScreen;

        /// <summary>
        /// Name of the current screen being displayed
        /// </summary>
        public string CurrentScreen { get; private set; }

        /// <summary>
        /// Lookup table of name of screen to screen being shown
        /// </summary>
        private Dictionary<string, GameObject> screenLookup;

        /// <summary>
        /// Sequence of all screens used by the player
        /// </summary>
        private LinkedList<string> screenSequence = new LinkedList<string>();

        /// <summary>
        /// Input actions used by UI Module
        /// </summary>
        public InputActionAsset inputActions;

        /// <summary>
        /// Maximum number of screen changes that can be saved
        /// </summary>
        public int maxScreenHistory = 100;

        /// <summary>
        /// Clear out history of all visited screens (except for current screen)
        /// </summary>
        public void ClearHistory()
        {
            string current = screenSequence.Last.Value;
            screenSequence.Clear();
            screenSequence.AddLast(current);
        }

        public IEnumerator DestorySelf()
        {
            GameObject.Destroy(gameObject);
            yield return null;
        }

        public void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                // Only let one exist
                StartCoroutine(DestorySelf());
                return;
            }

            if (this.screenPrefabs.Count == 0)
            {
                UnityEngine.Debug.Log("No valid screens to display for UIManager");
                return;
            }

            if (this.initialScreen < 0 || this.initialScreen > this.screenPrefabs.Count)
            {
                UnityEngine.Debug.Log($"Initial Screen {this.initialScreen} invalid, defaulting to screen {0}");
                this.initialScreen = 0;
            }

            // Setup dictionary of screens
            this.screenLookup = new Dictionary<string, GameObject>();
            // GetComponent<InputSystemUIInputModule>().actionsAsset = inputActions;
            for (int idx = 0; idx < this.screenPrefabs.Count; idx++)
            {
                string screenName = screenPrefabs[idx].name;
                // instantiate a copy of each screen and set all to disabled except current screen
                UnityEngine.Debug.Log(screenPrefabs[idx].name);
                GameObject screen = GameObject.Instantiate(screenPrefabs[idx].gameObject);
                // Set object parent to this for more organized hierarchy
                screen.transform.SetParent(this.transform, worldPositionStays: false);
                this.screenLookup[screenName] = screen;
                GameScreen gameScreen = screen.GetComponent<GameScreen>();
                screen.SetActive(true);
                if (idx == this.initialScreen)
                {
                    this.CurrentScreen = screenName;
                    screenSequence.AddLast(screenName);
                    gameScreen.DisplayScreen();
                }
                else
                {
                    gameScreen.HideScreen();
                }
            }

            // Setup listening to event queue
            UIManager.RequestScreenChange += this.HandleScreenRequest;
        }

        public void OnDestroy()
        {
            if (UIManager.Instance == this)
            {
                UIManager.Instance = null;
            }
            UIManager.RequestScreenChange -= this.HandleScreenRequest;
        }

        /// <summary>
        /// Handle a request to change screens
        /// </summary>
        /// <param name="sender">sender of the event</param>
        /// <param name="eventArgs">arguments of screen change</param>
        public void HandleScreenRequest(object sender, RequestScreenChangeEventArgs eventArgs)
        {
            this.SetScreen(eventArgs.newScreen);
        }

        /// <summary>
        /// Sets this screen to be displayed
        /// </summary>
        /// <param name="screenName">Name of the screen to display</param>
        public void SetScreen(string screenName)
        {
            if (!this.screenLookup.ContainsKey(screenName))
            {
                UnityEngine.Debug.Log($"Screen name {screenName} not recognized as new screen");
                return;
            }
            if (screenName == this.CurrentScreen)
            {
                // no change, do nothing
                return;
            }

            // Append to current screen sequence
            screenSequence.AddLast(screenName);
            while (screenSequence.Count > maxScreenHistory)
            {
                screenSequence.RemoveFirst();
            }

            GameObject currentlyDisplayed = this.screenLookup[this.CurrentScreen];
            GameObject newDisplay = this.screenLookup[screenName];

            GameScreen currentScreen = currentlyDisplayed.GetComponent<GameScreen>();
            GameScreen nextScreen = newDisplay.GetComponent<GameScreen>();

            currentScreen.HideScreen();
            nextScreen.DisplayScreen();

            ScreenChangeEventArgs changeEvent = new ScreenChangeEventArgs();
            changeEvent.oldScreen = this.CurrentScreen;
            changeEvent.newScreen = screenName;

            this.CurrentScreen = screenName;

            // invoke screen change event
            UIManager.ScreenChangeOccur?.Invoke(this, changeEvent);
        }

        public static void PreviousScreen(object sender)
        {
            // Check to make sure that there are at least two elements in the sequence
            if (UIManager.Instance.screenSequence.Count < 2)
            {
                return;
            }
            // Remove the last element
            UIManager.Instance.screenSequence.RemoveLast();
            // Get the new target sequence
            string previous = UIManager.Instance.screenSequence.Last.Value;
            // Cleanup the sequence to re-create opening this screen
            UIManager.Instance.screenSequence.RemoveLast();
            UIManager.RequestNewScreen(sender, previous);

        }

        /// <summary>
        /// Requests a new screen to be shown
        /// </summary>
        /// <param name="sender">Object sending the event</param>
        /// <param name="name">Name of new screen to show</param>
        public static void RequestNewScreen(object sender, string name)
        {
            RequestScreenChangeEventArgs request = new RequestScreenChangeEventArgs();
            request.newScreen = name;
            UIManager.RequestScreenChange?.Invoke(sender, request);
        }
    }
}