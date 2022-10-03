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
using System.Linq;
using System.Reflection;
using nickmaltbie.OpenKCC.FSM.Attributes;
using UnityEngine;

namespace nickmaltbie.OpenKCC.FSM
{
    /// <summary>
    /// Abstract state machine to manage a set of given states
    /// and transitions.
    /// </summary>
    public class StateMachine : MonoBehaviour
    {
        /// <summary>
        /// Current state of the state machine.
        /// </summary>
        public Type CurrentState { get; private set; }

        /// <summary>
        /// Initializes an abstract state machine based on a
        /// <see cref="MonoBehaviour"/> and will set the initial
        /// state to the state defined under this class with a <see cref="InitialStateAttribute"/>.
        /// </summary>
        public StateMachine()
        {
            CurrentState = GetType().GetNestedTypes()
                .Where(type => type.IsClass && type.IsSubclassOf(typeof(State)))
                .First(type => State.IsInitialState(type));

            InvokeAction(State.OnEnter(CurrentState));
        }

        /// <summary>
        /// Raise a synchronous event for a given state machine.
        /// <br/>
        /// If the state machine's <see cref="CurrentState"/> expects a transition
        /// based on the event, then this will trigger the <see cref="OnExitStateAttribute"/>
        /// of the <see cref="CurrentState"/>, change to the next state defined in
        /// the <see cref="TransitionAttribute"/>, then trigger the <see cref="OnEnterStateAttribute"/>
        /// of the next state.
        /// </summary>
        /// <param name="evt">Event to send to this state machine.</param>
        public void RaiseEvent(Event evt)
        {
            if (TransitionAttribute.RequireTransition(CurrentState, evt, out Type nextState))
            {
                InvokeAction(State.OnExit(CurrentState));
                CurrentState = nextState;
                InvokeAction(State.OnEnter(nextState));
            }
        }

        /// <summary>
        /// Synchronously invokes an action of a given name.
        /// </summary>
        /// <param name="actionName">Name of action to invoke.</param>
        public void InvokeAction(string actionName)
        {
            if (actionName != null)
            {
                GetActionWithName(actionName)?.Invoke(this, new object[0]);
            }
        }

        /// <summary>
        /// Returns the action with the specified name.
        /// </summary>
        /// <param name="actionName">Name of action to search for.</param>
        /// <returns>Method info for the given action.</returns>
        private protected MethodInfo GetActionWithName(string actionName)
        {
            MethodInfo action;
            Type actorType = GetType();

            do
            {
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.FlattenHierarchy;
                action = actorType.GetMethod(actionName, bindingFlags, Type.DefaultBinder, Array.Empty<Type>(), null);
                actorType = actorType.BaseType;
            }
            while (action is null && actorType != typeof(StateMachine) && actorType != typeof(MonoBehaviour));

            return action;
        }

        /// <summary>
        /// Gets the action for a attribute based on the current state.
        /// </summary>
        /// <typeparam name="E">Attribute to search for.</typeparam>
        /// <returns>name of the action defined for the current state or null if none is provided.</returns>
        public string GetActionForCurrentState<E>() where E : ActionAttribute
        {
            return State.GetActionForAttribute<E>(CurrentState);
        }

        public void Update()
        {
            InvokeAction(GetActionForCurrentState<OnUpdateAttribute>());
        }

        public void FixedUpdate()
        {
            InvokeAction(GetActionForCurrentState<OnFixedUpdateAttribute>());
        }

        public void LateUpdate()
        {
            InvokeAction(GetActionForCurrentState<OnLateUpdateAttribute>());
        }

        public void OnGUI()
        {
            InvokeAction(GetActionForCurrentState<OnGUIAttribute>());
        }

        public void OnEnable()
        {
            InvokeAction(GetActionForCurrentState<OnEnableAttribute>());
        }

        public void OnDisable()
        {
            InvokeAction(GetActionForCurrentState<OnDisableAttribute>());
        }

        public void OnAnimatorIK()
        {
            InvokeAction(GetActionForCurrentState<OnAnimatorIKAttribute>());
        }
    }
}
