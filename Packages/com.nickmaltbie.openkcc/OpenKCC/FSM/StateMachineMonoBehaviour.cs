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

using nickmaltbie.OpenKCC.FSM.Attributes;
using UnityEngine;

namespace nickmaltbie.OpenKCC.FSM
{
    public abstract class StateMachineMonoBehaviour : MonoBehaviour
    {
        public abstract IStateMachine StateMachine { get; }

        public virtual void Update()
        {
            StateMachine.RaiseEvent(OnUpdateEvent.Instance);
        }

        public virtual void FixedUpdate()
        {
            StateMachine.RaiseEvent(OnFixedUpdateEvent.Instance);
        }

        public virtual void LateUpdate()
        {
            StateMachine.RaiseEvent(OnLateUpdateEvent.Instance);
        }

        public virtual void OnGUI()
        {
            StateMachine.RaiseEvent(OnGUIEvent.Instance);
        }

        public virtual void OnEnable()
        {
            StateMachine.RaiseEvent(OnEnableEvent.Instance);
        }

        public virtual void OnDisable()
        {
            StateMachine.RaiseEvent(OnDisableEvent.Instance);
        }

        public virtual void OnAnimatorIK()
        {
            StateMachine.RaiseEvent(OnAnimatorIKEvent.Instance);
        }
    }
}
