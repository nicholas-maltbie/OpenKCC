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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using nickmaltbie.OpenKCC.FSM.Attributes;

namespace nickmaltbie.OpenKCC.FSM
{
    /// <summary>
    /// Abstract state machine to manage a set of given states
    /// and transitions.
    /// </summary>
    public abstract class StateMachine : IStateMachine
    {
        /// <summary>
        /// Current state of the state machine.
        /// </summary>
        public Type CurrentState { get; private set; }

        /// <summary>
        /// Map of actions of state machine -> state, attribute) -> action.
        /// </summary>
        internal static ConcurrentDictionary<Type, Dictionary<(Type, Type), MethodInfo>> ActionCache =
            new ConcurrentDictionary<Type, Dictionary<(Type, Type), MethodInfo>>();

        /// <summary>
        /// Map of transitions of state machine -> (state, event) -> state.
        /// </summary>
        internal static ConcurrentDictionary<Type, Dictionary<(Type, Type), Type>> TransitionCache =
            new ConcurrentDictionary<Type, Dictionary<(Type, Type), Type>>();

        /// <summary>
        /// Map of actions of state machine -> (state, event) -> [ actions ]
        /// </summary>
        internal static ConcurrentDictionary<Type, Dictionary<(Type, Type), List<MethodInfo>>> EventCache =
            new ConcurrentDictionary<Type, Dictionary<(Type, Type), List<MethodInfo>>>();

        /// <summary>
        /// Setup the cache for the state machine if it hasn't been done already.
        /// </summary>
        internal static void SetupCache(Type stateMachine)
        {
            if (!ActionCache.ContainsKey(stateMachine))
            {
                ActionCache.TryAdd(stateMachine, FSMUtils.CreateActionAttributeCache(stateMachine));
            }
            if (!TransitionCache.ContainsKey(stateMachine))
            {
                TransitionCache.TryAdd(stateMachine, FSMUtils.CreateTransationAttributeCache(stateMachine));
            }
            if (!EventCache.ContainsKey(stateMachine))
            {
                EventCache.TryAdd(stateMachine, FSMUtils.CreateEventActionCache(stateMachine));
            }
        }

        /// <summary>
        /// Initializes a state machine
        /// and will set the initial
        /// state to the state defined under this class with a <see cref="InitialStateAttribute"/>.
        /// </summary>
        public StateMachine()
        {
            // Ensure the cahce is setup if not done so already
            SetupCache(GetType());

            CurrentState = GetType().GetNestedTypes()
                .Where(type => type.IsClass && type.IsSubclassOf(typeof(State)))
                .First(type => State.IsInitialState(type));

            InvokeAction<OnEnterStateAttribute>(CurrentState);
        }

        /// <summary>
        /// Raise a synchronous event for a given state machine.
        /// <br/>
        /// First checks if this state machine expects any events of this type
        /// for the state machine's <see cref="CurrentState"/>. These
        /// would follow an attribute of type <see cref="OnEventDoActionAttribute"/>.
        /// <br/>
        /// If the state machine's <see cref="CurrentState"/> expects a transition
        /// based on the event, then this will trigger the <see cref="OnExitStateAttribute"/>
        /// of the <see cref="CurrentState"/>, change to the next state defined in
        /// the <see cref="TransitionAttribute"/>, then trigger the <see cref="OnEnterStateAttribute"/>
        /// of the next state.
        /// </summary>
        /// <param name="evt">Event to send to this state machine.</param>
        public void RaiseEvent(IEvent evt)
        {
            if (EventCache[GetType()].TryGetValue((CurrentState, evt.GetType()), out List<MethodInfo> actions))
            {
                foreach (MethodInfo action in actions)
                {
                    action?.Invoke(this, new object[0]);
                }
            }

            if (TransitionCache[GetType()].TryGetValue((CurrentState, evt.GetType()), out Type nextState))
            {
                InvokeAction<OnExitStateAttribute>(CurrentState);
                CurrentState = nextState;
                InvokeAction<OnEnterStateAttribute>(CurrentState);
            }
        }

        /// <summary>
        /// Synchronously invokes an action of a given name.
        /// </summary>
        /// <typeparam name="E">Type of action to invoke.</typeparam>
        /// <param name="state">State to invoke action for, if unspecificed will use current state.</param>
        /// <returns>True if an action was found and invoked, false otherwise.</returns>
        public bool InvokeAction<E>(Type state = null) where E : ActionAttribute
        {
            return InvokeAction(typeof(E), state);
        }

        /// <summary>
        /// Synchronously invokes an action of a given name.
        /// </summary>
        /// <param name="actionType">Type of action to invoke.</param>
        /// <param name="state">State to invoke action for, if unspecificed will use current state.</param>
        /// <returns>True if an action was found and invoked, false otherwise.</returns>
        public bool InvokeAction(Type actionType, Type state = null)
        {
            if (ActionCache[GetType()].TryGetValue((state ?? CurrentState, actionType), out MethodInfo method))
            {
                method.Invoke(this, new object[0]);
                return method != null;
            }
            else
            {
                return false;
            }
        }
    }
}
