using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace nickmaltbie.OpenKCC.UI
{
    [Serializable]
    public struct DefaultPlatformBinding
    {
        public RuntimePlatform platform;
        public InputAction actionBinding;
    }

    public class BindingForPlatform : MonoBehaviour
    {
        public InputActionReference actionReference;
        public DefaultPlatformBinding[] bindings;

        public void Awake()
        {
            // If there is already a binding override, ignore this.
            if (actionReference.action.bindings[0].effectivePath != actionReference.action.bindings[0].path)
            {
                return;
            }

            foreach(DefaultPlatformBinding binding in bindings)
            {
                if (binding.platform == Application.platform)
                {
                    actionReference.action.ApplyBindingOverride(binding.actionBinding.bindings[0].effectivePath);
                }
            }
        }

    }
}