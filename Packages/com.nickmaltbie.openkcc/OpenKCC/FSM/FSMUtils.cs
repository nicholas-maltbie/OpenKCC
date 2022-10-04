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
    /// Class with utility functions for state machine.
    /// </summary>
    public static class FSMUtils
    {
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
        /// Returns the action with the specified name.
        /// </summary>
        /// <param name="actionName">Name of action to search for.</param>
        /// <returns>Method info for the given action.</returns>
        public static MethodInfo GetActionWithName(Type type, string actionName)
        {
            MethodInfo action;

            do
            {
                BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic |
                    BindingFlags.Instance | BindingFlags.FlattenHierarchy;
                action = type.GetMethod(actionName, bindingFlags);
                type = type.BaseType;
            }
            while (action is null && type != null);

            return action;
        }

        /// <summary>
        /// Sets up an action lookup by enumerating the <see cref="State"/> classes
        /// defined within a state machine type and getting all <see cref="ActionAttribute"/>
        /// decorators defined for each <see cref="State"/> within the class.
        /// </summary>
        /// <param name="stateMachine">State machine to lookup <see cref="State"/> and <see cref="ActionAttribute"/> for.</param>
        /// <returns>A lookup table mapped as (state, actionType) -> Method</returns>
        public static Dictionary<(Type, Type), MethodInfo> CreateActionAttributeCache(Type stateMachine)
        {
            var actionLookup = new Dictionary<(Type, Type), MethodInfo>();

            // Find all the supported states for the state machine.
            foreach (Type state in stateMachine.GetNestedTypes()
                .Where(type => type.IsClass && type.IsSubclassOf(typeof(State))))
            {
                // Find all action attributes of given state
                foreach (ActionAttribute attr in Attribute.GetCustomAttributes(state, typeof(ActionAttribute)))
                {
                    Type actionType = attr.GetType();
                    actionLookup[(state, actionType)] = GetActionWithName(stateMachine, attr.Action);
                }
            }

            return actionLookup;
        }

        /// <summary>
        /// Sets up an transition lookup by enumerating the <see cref="State"/> classes
        /// defined within a state machine type and getting all <see cref="TransitionAttribute"/>
        /// decorators defined for each <see cref="State"/> within the class.
        /// </summary>
        /// <param name="stateMachine">State machine to lookup <see cref="State"/> and <see cref="TransitionAttribute"/> for.</param>
        /// <returns>A lookup table mapped as (state, event) -> state</returns>
        public static Dictionary<(Type, Type), Type> CreateTransationAttributeCache(Type stateMachine)
        {
            var transitionLookup = new Dictionary<(Type, Type), Type>();

            // Find all the supported states for the state machine.
            foreach (Type state in stateMachine.GetNestedTypes()
                .Where(type => type.IsClass && type.IsSubclassOf(typeof(State))))
            {
                // Find all transition attributes of given state
                foreach (TransitionAttribute attr in Attribute.GetCustomAttributes(state, typeof(TransitionAttribute)))
                {
                    transitionLookup[(state, attr.TriggerEvent)] = attr.TargetState;
                }
            }

            return transitionLookup;
        }

        /// <summary>
        /// Sets up an event lookup by enumerating the <see cref="State"/> classes
        /// defined within a state machine type and getting all <see cref="OnEventDoActionAttribute"/>
        /// decorators defined for each <see cref="State"/> within the class.
        /// </summary>
        /// <param name="stateMachine">State machine to lookup <see cref="State"/> and <see cref="OnEventDoActionAttribute"/> for.</param>
        /// <returns>A lookup table mapped as (state, event) -> [ methods ]</returns>
        public static Dictionary<(Type, Type), List<MethodInfo>> CreateEventActionCache(Type stateMachine)
        {
            var eventLookup = new Dictionary<(Type, Type), List<MethodInfo>>();

            // Find all the supported states for the state machine.
            foreach (Type state in stateMachine.GetNestedTypes()
                .Where(type => type.IsClass && type.IsSubclassOf(typeof(State))))
            {
                // Find all OnEventDoAction attributes of given state
                foreach (OnEventDoActionAttribute attr in Attribute.GetCustomAttributes(state, typeof(OnEventDoActionAttribute)))
                {
                    Type evt = attr.Event;
                    MethodInfo action = FSMUtils.GetActionWithName(stateMachine, attr.Action);

                    if (eventLookup.ContainsKey((state, evt)))
                    {
                        eventLookup[(state, evt)].Add(action);
                    }
                    else
                    {
                        eventLookup[(state, evt)] = new List<MethodInfo>(new MethodInfo[] { action });
                    }
                }
            }

            return eventLookup;
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
        /// <param name="StateMachine">state machine to invoke method of.</param>
        /// <param name="evt">Event to send to this state machine.</param>
        public static void RaiseCachedEvent(IStateMachine stateMachine, IEvent evt)
        {
            if (EventCache[stateMachine.GetType()].TryGetValue((stateMachine.CurrentState, evt.GetType()), out List<MethodInfo> actions))
            {
                foreach (MethodInfo action in actions)
                {
                    action?.Invoke(stateMachine, new object[0]);
                }
            }

            if (TransitionCache[stateMachine.GetType()].TryGetValue((stateMachine.CurrentState, evt.GetType()), out Type nextState))
            {
                InvokeAction<OnExitStateAttribute>(stateMachine, stateMachine.CurrentState);
                stateMachine.SetStateQuiet(nextState);
                InvokeAction<OnEnterStateAttribute>(stateMachine, stateMachine.CurrentState);
            }
        }

        /// <summary>
        /// Synchronously invokes an action of a given name.
        /// </summary>
        /// <typeparam name="E">Type of action to invoke.</typeparam>
        /// <param name="stateMachine">state machine to invoke method of.</param>
        /// <param name="state">State to invoke action for, if unspecificed will use current state.</param>
        /// <returns>True if an action was found and invoked, false otherwise.</returns>
        public static bool InvokeAction<E>(IStateMachine stateMachine, Type state = null) where E : ActionAttribute
        {
            return InvokeAction(stateMachine, typeof(E), state);
        }

        /// <summary>
        /// Synchronously invokes an action of a given name.
        /// </summary>
        /// <param name="stateMachine">state machine to invoke method of.</param>
        /// <param name="actionType">Type of action to invoke.</param>
        /// <param name="state">State to invoke action for.</param>
        /// <returns>True if an action was found and invoked, false otherwise.</returns>
        public static bool InvokeAction(IStateMachine stateMachine, Type actionType, Type state)
        {
            UnityEngine.Debug.Log($"Invoking sm: {stateMachine.GetType()} action: {actionType} state: {state}");
            UnityEngine.Debug.Log($"Cached actions for sm: {stateMachine.GetType()} include: [{string.Join(", ", ActionCache[stateMachine.GetType()].Keys.Select(tuple => $"({tuple.Item1}, {tuple.Item2})"))})]");
            if (ActionCache[stateMachine.GetType()].TryGetValue((state ?? stateMachine.CurrentState, actionType), out MethodInfo method))
            {
                method.Invoke(stateMachine, new object[0]);
                return method != null;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Initialize a state machine with the initializetion state and ensure
        /// all the events, transitions, and actions are cached.
        /// </summary>
        /// <param name="stateMachine">state machine to setup.</param>
        public static void InitializeStateMachine(IStateMachine stateMachine)
        {
            // Ensure the cahce is setup if not done so already
            SetupCache(stateMachine.GetType());

            stateMachine.SetStateQuiet(stateMachine.GetType().GetNestedTypes()
                .Where(type => type.IsClass && type.IsSubclassOf(typeof(State)))
                .First(type => State.IsInitialState(type)));

            UnityEngine.Debug.Log($"Set initial state to \"{stateMachine.CurrentState}\" (isNull = {stateMachine.CurrentState == null})");

            InvokeAction<OnEnterStateAttribute>(stateMachine, stateMachine.CurrentState);
        }

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
    }
}
