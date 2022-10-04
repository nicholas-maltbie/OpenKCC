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
using System.Reflection;
using nickmaltbie.OpenKCC.FSM;
using nickmaltbie.OpenKCC.FSM.Attributes;
using nickmaltbie.OpenKCC.TestCommon;
using NUnit.Framework;

namespace nickmaltbie.OpenKCC.Tests.EditMode.FSM
{
    /// <summary>
    /// Basic tests for <see cref="StateMachineMonoBehaviour"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class StateMachineMonoBehaviourTests : TestBase
    {
        public DemoStateMachineMonoBehaviour sm;

        [SetUp]
        public void SetUp()
        {
            sm = base.CreateGameObject().AddComponent<DemoStateMachineMonoBehaviour>();
        }

        [Test]
        public void VerifyUpdateActionCounts()
        {
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnFixedUpdateAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnLateUpdateAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnGUIAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnEnableAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnDisableAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnAnimatorIKAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState)), t => 0), 0);

            sm.Update();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnUpdateAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState))], 1);

            sm.FixedUpdate();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnFixedUpdateAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState))], 1);

            sm.LateUpdate();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnLateUpdateAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState))], 1);

            sm.OnGUI();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnGUIAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState))], 1);

            sm.OnEnable();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnEnableAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState))], 1);

            sm.OnDisable();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnDisableAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState))], 1);

            sm.OnAnimatorIK();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnAnimatorIKAttribute), typeof(DemoStateMachineMonoBehaviour.StartingState))], 1);

            sm.RaiseEvent(new AEvent());
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoStateMachineMonoBehaviour.StateA)), t => 0), 0);

            sm.Update();
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoStateMachineMonoBehaviour.StateA)), t => 0), 1);
        }

        [Test]
        public void RebuildCache()
        {
            StateMachine.ActionCache = new ConcurrentDictionary<Type, Dictionary<(Type, Type), MethodInfo>>();
            StateMachine.TransitionCache = new ConcurrentDictionary<Type, Dictionary<(Type, Type), Type>>();
            StateMachine.EventCache = new ConcurrentDictionary<Type, Dictionary<(Type, Type), List<MethodInfo>>>();
            StateMachine.SetupCache(typeof(DemoStateMachineMonoBehaviour));
        }

        [Test]
        public void VerifyTransitionNoEvent()
        {
            UnityEngine.Debug.Log($"State machine in state {sm.CurrentState}");
            Assert.AreEqual(sm.CurrentState, typeof(DemoStateMachineMonoBehaviour.StartingState));

            SendAndVerifyEventSequence(new (IEvent, Type)[]
            {
                (new EmptyEvent(), typeof(DemoStateMachineMonoBehaviour.StartingState)),
                (new EmptyEvent(), typeof(DemoStateMachineMonoBehaviour.StartingState)),
                (new EmptyEvent(), typeof(DemoStateMachineMonoBehaviour.StartingState)),
                (new EmptyEvent(), typeof(DemoStateMachineMonoBehaviour.StartingState)),
                (new EmptyEvent(), typeof(DemoStateMachineMonoBehaviour.StartingState)),
            });
        }

        [Test]
        public void VerifyTransitionsDeadEnd()
        {
            UnityEngine.Debug.Log($"State machine in state {sm.CurrentState}");
            Assert.AreEqual(sm.CurrentState, typeof(DemoStateMachineMonoBehaviour.StartingState));

            SendAndVerifyEventSequence(new (IEvent, Type)[]
            {
                (new AEvent(), typeof(DemoStateMachineMonoBehaviour.StateA)),
                (new BEvent(), typeof(DemoStateMachineMonoBehaviour.StateB)),
                (new BEvent(), typeof(DemoStateMachineMonoBehaviour.StateB)),
                (new CEvent(), typeof(DemoStateMachineMonoBehaviour.StateB)),
            });
        }

        [Test]
        public void VerifyTransitionsCycle()
        {
            UnityEngine.Debug.Log($"State machine in state {sm.CurrentState}");
            Assert.AreEqual(sm.CurrentState, typeof(DemoStateMachineMonoBehaviour.StartingState));

            SendAndVerifyEventSequence(new (IEvent, Type)[]
            {
                (new AEvent(), typeof(DemoStateMachineMonoBehaviour.StateA)),
                (new CEvent(), typeof(DemoStateMachineMonoBehaviour.StateC)),
                (new ResetEvent(), typeof(DemoStateMachineMonoBehaviour.StartingState)),
                (new AEvent(), typeof(DemoStateMachineMonoBehaviour.StateA)),
                (new CEvent(), typeof(DemoStateMachineMonoBehaviour.StateC)),
                (new ResetEvent(), typeof(DemoStateMachineMonoBehaviour.StartingState)),
            });
        }

        [Test]
        public void VerifyEnterExitCount()
        {
            UnityEngine.Debug.Log($"State machine in state {sm.CurrentState}");
            Assert.AreEqual(sm.CurrentState, typeof(DemoStateMachineMonoBehaviour.StartingState));

            Assert.AreEqual(sm.OnEntryCount[typeof(DemoStateMachineMonoBehaviour.StartingState)], 1);
            Assert.AreEqual(sm.OnExitCount.GetOrAdd(typeof(DemoStateMachineMonoBehaviour.StartingState), t => 0), 0);

            SendAndVerifyEventSequence((new CEvent(), typeof(DemoStateMachineMonoBehaviour.StateC)));

            Assert.AreEqual(sm.OnEntryCount[typeof(DemoStateMachineMonoBehaviour.StartingState)], 1);
            Assert.AreEqual(sm.OnEntryCount[typeof(DemoStateMachineMonoBehaviour.StateC)], 1);

            UnityEngine.Debug.Log(sm.OnExitCount[typeof(DemoStateMachineMonoBehaviour.StartingState)]);

            Assert.AreEqual(sm.OnExitCount[typeof(DemoStateMachineMonoBehaviour.StartingState)], 1);

            SendAndVerifyEventSequence((new ResetEvent(), typeof(DemoStateMachineMonoBehaviour.StartingState)));

            Assert.AreEqual(sm.OnEntryCount[typeof(DemoStateMachineMonoBehaviour.StartingState)], 2);
            Assert.AreEqual(sm.OnEntryCount[typeof(DemoStateMachineMonoBehaviour.StateC)], 1);
            Assert.AreEqual(sm.OnExitCount[typeof(DemoStateMachineMonoBehaviour.StartingState)], 1);

            SendAndVerifyEventSequence((new CEvent(), typeof(DemoStateMachineMonoBehaviour.StateC)));
            SendAndVerifyEventSequence((new CEvent(), typeof(DemoStateMachineMonoBehaviour.StateC)));

            Assert.AreEqual(sm.OnEntryCount[typeof(DemoStateMachineMonoBehaviour.StartingState)], 2);
            Assert.AreEqual(sm.OnEntryCount[typeof(DemoStateMachineMonoBehaviour.StateC)], 3);
            Assert.AreEqual(sm.OnExitCount[typeof(DemoStateMachineMonoBehaviour.StartingState)], 2);

            SendAndVerifyEventSequence((new ResetEvent(), typeof(DemoStateMachineMonoBehaviour.StartingState)));

            Assert.AreEqual(sm.OnEntryCount.GetOrAdd(typeof(DemoStateMachineMonoBehaviour.StateA), t => 0), 0);

            SendAndVerifyEventSequence((new ResetEvent(), typeof(DemoStateMachineMonoBehaviour.StartingState)));
            SendAndVerifyEventSequence((new AEvent(), typeof(DemoStateMachineMonoBehaviour.StateA)));

            Assert.AreEqual(sm.OnEntryCount[typeof(DemoStateMachineMonoBehaviour.StartingState)], 3);
            Assert.AreEqual(sm.OnEntryCount[typeof(DemoStateMachineMonoBehaviour.StateC)], 3);
            Assert.AreEqual(sm.OnEntryCount[typeof(DemoStateMachineMonoBehaviour.StateA)], 1);
            Assert.AreEqual(sm.OnExitCount[typeof(DemoStateMachineMonoBehaviour.StartingState)], 3);

            SendAndVerifyEventSequence((new AEvent(), typeof(DemoStateMachineMonoBehaviour.StateA)));

            Assert.AreEqual(sm.OnEntryCount[typeof(DemoStateMachineMonoBehaviour.StateA)], 1);
        }

        /// <summary>
        /// Send an event sequence to a state machine and verify the given transitions.
        /// </summary>
        /// <param name="transitions">Sequence of events and transitions
        /// to send to the state machine and expect changes.</param>
        public void SendAndVerifyEventSequence(params (IEvent, Type)[] transitions)
        {
            SendAndVerifyEventSequence(transitions, false);
        }

        /// <summary>
        /// Send an event sequence to a state machine and verify the given transitions.
        /// </summary>
        /// <param name="transitions">Sequence of events and transitions
        /// to send to the state machine and expect changes.</param>
        public void SendAndVerifyEventSequence(IEnumerable<(IEvent, Type)> transitions, bool log = true)
        {
            foreach ((IEvent, Type) tuple in transitions)
            {
                if (log)
                {
                    UnityEngine.Debug.Log($"Sending event {tuple.Item1.GetType()} to state machine");
                }

                sm.RaiseEvent(tuple.Item1);

                if (log)
                {
                    UnityEngine.Debug.Log($"State machine is now in state {sm.CurrentState}");
                }

                Assert.AreEqual(sm.CurrentState, tuple.Item2);
            }
        }
    }
}
