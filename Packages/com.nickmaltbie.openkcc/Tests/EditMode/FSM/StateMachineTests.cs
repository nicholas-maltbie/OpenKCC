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
using nickmaltbie.OpenKCC.TestCommon;
using NUnit.Framework;

namespace nickmaltbie.OpenKCC.Tests.EditMode.FSM
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.FSM.FixedStateMachine"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class StateMachineTests : TestBase
    {
        public DemoFixedStateMachine demoStateMachine;

        [SetUp]
        public void SetUp()
        {
            demoStateMachine = new DemoFixedStateMachine();
        }

        [Test]
        public void RebuildCache()
        {
            FSMUtils.ActionCache = new ConcurrentDictionary<Type, Dictionary<(Type, Type), MethodInfo>>();
            FSMUtils.TransitionCache = new ConcurrentDictionary<Type, Dictionary<(Type, Type), Type>>();
            FSMUtils.EventCache = new ConcurrentDictionary<Type, Dictionary<(Type, Type), List<MethodInfo>>>();
            FSMUtils.SetupCache(typeof(DemoFixedStateMachine));
            FSMUtils.SetupCache(typeof(DemoFixedStateMachineBehaviour));
        }

        [Test]
        public void VerifyTransitionNoEvent()
        {
            UnityEngine.Debug.Log($"State machine in state {demoStateMachine.CurrentState}");
            Assert.AreEqual(demoStateMachine.CurrentState, typeof(DemoFixedStateMachine.StartingState));

            SendAndVerifyEventSequence(new (IEvent, Type)[]
            {
                (new EmptyEvent(), typeof(DemoFixedStateMachine.StartingState)),
                (new EmptyEvent(), typeof(DemoFixedStateMachine.StartingState)),
                (new EmptyEvent(), typeof(DemoFixedStateMachine.StartingState)),
                (new EmptyEvent(), typeof(DemoFixedStateMachine.StartingState)),
                (new EmptyEvent(), typeof(DemoFixedStateMachine.StartingState)),
            });
        }

        [Test]
        public void VerifyTransitionsDeadEnd()
        {
            UnityEngine.Debug.Log($"State machine in state {demoStateMachine.CurrentState}");
            Assert.AreEqual(demoStateMachine.CurrentState, typeof(DemoFixedStateMachine.StartingState));

            SendAndVerifyEventSequence(new (IEvent, Type)[]
            {
                (new AEvent(), typeof(DemoFixedStateMachine.StateA)),
                (new BEvent(), typeof(DemoFixedStateMachine.StateB)),
                (new BEvent(), typeof(DemoFixedStateMachine.StateB)),
                (new CEvent(), typeof(DemoFixedStateMachine.StateB)),
            });
        }

        [Test]
        public void VerifyTransitionsCycle()
        {
            UnityEngine.Debug.Log($"State machine in state {demoStateMachine.CurrentState}");
            Assert.AreEqual(demoStateMachine.CurrentState, typeof(DemoFixedStateMachine.StartingState));

            SendAndVerifyEventSequence(new (IEvent, Type)[]
            {
                (new AEvent(), typeof(DemoFixedStateMachine.StateA)),
                (new CEvent(), typeof(DemoFixedStateMachine.StateC)),
                (new ResetEvent(), typeof(DemoFixedStateMachine.StartingState)),
                (new AEvent(), typeof(DemoFixedStateMachine.StateA)),
                (new CEvent(), typeof(DemoFixedStateMachine.StateC)),
                (new ResetEvent(), typeof(DemoFixedStateMachine.StartingState)),
            });
        }

        [Test]
        public void VerifyEnterExitCount()
        {
            UnityEngine.Debug.Log($"State machine in state {demoStateMachine.CurrentState}");
            Assert.AreEqual(demoStateMachine.CurrentState, typeof(DemoFixedStateMachine.StartingState));

            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoFixedStateMachine.StartingState)], 1);
            Assert.AreEqual(demoStateMachine.OnExitCount.GetOrAdd(typeof(DemoFixedStateMachine.StartingState), t => 0), 0);

            SendAndVerifyEventSequence((new CEvent(), typeof(DemoFixedStateMachine.StateC)));

            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoFixedStateMachine.StartingState)], 1);
            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoFixedStateMachine.StateC)], 1);

            UnityEngine.Debug.Log(demoStateMachine.OnExitCount[typeof(DemoFixedStateMachine.StartingState)]);

            Assert.AreEqual(demoStateMachine.OnExitCount[typeof(DemoFixedStateMachine.StartingState)], 1);

            SendAndVerifyEventSequence((new ResetEvent(), typeof(DemoFixedStateMachine.StartingState)));

            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoFixedStateMachine.StartingState)], 2);
            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoFixedStateMachine.StateC)], 1);
            Assert.AreEqual(demoStateMachine.OnExitCount[typeof(DemoFixedStateMachine.StartingState)], 1);

            SendAndVerifyEventSequence((new CEvent(), typeof(DemoFixedStateMachine.StateC)));
            SendAndVerifyEventSequence((new CEvent(), typeof(DemoFixedStateMachine.StateC)));

            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoFixedStateMachine.StartingState)], 2);
            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoFixedStateMachine.StateC)], 3);
            Assert.AreEqual(demoStateMachine.OnExitCount[typeof(DemoFixedStateMachine.StartingState)], 2);

            SendAndVerifyEventSequence((new ResetEvent(), typeof(DemoFixedStateMachine.StartingState)));

            Assert.AreEqual(demoStateMachine.OnEntryCount.GetOrAdd(typeof(DemoFixedStateMachine.StateA), t => 0), 0);

            SendAndVerifyEventSequence((new ResetEvent(), typeof(DemoFixedStateMachine.StartingState)));
            SendAndVerifyEventSequence((new AEvent(), typeof(DemoFixedStateMachine.StateA)));

            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoFixedStateMachine.StartingState)], 3);
            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoFixedStateMachine.StateC)], 3);
            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoFixedStateMachine.StateA)], 1);
            Assert.AreEqual(demoStateMachine.OnExitCount[typeof(DemoFixedStateMachine.StartingState)], 3);

            SendAndVerifyEventSequence((new AEvent(), typeof(DemoFixedStateMachine.StateA)));

            Assert.AreEqual(demoStateMachine.OnEntryCount[typeof(DemoFixedStateMachine.StateA)], 1);
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

                demoStateMachine.RaiseEvent(tuple.Item1);

                if (log)
                {
                    UnityEngine.Debug.Log($"State machine is now in state {demoStateMachine.CurrentState}");
                }

                Assert.AreEqual(demoStateMachine.CurrentState, tuple.Item2);
            }
        }
    }
}
