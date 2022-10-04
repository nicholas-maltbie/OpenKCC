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
        /// Initializes a state machine
        /// and will set the initial
        /// state to the state defined under this class with a <see cref="InitialStateAttribute"/>.
        /// </summary>
        public StateMachine()
        {
            FSMUtils.InitializeStateMachine(this);
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
            FSMUtils.RaiseCachedEvent(this, evt);
        }

        /// <inheritdoc/>
        public void SetStateQuiet(Type newState)
        {
            CurrentState = newState;
        }
    }
}
