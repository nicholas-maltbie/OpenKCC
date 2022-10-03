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
using nickmaltbie.OpenKCC.FSM;
using nickmaltbie.OpenKCC.FSM.Attributes;
using nickmaltbie.OpenKCC.TestCommon;
using NUnit.Framework;

namespace nickmaltbie.OpenKCC.Tests.EditMode.FSM
{
    /// <summary>
    /// Basic tests for AbstractStateMachine in edit mode.
    /// </summary>
    [TestFixture]
    public class AbstractStateMachineTests : TestBase
    {
        public DemoStateMachine demoStateMachine;

        [SetUp]
        public void SetUp()
        {
            demoStateMachine = base.CreateGameObject().AddComponent<DemoStateMachine>();
        }

        [Test]
        public void VerifyTransitionNoEvent()
        {
            UnityEngine.Debug.Log($"State machine in state {demoStateMachine.CurrentState}");
            Assert.AreEqual(demoStateMachine.CurrentState, typeof(DemoStateMachine.StartingState));

            SendAndVerifyEventSequence(new (Event, Type)[]
            {
                (new EmptyEvent(), typeof(DemoStateMachine.StartingState)),
                (new EmptyEvent(), typeof(DemoStateMachine.StartingState)),
                (new EmptyEvent(), typeof(DemoStateMachine.StartingState)),
                (new EmptyEvent(), typeof(DemoStateMachine.StartingState)),
                (new EmptyEvent(), typeof(DemoStateMachine.StartingState)),
            });
        }

        [Test]
        public void VerifyTransitionsDeadEnd()
        {
            UnityEngine.Debug.Log($"State machine in state {demoStateMachine.CurrentState}");
            Assert.AreEqual(demoStateMachine.CurrentState, typeof(DemoStateMachine.StartingState));

            SendAndVerifyEventSequence(new (Event, Type)[]
            {
                (new AEvent(), typeof(DemoStateMachine.StateA)),
                (new BEvent(), typeof(DemoStateMachine.StateB)),
                (new BEvent(), typeof(DemoStateMachine.StateB)),
                (new CEvent(), typeof(DemoStateMachine.StateB)),
            });
        }

        [Test]
        public void VerifyTransitionsCycle()
        {
            UnityEngine.Debug.Log($"State machine in state {demoStateMachine.CurrentState}");
            Assert.AreEqual(demoStateMachine.CurrentState, typeof(DemoStateMachine.StartingState));

            SendAndVerifyEventSequence(new (Event, Type)[]
            {
                (new AEvent(), typeof(DemoStateMachine.StateA)),
                (new CEvent(), typeof(DemoStateMachine.StateC)),
                (new ResetEvent(), typeof(DemoStateMachine.StartingState)),
                (new AEvent(), typeof(DemoStateMachine.StateA)),
                (new CEvent(), typeof(DemoStateMachine.StateC)),
                (new ResetEvent(), typeof(DemoStateMachine.StartingState)),
            });
        }

        [Test]
        public void VerifyUpdateActionCounts()
        {
            Assert.AreEqual(demoStateMachine.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);
            Assert.AreEqual(demoStateMachine.actionStateCounts.GetOrAdd((typeof(OnFixedUpdateAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);
            Assert.AreEqual(demoStateMachine.actionStateCounts.GetOrAdd((typeof(OnLateUpdateAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);
            Assert.AreEqual(demoStateMachine.actionStateCounts.GetOrAdd((typeof(OnGUIAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);
            Assert.AreEqual(demoStateMachine.actionStateCounts.GetOrAdd((typeof(OnEnableAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);
            Assert.AreEqual(demoStateMachine.actionStateCounts.GetOrAdd((typeof(OnDisableAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);
            Assert.AreEqual(demoStateMachine.actionStateCounts.GetOrAdd((typeof(OnAnimatorIKAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);

            demoStateMachine.Update();
            Assert.AreEqual(demoStateMachine.actionStateCounts[(typeof(OnUpdateAttribute), typeof(DemoStateMachine.StartingState))], 1);

            demoStateMachine.FixedUpdate();
            Assert.AreEqual(demoStateMachine.actionStateCounts[(typeof(OnFixedUpdateAttribute), typeof(DemoStateMachine.StartingState))], 1);

            demoStateMachine.LateUpdate();
            Assert.AreEqual(demoStateMachine.actionStateCounts[(typeof(OnLateUpdateAttribute), typeof(DemoStateMachine.StartingState))], 1);

            demoStateMachine.OnGUI();
            Assert.AreEqual(demoStateMachine.actionStateCounts[(typeof(OnGUIAttribute), typeof(DemoStateMachine.StartingState))], 1);

            demoStateMachine.OnEnable();
            Assert.AreEqual(demoStateMachine.actionStateCounts[(typeof(OnEnableAttribute), typeof(DemoStateMachine.StartingState))], 1);

            demoStateMachine.OnDisable();
            Assert.AreEqual(demoStateMachine.actionStateCounts[(typeof(OnDisableAttribute), typeof(DemoStateMachine.StartingState))], 1);

            demoStateMachine.OnAnimatorIK();
            Assert.AreEqual(demoStateMachine.actionStateCounts[(typeof(OnAnimatorIKAttribute), typeof(DemoStateMachine.StartingState))], 1);

            SendAndVerifyEventSequence((new AEvent(), typeof(DemoStateMachine.StateA)));
            Assert.AreEqual(demoStateMachine.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoStateMachine.StateA)), t => 0), 0);

            demoStateMachine.Update();
            Assert.AreEqual(demoStateMachine.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoStateMachine.StateA)), t => 0), 1);
        }

        [Test]
        public void VerifyEnterExitCount()
        {
            UnityEngine.Debug.Log($"State machine in state {demoStateMachine.CurrentState}");
            Assert.AreEqual(demoStateMachine.CurrentState, typeof(DemoStateMachine.StartingState));

            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoStateMachine.StartingState)], 1);
            Assert.AreEqual(demoStateMachine.OnExitCount.GetOrAdd(typeof(DemoStateMachine.StartingState), t => 0), 0);

            SendAndVerifyEventSequence((new CEvent(), typeof(DemoStateMachine.StateC)));

            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoStateMachine.StartingState)], 1);
            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoStateMachine.StateC)], 1);

            UnityEngine.Debug.Log(demoStateMachine.OnExitCount[typeof(DemoStateMachine.StartingState)]);

            Assert.AreEqual(demoStateMachine.OnExitCount[typeof(DemoStateMachine.StartingState)], 1);

            SendAndVerifyEventSequence((new ResetEvent(), typeof(DemoStateMachine.StartingState)));

            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoStateMachine.StartingState)], 2);
            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoStateMachine.StateC)], 1);
            Assert.AreEqual(demoStateMachine.OnExitCount[typeof(DemoStateMachine.StartingState)], 1);

            SendAndVerifyEventSequence((new CEvent(), typeof(DemoStateMachine.StateC)));
            SendAndVerifyEventSequence((new CEvent(), typeof(DemoStateMachine.StateC)));

            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoStateMachine.StartingState)], 2);
            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoStateMachine.StateC)], 3);
            Assert.AreEqual(demoStateMachine.OnExitCount[typeof(DemoStateMachine.StartingState)], 2);

            SendAndVerifyEventSequence((new ResetEvent(), typeof(DemoStateMachine.StartingState)));

            Assert.AreEqual(demoStateMachine.OnEntryCount.GetOrAdd(typeof(DemoStateMachine.StateA), t => 0), 0);

            SendAndVerifyEventSequence((new ResetEvent(), typeof(DemoStateMachine.StartingState)));
            SendAndVerifyEventSequence((new AEvent(), typeof(DemoStateMachine.StateA)));

            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoStateMachine.StartingState)], 3);
            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoStateMachine.StateC)], 3);
            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoStateMachine.StateA)], 1);
            Assert.AreEqual(demoStateMachine.OnExitCount[typeof(DemoStateMachine.StartingState)], 3);

            SendAndVerifyEventSequence((new AEvent(), typeof(DemoStateMachine.StateA)));

            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoStateMachine.StateA)], 1);
        }


        /// <summary>
        /// Send an event sequence to a state machine and verify the given transitions.
        /// </summary>
        /// <param name="transitions">Sequence of events and transitions
        /// to send to the state machine and expect changes.</param>
        public void SendAndVerifyEventSequence(params (Event, Type)[] transitions)
        {
            SendAndVerifyEventSequence(transitions, false);
        }

        /// <summary>
        /// Send an event sequence to a state machine and verify the given transitions.
        /// </summary>
        /// <param name="transitions">Sequence of events and transitions
        /// to send to the state machine and expect changes.</param>
        public void SendAndVerifyEventSequence(IEnumerable<(Event, Type)> transitions, bool log = true)
        {
            foreach ((Event, Type) tuple in transitions)
            {
                if (log)
                {
                    UnityEngine.Debug.Log($"Sending event {tuple.Item1.GetType()} to state machine");
                }

                demoStateMachine.RaiseEvent(tuple.Item1);

                if (log)
                {
                    UnityEngine.Debug.Log($"State machine is now in state {demoStateMachine.CurrentState}");
                }

                Assert.AreEqual(demoStateMachine.CurrentState, tuple.Item2);
            }
        }
    }

    public class AEvent : Event { }

    public class BEvent : Event { }

    public class CEvent : Event { }

    public class ResetEvent : Event { }

    public class EmptyEvent : Event { }

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
        [OnUpdate(nameof(OnUpdateStartingState))]
        [OnFixedUpdate(nameof(OnFixedUpdateStartingState))]
        [OnLateUpdate(nameof(OnLateUpdateStartingState))]
        [OnGUI(nameof(OnGUIStartingState))]
        [OnEnable(nameof(OnEnableStartingState))]
        [OnDisable(nameof(OnDisableStartingState))]
        [OnAnimatorIK(nameof(OnAnimatorIKStartingState))]
        public class StartingState : State { }

        [Transition(typeof(BEvent), typeof(StateB))]
        [Transition(typeof(CEvent), typeof(StateC))]
        [OnEnterState(nameof(OnEnterStateACount))]
        [OnUpdate(nameof(OnUpdateStateA))]
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

        public void OnUpdateStartingState()
        {
            actionStateCounts.AddOrUpdate((typeof(OnUpdateAttribute), typeof(StartingState)), 1, (_, v) => v + 1);
        }

        public void OnFixedUpdateStartingState()
        {
            actionStateCounts.AddOrUpdate((typeof(OnFixedUpdateAttribute), typeof(StartingState)), 1, (_, v) => v + 1);
        }

        public void OnLateUpdateStartingState()
        {
            actionStateCounts.AddOrUpdate((typeof(OnLateUpdateAttribute), typeof(StartingState)), 1, (_, v) => v + 1);
        }

        public void OnGUIStartingState()
        {
            actionStateCounts.AddOrUpdate((typeof(OnGUIAttribute), typeof(StartingState)), 1, (_, v) => v + 1);
        }

        public void OnEnableStartingState()
        {
            actionStateCounts.AddOrUpdate((typeof(OnEnableAttribute), typeof(StartingState)), 1, (_, v) => v + 1);
        }

        public void OnDisableStartingState()
        {
            actionStateCounts.AddOrUpdate((typeof(OnDisableAttribute), typeof(StartingState)), 1, (_, v) => v + 1);
        }

        public void OnAnimatorIKStartingState()
        {
            actionStateCounts.AddOrUpdate((typeof(OnAnimatorIKAttribute), typeof(StartingState)), 1, (_, v) => v + 1);
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
