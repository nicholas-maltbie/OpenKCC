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

using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;

namespace nickmaltbie.OpenKCC.UI
{
    [RequireComponent(typeof(UnityEngine.UI.Text))]
    public class DisplayControls : MonoBehaviour, IScreenComponent
    {
        public InputActionReference[] inputActions = null;

        private string GetKeyReadableName(InputAction action)
        {
            if (!action.bindings[0].isComposite)
            {
                action.GetBindingDisplayString(
                    0,
                    out var deviceLayoutName,
                    out var controlPath,
                        InputBinding.DisplayStringOptions.DontUseShortDisplayNames);
                var name = InputControlPath.ToHumanReadableString(
                    action.bindings[0].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);

                return (string.IsNullOrEmpty(deviceLayoutName) ? name : $"{name} [{deviceLayoutName}]");
            }
            else
            {
                action.GetBindingDisplayString(
                    1,
                    out var deviceLayoutName,
                    out var controlPath,
                        InputBinding.DisplayStringOptions.DontUseShortDisplayNames);

                var names = string.Join("|", Enumerable.Range(1, 4)
                    .Select(i => InputControlPath.ToHumanReadableString(
                        action.bindings[i].effectivePath,
                        InputControlPath.HumanReadableStringOptions.OmitDevice)));

                return (string.IsNullOrEmpty(deviceLayoutName) ? names : $"{names} [{deviceLayoutName}]");
            }
        }

        public void Start()
        {
            UpdateDisplay();
        }

        public void UpdateDisplay()
        {
            var lines = new List<string>();
            lines.Add("Controls (rebind in settings)");

            foreach (var inputAction in inputActions)
            {
                lines.Add($"{inputAction.name}: {GetKeyReadableName(inputAction.action)}");
            }

            GetComponent<UnityEngine.UI.Text>().text = string.Join("\n", lines);
        }

        public void OnScreenLoaded()
        {
            UpdateDisplay();
        }

        public void OnScreenUnloaded()
        {

        }
    }
}
