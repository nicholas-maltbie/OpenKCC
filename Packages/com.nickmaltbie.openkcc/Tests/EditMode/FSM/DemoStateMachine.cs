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
using nickmaltbie.OpenKCC.FSM;
using nickmaltbie.OpenKCC.FSM.Attributes;

namespace nickmaltbie.OpenKCC.Tests.EditMode.FSM
{
    /// <summary>
    /// Demo state machine for 
    /// </summary>
    public class DemoStateMachine : StateMachine
    {
        /// <summary>
        /// Counts of actions for each combination of (Action, State).
        /// </summary>
        public ConcurrentDictionary<(Type, Type), int> actionStateCounts = new ConcurrentDictionary<(Type, Type), int>();

        /// <summary>
        /// Times that a state is entered.
        /// </summary>
        public ConcurrentDictionary<Type, int> OnEntryCount = new ConcurrentDictionary<Type, int>();

        /// <summary>
        /// Times that a state is exited.
        /// </summary>
        public ConcurrentDictionary<Type, int> OnExitCount = new ConcurrentDictionary<Type, int>();

        [InitialState]
        [Transition(typeof(AEvent), typeof(StateA))]
        [Transition(typeof(BEvent), typeof(StateB))]
        [Transition(typeof(CEvent), typeof(StateC))]
        [OnEnterState(nameof(OnEnterStartingState))]
        [OnExitState(nameof(OnExitStartingState))]
        public class StartingState : State { }

        [Transition(typeof(BEvent), typeof(StateB))]
        [Transition(typeof(CEvent), typeof(StateC))]
        [OnEnterState(nameof(OnEnterStateACount))]
        [OnEventDoAction(typeof(AEvent), nameof(DoNothing))]
        [OnEventDoAction(typeof(OnUpdateEvent), nameof(DoNothing))]
        public class StateA : State { }

        [OnEnterState(nameof(OnEnterStateBCount))]
        public class StateB : State { }

        [Transition(typeof(ResetEvent), typeof(StartingState))]
        [Transition(typeof(CEvent), typeof(StateC))]
        [OnEnterState(nameof(OnEnterStateCCount))]
        public class StateC : State { }
        public void OnUpdateStateA()
        {
            actionStateCounts.AddOrUpdate((typeof(OnUpdateAttribute), typeof(StateA)), 1, (_, v) => v + 1);
        }

        public void DoNothing()
        {

        }

        public void OnEnterStartingState()
        {
            OnEntryCount.AddOrUpdate(typeof(StartingState), 1, (Type t, int v) => v + 1);
        }

        public void OnExitStartingState()
        {
            OnExitCount.AddOrUpdate(typeof(StartingState), 1, (Type t, int v) => v + 1);
        }

        public void OnEnterStateACount()
        {
            OnEntryCount.AddOrUpdate(typeof(StateA), 1, (Type t, int v) => v + 1);
        }

        public void OnEnterStateBCount()
        {
            OnEntryCount.AddOrUpdate(typeof(StateB), 1, (Type t, int v) => v + 1);
        }

        public void OnEnterStateCCount()
        {
            OnEntryCount.AddOrUpdate(typeof(StateC), 1, (Type t, int v) => v + 1);
        }
    }
}
