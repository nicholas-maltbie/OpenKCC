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
using nickmaltbie.OpenKCC.TestCommon;
using NUnit.Framework;

namespace nickmaltbie.OpenKCC.Tests.EditMode.FSM
{
    /// <summary>
    /// Demo state machine for 
    /// </summary>
    public class DemoFixedStateMachineMonoBehaviour : FixedStateMachineBehaviour
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
            UnityEngine.Debug.Log("On Enter Starting State Invoked");
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

    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.FSM.FixedStateMachineBehaviour"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class StateMachineBehaviourTests : TestBase
    {
        [Test]
        public void VerifyUpdateActionCounts()
        {
            DemoFixedStateMachineMonoBehaviour sm = CreateGameObject().AddComponent<DemoFixedStateMachineMonoBehaviour>();

            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnFixedUpdateAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnLateUpdateAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnGUIAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnEnableAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnDisableAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnAnimatorIKAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState)), t => 0), 0);

            sm.Update();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnUpdateAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState))], 1);

            sm.FixedUpdate();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnFixedUpdateAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState))], 1);

            sm.LateUpdate();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnLateUpdateAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState))], 1);

            sm.OnGUI();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnGUIAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState))], 1);

            sm.OnEnable();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnEnableAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState))], 1);

            sm.OnDisable();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnDisableAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState))], 1);

            sm.OnAnimatorIK();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnAnimatorIKAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StartingState))], 1);

            sm.RaiseEvent(new AEvent());
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StateA)), t => 0), 0);

            sm.Update();
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoFixedStateMachineMonoBehaviour.StateA)), t => 0), 1);
        }
    }
}
