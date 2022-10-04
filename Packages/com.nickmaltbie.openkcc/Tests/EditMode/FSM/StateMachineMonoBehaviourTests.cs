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

using nickmaltbie.OpenKCC.FSM;
using nickmaltbie.OpenKCC.FSM.Attributes;
using nickmaltbie.OpenKCC.TestCommon;
using NUnit.Framework;

namespace nickmaltbie.OpenKCC.Tests.EditMode.FSM
{
    public class ExampleStateMachineMonoBehaviour : StateMachineMonoBehaviour
    {
        public DemoStateMachine _stateMachine = new DemoStateMachine();

        public override IStateMachine StateMachine => _stateMachine;
    }

    /// <summary>
    /// Basic tests for <see cref="StateMachineMonoBehaviour"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class StateMachineMonoBehaviourTests : TestBase
    {
        public ExampleStateMachineMonoBehaviour stateMachineBehaviour;

        [SetUp]
        public void SetUp()
        {
            stateMachineBehaviour = base.CreateGameObject().AddComponent<ExampleStateMachineMonoBehaviour>();
        }

        [Test]
        public void VerifyUpdateActionCounts()
        {
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts.GetOrAdd((typeof(OnFixedUpdateAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts.GetOrAdd((typeof(OnLateUpdateAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts.GetOrAdd((typeof(OnGUIAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts.GetOrAdd((typeof(OnEnableAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts.GetOrAdd((typeof(OnDisableAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts.GetOrAdd((typeof(OnAnimatorIKAttribute), typeof(DemoStateMachine.StartingState)), t => 0), 0);

            stateMachineBehaviour.Update();
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts[(typeof(OnUpdateAttribute), typeof(DemoStateMachine.StartingState))], 1);

            stateMachineBehaviour.FixedUpdate();
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts[(typeof(OnFixedUpdateAttribute), typeof(DemoStateMachine.StartingState))], 1);

            stateMachineBehaviour.LateUpdate();
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts[(typeof(OnLateUpdateAttribute), typeof(DemoStateMachine.StartingState))], 1);

            stateMachineBehaviour.OnGUI();
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts[(typeof(OnGUIAttribute), typeof(DemoStateMachine.StartingState))], 1);

            stateMachineBehaviour.OnEnable();
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts[(typeof(OnEnableAttribute), typeof(DemoStateMachine.StartingState))], 1);

            stateMachineBehaviour.OnDisable();
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts[(typeof(OnDisableAttribute), typeof(DemoStateMachine.StartingState))], 1);

            stateMachineBehaviour.OnAnimatorIK();
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts[(typeof(OnAnimatorIKAttribute), typeof(DemoStateMachine.StartingState))], 1);

            stateMachineBehaviour._stateMachine.RaiseEvent(new AEvent());
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoStateMachine.StateA)), t => 0), 0);

            stateMachineBehaviour.Update();
            Assert.AreEqual(stateMachineBehaviour._stateMachine.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoStateMachine.StateA)), t => 0), 1);
        }
    }
}
