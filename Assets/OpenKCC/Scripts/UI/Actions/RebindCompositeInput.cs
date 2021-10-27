using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.UI.Actions
{
    /// <summary>
    /// Rebind a composite set of inputs
    /// </summary>
    public class RebindCompositeInput : MonoBehaviour, IBindingControl
    {
        /// <summary>
        /// Prefix for input mapping for saving to player preferences
        /// </summary>
        public const string inputMappingPlayerPrefPrefix = "Input Mapping";

        /// <summary>
        /// Composite input action being modified
        /// </summary>
        public InputActionReference inputAction = null;
        /// <summary>
        /// Menu controller related to this selected object
        /// </summary>
        public MenuController menuController;

        /// <summary>
        /// Rebinding group definition that includes the string name and button to control it
        /// </summary>
        [System.Serializable]
        public struct RebindingGroup
        {
            /// <summary>
            /// Location of descriptive name of button
            /// </summary>
            public UnityEngine.UI.Text bindingDisplayNameText;
            /// <summary>
            /// Button used to start rebinding operation
            /// </summary>
            public Button startRebinding;
            /// <summary>
            /// Text to show when waiting for player input
            /// </summary>
            public GameObject waitingForInputObject;
        }

        /// <summary>
        /// Binding groups that control binding for each component of the composite
        /// </summary>
        public RebindingGroup[] rebindingGroups;

        /// <summary>
        /// Rebinding operation action waiting for player command to change button bindings
        /// </summary>
        public InputActionRebindingExtensions.RebindingOperation rebindingOperation { get; private set; }

        /// <summary>
        /// Get a readable display name for a binding index
        /// </summary>
        /// <param name="index">Index of binding information</param>
        /// <returns>Human readable information of button for this binding index</returns>
        private string GetKeyReadableName(int index) => InputControlPath.ToHumanReadableString(
            inputAction.action.bindings[index].effectivePath,
            InputControlPath.HumanReadableStringOptions.OmitDevice);

        /// <summary>
        /// Get the input mapping player preference key from a given index
        /// </summary>
        /// <param name="index">Index of binding component</param>
        public string InputMappingKey(int index) => $"{inputMappingPlayerPrefPrefix} {index} {inputAction.action.name}";

        public void Awake()
        {
            // Load the default mapping saved to the file
            // Start bindings at 1 as 0 is the composite start and 1 represents the first binding index
            for (int i = 1; i <= rebindingGroups.Length; i++)
            {
                string inputMapping = PlayerPrefs.GetString(InputMappingKey(i), string.Empty);
                if (!string.IsNullOrEmpty(inputMapping))
                {
                    inputAction.action.ApplyBindingOverride(i, inputMapping);
                }
            }
        }

        public void Start()
        {
            for (int i = 0; i < rebindingGroups.Length; i++)
            {
                int temp = i;
                rebindingGroups[i].startRebinding.onClick.AddListener(() => StartRebinding(temp));
                rebindingGroups[i].bindingDisplayNameText.text = GetKeyReadableName(i + 1);
            }
        }

        /// <summary>
        /// Start the rebinding process for a given component of this composite axis.
        /// </summary>
        /// <param name="index">Index of binding (starting at 0)</param>
        public void StartRebinding(int index)
        {
            rebindingGroups[index].startRebinding.gameObject.SetActive(false);
            rebindingGroups[index].waitingForInputObject.SetActive(true);
            menuController.allowInputChanges = false;

            inputAction.action.Disable();
            inputAction.action.actionMap.Disable();

            rebindingOperation = inputAction.action.PerformInteractiveRebinding(index + 1)
                .WithControlsExcluding("<Pointer>/position") // Don't bind to mouse position
                .WithControlsExcluding("<Pointer>/delta")    // To avoid accidental input from mouse motion
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .WithTimeout(5.0f)
                .OnComplete(operation => RebindComplete(index))
                .OnCancel(operation => RebindCancel(index))
                .Start();
        }

        /// <summary>
        /// Cancel the rebinding process for a given component of this composite axis.
        /// </summary>
        public void RebindCancel(int index)
        {
            int bindingIndex = index + 1;

            rebindingGroups[index].bindingDisplayNameText.text = GetKeyReadableName(bindingIndex);
            rebindingOperation.Dispose();

            rebindingGroups[index].startRebinding.gameObject.SetActive(true);
            rebindingGroups[index].waitingForInputObject.SetActive(false);
            menuController.allowInputChanges = true;
            inputAction.action.Enable();
            inputAction.action.actionMap.Enable();
        }


        /// <summary>
        /// Finish the rebinding process for a given component of this composite axis.
        /// </summary>
        /// <param name="index">Index of binding (starting at 0)</param>
        public void RebindComplete(int index)
        {
            int bindingIndex = index + 1;
            string overridePath = inputAction.action.bindings[bindingIndex].overridePath;
            foreach (PlayerInput input in GameObject.FindObjectsOfType<PlayerInput>())
            {
                InputAction action = input.actions.FindAction(inputAction.name);
                if (action != null) action.ApplyBindingOverride(bindingIndex, overridePath);
            }

            rebindingGroups[index].bindingDisplayNameText.text = GetKeyReadableName(bindingIndex);
            rebindingOperation.Dispose();

            PlayerPrefs.SetString(InputMappingKey(bindingIndex), inputAction.action.bindings[bindingIndex].overridePath);

            rebindingGroups[index].startRebinding.gameObject.SetActive(true);
            rebindingGroups[index].waitingForInputObject.SetActive(false);
            menuController.allowInputChanges = true;
            inputAction.action.Enable();
            inputAction.action.actionMap.Enable();
        }

        public void ResetBinding()
        {
            inputAction.action.RemoveAllBindingOverrides();
            for (int bindingIndex = 1; bindingIndex <= rebindingGroups.Length; bindingIndex++)
            {
                PlayerPrefs.DeleteKey(InputMappingKey(bindingIndex));
            }
        }

        public void UpdateDisplay()
        {
            for (int index = 0; index < rebindingGroups.Length; index++)
            {
                int bindingIndex = index + 1;
                rebindingGroups[index].bindingDisplayNameText.text = GetKeyReadableName(bindingIndex);
            }
        }
    }
}