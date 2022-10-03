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

namespace nickmaltbie.OpenKCC.StateMachine.Attributes
{
    /// <summary>
    /// Action invoked with every frame update. See unity doc's
    /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.Update.html">MonoBehaviour.Update</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnUpdateAttribute : ActionAttribute
    {
        public OnUpdateAttribute(Action action) : base(action) { }
    }

    /// <summary>
    /// Frame-rate independent update for physics calculations. See unity doc's
    /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.FixedUpdate.html">MonoBehaviour.FixedUpdate</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnFixedUpdateAttribute : ActionAttribute
    {
        public OnFixedUpdateAttribute(Action action) : base(action) { }
    }

    /// <summary>
    /// Action invoked with every frame update, LateUpdate is called after all Update functions have been called. See unity doc's
    /// <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.LateUpdate.html">MonoBehaviour.LateUpdate</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnLateUpdateAttribute : ActionAttribute
    {
        public OnLateUpdateAttribute(Action action) : base(action) { }
    }
    
    /// <summary>
    /// OnGUI is called for rendering and handling GUI events.
    /// See unity doc's <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnGUI.html">MonoBehaviour.OnGUI</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnGUIAttribute : ActionAttribute
    {
        public OnGUIAttribute(Action action) : base(action) { }
    }
    
    /// <summary>
    /// This function is called when the object becomes enabled and active.
    /// See unity doc's <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnEnable.html">MonoBehaviour.OnEnable</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnEnableAttribute : ActionAttribute
    {
        public OnEnableAttribute(Action action) : base(action) { }
    }
    
    /// <summary>
    /// This function is called when the behaviour becomes disabled.
    /// See unity doc's <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnDisable.html">MonoBehaviour.OnDisable</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnDisableAttribute : ActionAttribute
    {
        public OnDisableAttribute(Action action) : base(action) { }
    }
    
    /// <summary>
    /// Callback for setting up animation IK (inverse kinematics).
    /// See unity doc's <see href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnAnimatorIK.html">MonoBehaviour.OnAnimatorIK</see>
    /// for reference.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class OnAnimatorIKAttribute : ActionAttribute
    {
        public OnAnimatorIKAttribute(Action action) : base(action) { }
    }
}
