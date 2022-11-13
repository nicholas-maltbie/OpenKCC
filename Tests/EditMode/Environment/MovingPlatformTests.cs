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

using System.Collections.Generic;
using Moq;
using nickmaltbie.OpenKCC.Environment;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Environment
{
    /// <summary>
    /// Basic tests for MovingPlatform in edit mode.
    /// </summary>
    [TestFixture]
    public class MovingPlatformTests : TestBase
    {
        private Mock<IUnityService> mockUnityService;
        private MovingPlatform platform;
        private Rigidbody rb;

        [SetUp]
        public void SetUp()
        {
            GameObject go = CreateGameObject();
            rb = go.AddComponent<Rigidbody>();
            platform = go.AddComponent<MovingPlatform>();
            platform.untiyService = (mockUnityService = new Mock<IUnityService>()).Object;

            mockUnityService.Setup(e => e.fixedDeltaTime).Returns(1.0f);
            mockUnityService.Setup(e => e.deltaTime).Returns(1.0f);
        }

        [Test]
        public void Validate_MovingPlatform_NullOrEmpty([Values] bool isNull)
        {
            platform.targetsList = isNull ? null : new List<Transform>();
            platform.FixedUpdate();
            Assert.IsNull(platform.CurrentTarget);
            Assert.AreEqual(platform.currentTargetIndex, 0);
            TestUtils.AssertInBounds(rb.velocity, Vector3.zero);
        }

        [Test]
        public void Validate_MovingPlatform_OneTarget(
            [Values] bool isContinuous,
            [Values] bool overlapTarget)
        {
            GameObject target = CreateGameObject();
            target.transform.position = overlapTarget ? Vector3.zero : Vector3.forward * 10;
            platform.isContinuous = isContinuous;

            platform.targetsList = new List<Transform>(new[] { target.transform });
            platform.FixedUpdate();
            Assert.AreEqual(platform.CurrentTarget, target.transform);
            Assert.AreEqual(platform.currentTargetIndex, 0);

            if (!isContinuous)
            {
                if (overlapTarget)
                {
                    TestUtils.AssertInBounds(rb.position, Vector3.zero);
                }
                else
                {
                    Assert.IsTrue(rb.position.magnitude > 0.0f);
                }
            }
        }

        [Test]
        public void Validate_MovingPlatform_MultipleTargets()
        {
            GameObject target1 = CreateGameObject();
            GameObject target2 = CreateGameObject();
            target1.transform.position = Vector3.forward * 10;
            target2.transform.position = Vector3.right * 10;
            platform.isContinuous = false;

            platform.targetsList = new List<Transform>(new[] { target1.transform, target2.transform });

            for (int targetIdx = 0; targetIdx < platform.targetsList.Count; targetIdx++)
            {

                for (int i = 0; i < 100 && platform.CurrentTarget == platform.targetsList[targetIdx]; i++)
                {
                    platform.FixedUpdate();
                }

                Assert.IsTrue(platform.CurrentTarget == platform.targetsList[(targetIdx + 1) % platform.targetsList.Count]);
            }
        }
    }
}
