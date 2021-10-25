using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.UI.Actions
{
    /// <summary>
    /// A binding that can be reset
    /// </summary>
    public interface IBindingControl
    {
        void ResetBinding();
        void UpdateDisplay();
    }
}