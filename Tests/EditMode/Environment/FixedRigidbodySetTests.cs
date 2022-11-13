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
using nickmaltbie.OpenKCC.Environment;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Environment
{
    /// <summary>
    /// Basic tests for FixedRigidbodySet in edit mode.
    /// </summary>
    [TestFixture]
    public class FixedRigidbodySetTests : TestBase
    {
        private Mock<IUnityService> mockUnityService;
        private Transform bodyTransform;
        private Transform parentTransform;
        private FixedRigidbodySet fixedRigidbody;
        private Rigidbody rigidbody;

        [SetUp]
        public void SetUp()
        {
            mockUnityService = new Mock<IUnityService>();

            GameObject go = CreateGameObject();
            GameObject parent = CreateGameObject();

            bodyTransform = go.transform;
            parentTransform = parent.transform;

            go.transform.SetParent(parent.transform);
            go.AddComponent<SphereCollider>();
            rigidbody = go.AddComponent<Rigidbody>();
            fixedRigidbody = go.AddComponent<FixedRigidbodySet>();

            rigidbody.isKinematic = true;

            fixedRigidbody.unityService = mockUnityService.Object;
            fixedRigidbody.Start();
        }

        [Test]
        public void Validate_BasicMovement(
            [NUnit.Framework.Range(0.5f, 2.5f, 2)] float deltaTime,
            [NUnit.Framework.Range(1, 3, 2)] float speed,
            [Values] bool isLocal,
            [ValueSource(nameof(TestDirections))] Vector3 dir,
            [Values] bool isContinuous)
        {
            mockUnityService.Setup(e => e.fixedDeltaTime).Returns(deltaTime);
            fixedRigidbody.localTranslation = isLocal;
            fixedRigidbody.linearVelocity = speed * dir;
            fixedRigidbody.isContinuous = isContinuous;

            Vector3 start = rigidbody.position;
            fixedRigidbody.FixedUpdate();
            Vector3 finish = rigidbody.position;

            Vector3 delta = finish - start;

            if (!isContinuous)
            {
                TestUtils.AssertInBounds(delta, speed * dir * deltaTime, 0.001f);
            }
            else
            {
                TestUtils.AssertInBounds(delta, Vector3.zero, 0.001f);
            }
        }

        [Test]
        public void Validate_BasicRotation(
            [NUnit.Framework.Range(0.5f, 2.5f, 2)] float deltaTime,
            [NUnit.Framework.Range(1, 3, 2)] float speed,
            [Values] bool isLocal,
            [ValueSource(nameof(TestDirections))] Vector3 dir,
            [Values] bool isContinuous)
        {
            mockUnityService.Setup(e => e.fixedDeltaTime).Returns(deltaTime);
            fixedRigidbody.localRotation = isLocal;
            fixedRigidbody.angularVelocity = speed * dir;
            fixedRigidbody.isContinuous = isContinuous;

            Quaternion start = rigidbody.rotation;
            fixedRigidbody.FixedUpdate();
            Quaternion finish = rigidbody.rotation;

            float angleDelta = Quaternion.Angle(start, finish);

            if (!isContinuous)
            {
                TestUtils.AssertInBounds(angleDelta, dir.normalized.magnitude * speed * deltaTime, 0.1f);
            }
            else
            {
                TestUtils.AssertInBounds(angleDelta, 0.0f, 0.1f);
            }
        }
    }
}
