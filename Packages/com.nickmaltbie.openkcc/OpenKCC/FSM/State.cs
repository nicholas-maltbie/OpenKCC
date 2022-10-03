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
using nickmaltbie.OpenKCC.FSM.Attributes;

namespace nickmaltbie.OpenKCC.FSM
{
    /// <summary>
    /// Basic state to represent the current configuration of a state machine.
    /// </summary>
    public abstract class State
    {
        /// <summary>
        /// Gets the on entry behavior for a given state.
        /// </summary>
        /// <param name="type">Type of state to check.</param>
        /// <returns>Action for the on entry of the state or null if there is none defined.</returns>
        public static string OnEnter(Type type)
        {
            return GetActionForAttribute<OnEnterStateAttribute>(type);
        }

        /// <summary>
        /// Gets the on exit behavior for a given state.
        /// </summary>
        /// <param name="type">Type of state to check.</param>
        /// <returns>Action for the on exit of the state or null if there is none defined.</returns>
        public static string OnExit(Type type)
        {
            return GetActionForAttribute<OnExitStateAttribute>(type);
        }

        /// <summary>
        /// Checks if a given state is labeled with the initial state type.
        /// </summary>
        /// <param name="type">Type of state to check.</param>
        /// <returns>True if this is the initial state, flase otherwise.</returns>
        public static bool IsInitialState(Type type)
        {
            return Attribute.GetCustomAttribute(type, typeof(InitialStateAttribute)) != null;
        }

        /// <summary>
        /// Gets the action for a give <see cref="ActionAttribute"/> of this state.
        /// </summary>
        /// <typeparam name="E">Type of attribute to lookup action for.</typeparam>
        /// <param name="type">Type of state to check.</param>
        /// <returns>Action for given type and state or null if none is defined.</returns>
        public static string GetActionForAttribute<E>(Type type) where E : ActionAttribute
        {
            return (Attribute.GetCustomAttribute(type, typeof(E)) as ActionAttribute)?.Action;
        }
    }
}
