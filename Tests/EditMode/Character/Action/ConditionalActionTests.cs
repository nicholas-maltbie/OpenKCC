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

using Moq;
using nickmaltbie.OpenKCC.Character.Action;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character.Action
{
    /// <summary>
    /// Basic tests for ConditionalAction in edit mode.
    /// </summary>
    [TestFixture]
    public class ConditionalActionTests : TestBase
    {
        private ConditionalAction action;
        private Mock<IUnityService> unityServiceMock;
        private bool condition = true;

        [SetUp]
        public void SetUp()
        {
            unityServiceMock = new Mock<IUnityService>();
            action = new ConditionalAction();
            action.unityService = unityServiceMock.Object;
            unityServiceMock.Setup(e => e.deltaTime).Returns(1.0f);
            action.SetCondition(() => condition);
        }

        [Test]
        public void Validate_ConditionalAction_Basic()
        {
            Assert.IsTrue(action.CanPerform);
            condition = false;
            Assert.IsFalse(action.CanPerform);
        }

        [Test]
        public void Validate_ConditionalAction_CoyoteTime()
        {
            action.coyoteTime = 2.0f;
            Assert.IsTrue(action.CanPerform);
            condition = false;
            Assert.IsTrue(action.CanPerform);

            action.Update();
            Assert.IsTrue(action.CanPerform);
            unityServiceMock.Setup(e => e.deltaTime).Returns(1000);

            action.Update();
            Assert.IsFalse(action.CanPerform);
        }
    }
}
