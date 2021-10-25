using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.UI.Actions
{
    /// <summary>
    /// Reset binding of all configurable input actions
    /// </summary>
    public class ResetBindingsButton : MonoBehaviour
    {
        public Button button;

        public void Start()
        {
            button.onClick.AddListener(() => ResetBindings());
        }

        public void ResetBindings()
        {
            foreach (IBindingControl control in GetComponentsInChildren<IBindingControl>())
            {
                control.ResetBinding();
                control.UpdateDisplay();
            }
        }
    }
}