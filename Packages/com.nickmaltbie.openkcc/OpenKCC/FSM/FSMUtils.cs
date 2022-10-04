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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using nickmaltbie.OpenKCC.FSM.Attributes;
using UnityEngine;

namespace nickmaltbie.OpenKCC.FSM
{
    /// <summary>
    /// Class with utility functions for state machine.
    /// </summary>
    public static class FSMUtils
    {
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
        /// Synchronously invokes an action of a given name.
        /// </summary>
        /// <param name="baseObject">Base object to invoke method of.</param>
        /// <param name="actionType">Type of action to invoke.</param>
        /// <param name="state">State to invoke action for.</param>
        /// <param name="actionCache">Cache to lookup method info from.</param>
        /// <returns>True if an action was found and invoked, false otherwise.</returns>
        public static bool InvokeAction(object baseObject, Type actionType, Type state, Dictionary<(Type, Type), MethodInfo> actionCache)
        {
            if (actionCache.TryGetValue((state, actionType), out MethodInfo method))
            {
                method.Invoke(baseObject, new object[0]);
                return method != null;
            }
            else
            {
                return false;
            }
        }
    }
}
