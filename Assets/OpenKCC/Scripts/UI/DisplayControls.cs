using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.UI
{
    [RequireComponent(typeof(Text))]
    public class DisplayControls : MonoBehaviour, IScreenComponent
    {
        public InputActionReference[] inputActions = null;

        private string GetKeyReadableName(InputAction action)
        {
            if (!action.bindings[0].isComposite)
            {
                action.GetBindingDisplayString(
                    0,
                    out string deviceLayoutName,
                    out string controlPath,
                        InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
                string name = InputControlPath.ToHumanReadableString(
                    action.bindings[0].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);

                return (string.IsNullOrEmpty(deviceLayoutName) ? name : $"{name} [{deviceLayoutName}]");
            }
            else
            {
                action.GetBindingDisplayString(
                    1,
                    out string deviceLayoutName,
                    out string controlPath,
                        InputBinding.DisplayStringOptions.DontUseShortDisplayNames);

                string names = string.Join("|", Enumerable.Range(1, 4)
                    .Select(i => InputControlPath.ToHumanReadableString(
                        action.bindings[i].effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice)));

                return (string.IsNullOrEmpty(deviceLayoutName) ? names : $"{names} [{deviceLayoutName}]");
            }
        }

        public void OnScreenLoaded()
        {
            List<string> lines = new List<string>();
            lines.Add("Controls (rebind in settings)");

            foreach (InputActionReference inputAction in inputActions)
            {
                lines.Add($"{inputAction.name}: {GetKeyReadableName(inputAction.action)}");
            }

            GetComponent<Text>().text = string.Join("\n", lines);
        }

        public void OnScreenUnloaded()
        {

        }
    }
}