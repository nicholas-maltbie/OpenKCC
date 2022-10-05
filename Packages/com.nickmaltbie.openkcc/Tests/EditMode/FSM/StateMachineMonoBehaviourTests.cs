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

using nickmaltbie.OpenKCC.FSM.Attributes;
using nickmaltbie.OpenKCC.TestCommon;
using NUnit.Framework;

namespace nickmaltbie.OpenKCC.Tests.EditMode.FSM
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.FSM.StateMachineMonoBehaviour"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class StateMachineMonoBehaviourTests : TestBase
    {
        [Test]
        public void VerifyUpdateActionCounts()
        {
            DemoStateMachineMonoBehaviour sm = CreateGameObject().AddComponent<DemoStateMachineMonoBehaviour>();

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
    }
}
