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
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.FSM.FixedStateMachineBehaviour"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class StateMachineBehaviourTests : TestBase
    {
        [Test]
        public void VerifyUpdateActionCounts()
        {
            DemoFixedStateMachineBehaviour sm = CreateGameObject().AddComponent<DemoFixedStateMachineBehaviour>();

            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnFixedUpdateAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnLateUpdateAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnGUIAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnEnableAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnDisableAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState)), t => 0), 0);
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnAnimatorIKAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState)), t => 0), 0);

            sm.Update();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnUpdateAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState))], 1);

            sm.FixedUpdate();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnFixedUpdateAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState))], 1);

            sm.LateUpdate();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnLateUpdateAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState))], 1);

            sm.OnGUI();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnGUIAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState))], 1);

            sm.OnEnable();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnEnableAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState))], 1);

            sm.OnDisable();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnDisableAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState))], 1);

            sm.OnAnimatorIK();
            Assert.AreEqual(sm.actionStateCounts[(typeof(OnAnimatorIKAttribute), typeof(DemoFixedStateMachineBehaviour.StartingState))], 1);

            sm.RaiseEvent(new AEvent());
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoFixedStateMachineBehaviour.StateA)), t => 0), 0);

            sm.Update();
            Assert.AreEqual(sm.actionStateCounts.GetOrAdd((typeof(OnUpdateAttribute), typeof(DemoFixedStateMachineBehaviour.StateA)), t => 0), 1);
        }
    }
}
