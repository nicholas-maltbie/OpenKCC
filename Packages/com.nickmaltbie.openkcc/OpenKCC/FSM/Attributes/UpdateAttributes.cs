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

using System;

namespace nickmaltbie.OpenKCC.FSM.Attributes
{
    /// <summary>
    /// Event invoked with every frame update. See unity doc's
    /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html">MonoBehaviour.Update</see>
    /// for reference.
    /// </summary>
    public class OnUpdateEvent : IEvent
    {
        private OnUpdateEvent() { }
        public static OnUpdateEvent Instance = new OnUpdateEvent();
    };

    /// <summary>
    /// Event invoked for frame-rate independent update for physics calculations. See unity doc's
    /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html">MonoBehaviour.FixedUpdate</see>
    /// for reference.
    /// </summary>
    public class OnFixedUpdateEvent : IEvent
    {
        private OnFixedUpdateEvent() { }
        public static OnFixedUpdateEvent Instance = new OnFixedUpdateEvent();
    };

    /// <summary>
    /// Event invoked with every frame update, LateUpdate is called after all Update functions have been called. See unity doc's
    /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html">MonoBehaviour.LateUpdate</see>
    /// for reference.
    /// </summary>
    public class OnLateUpdateEvent : IEvent
    {
        private OnLateUpdateEvent() { }
        public static OnLateUpdateEvent Instance = new OnLateUpdateEvent();
    };

    /// <summary>
    /// Event invoked for rendering and handling GUI events.
    /// See unity doc's <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnGUI.html">MonoBehaviour.OnGUI</see>
    /// for reference.
    /// </summary>
    public class OnGUIEvent : IEvent
    {
        private OnGUIEvent() { }
        public static OnGUIEvent Instance = new OnGUIEvent();
    };

    /// <summary>
    /// Event invoked for when the object becomes enabled and active.
    /// See unity doc's <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html">MonoBehaviour.OnEnable</see>
    /// for reference.
    /// </summary>
    public class OnEnableEvent : IEvent
    {
        private OnEnableEvent() { }
        public static OnEnableEvent Instance = new OnEnableEvent();
    };

    /// <summary>
    /// Event invoked for when the behaviour becomes disabled.
    /// See unity doc's <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDisable.html">MonoBehaviour.OnDisable</see>
    /// for reference.
    /// </summary>
    public class OnDisableEvent : IEvent
    {
        private OnDisableEvent() { }
        public static OnDisableEvent Instance = new OnDisableEvent();
    };

    /// <summary>
    /// Event invoked for setting up animation IK (inverse kinematics).
    /// See unity doc's <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnAnimatorIK.html">MonoBehaviour.OnAnimatorIK</see>
    /// for reference.
    /// </summary>
    public class OnAnimatorIKEvent : IEvent
    {
        private OnAnimatorIKEvent() { }
        public static OnAnimatorIKEvent Instance = new OnAnimatorIKEvent();
    };

    /// <summary>
    /// Action invoked with every frame update. See unity doc's
    /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html">MonoBehaviour.Update</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnUpdateAttribute : OnEventDoActionAttribute
    {
        public OnUpdateAttribute(string action) : base(typeof(OnUpdateEvent), action) { }
    }

    /// <summary>
    /// Frame-rate independent update for physics calculations. See unity doc's
    /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html">MonoBehaviour.FixedUpdate</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnFixedUpdateAttribute : OnEventDoActionAttribute
    {
        public OnFixedUpdateAttribute(string action) : base(typeof(OnFixedUpdateEvent), action) { }
    }

    /// <summary>
    /// Action invoked with every frame update, LateUpdate is called after all Update functions have been called. See unity doc's
    /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html">MonoBehaviour.LateUpdate</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnLateUpdateAttribute : OnEventDoActionAttribute
    {
        public OnLateUpdateAttribute(string action) : base(typeof(OnLateUpdateEvent), action) { }
    }
    
    /// <summary>
    /// OnGUI is called for rendering and handling GUI events.
    /// See unity doc's <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnGUI.html">MonoBehaviour.OnGUI</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnGUIAttribute : OnEventDoActionAttribute
    {
        public OnGUIAttribute(string action) : base(typeof(OnGUIEvent), action) { }
    }
    
    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// See unity doc's <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html">MonoBehaviour.OnEnable</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnEnableAttribute : OnEventDoActionAttribute
    {
        public OnEnableAttribute(string action) : base(typeof(OnEnableEvent), action) { }
    }
    
    /// <summary>
    /// This function is called when the behaviour becomes disabled.
    /// See unity doc's <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDisable.html">MonoBehaviour.OnDisable</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnDisableAttribute : OnEventDoActionAttribute
    {
        public OnDisableAttribute(string action) : base(typeof(OnDisableEvent), action) { }
    }
    
    /// <summary>
    /// Callback for setting up animation IK (inverse kinematics).
    /// See unity doc's <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnAnimatorIK.html">MonoBehaviour.OnAnimatorIK</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnAnimatorIKAttribute : OnEventDoActionAttribute
    {
        public OnAnimatorIKAttribute(string action) : base(typeof(OnAnimatorIKEvent), action) { }
    }
}
