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

namespace nickmaltbie.OpenKCC.StateMachine.Attributes
{
    /// <summary>
    /// Transition attribute to manage transitions between states for a state machine.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class TransitionAttribute : Attribute
    {
        /// <summary>
        /// Type of event to listen for.
        /// </summary>
        public Type TriggerEvent { get; private set; }

        /// <summary>
        /// Target state upon event trigger.
        /// </summary>
        public Type TargetState { get; private set; }

        /// <summary>
        /// Transition to another state on a given event.
        /// </summary>
        /// <param name="triggerEvent">Trigger event to cause transition.</param>
        /// <param name="targetState">New state to transition to upon trigger.</param>
        public TransitionAttribute(Type triggerEvent, Type targetState)
        {
            TriggerEvent = triggerEvent;
            TargetState = targetState;
        }

        /// <summary>
        /// Does a given state require a transition when an event is raised.
        /// </summary>
        /// <param name="currentState">State to check for required transition.</param>
        /// <param name="raisedEvent">Raised event based on the state transition.</param>
        /// <param name="nextState"></param>
        /// <returns></returns>
        public static bool RequireTransition(Type currentState, Event raisedEvent, out Type nextState)
        {
            var transitions = GetCustomAttributes(currentState, typeof(TransitionAttribute)) as TransitionAttribute[];
            nextState = transitions.FirstOrDefault(transition => raisedEvent.GetType().IsInstanceOfType(transition.TriggerEvent))?.TargetState;
            return nextState != null;
        }
    }
}
