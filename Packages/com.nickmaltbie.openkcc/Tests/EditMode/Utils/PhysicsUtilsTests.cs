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

using System.Linq;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Utils
{
    /// <summary>
    /// Basic EditMode tests for the <see cref="PhysicsUtils"/> class.
    /// </summary>
    [TestFixture]
    public class PhysicsUtilsTests : TestBase
    {
        private GameObject objectOne;
        private GameObject objectTwo;
        private GameObject objectThree;

        private GameObject[] objectList;

        [SetUp]
        public void SetUp()
        {
            RegisterGameObject(objectOne = GameObject.CreatePrimitive(PrimitiveType.Cube));
            RegisterGameObject(objectTwo = GameObject.CreatePrimitive(PrimitiveType.Cube));
            RegisterGameObject(objectThree = GameObject.CreatePrimitive(PrimitiveType.Cube));

            objectOne.transform.position = Vector3.zero;
            objectTwo.transform.position = Vector3.forward * 5;
            objectThree.transform.position = Vector3.forward * 10;

            objectList = new[] { objectOne, objectTwo, objectThree };
        }

        [Test]
        public void Validate_FilterForFirstHitAllow([NUnit.Framework.Range(0, 2, 1)] int index)
        {
            GameObject target = objectList[index];
            bool didHit = PhysicsUtils.RaycastHitAllow(
                target,
                Vector3.back * 5,
                Vector3.forward,
                Mathf.Infinity,
                ~0,
                QueryTriggerInteraction.Ignore,
                out RaycastHit hit);
            Assert.IsTrue(didHit);
            Assert.AreEqual(hit.collider.gameObject, target);
        }

        [Test]
        public void Validate_FilterForFirstHitAllow_Set([NUnit.Framework.Range(0, 0b111, 1)] int mask)
        {
            GameObject[] targets = Enumerable.Range(0, 3)
                .Where(index => (mask & (1 << index)) > 0)
                .Select(index => objectList[index])
                .ToArray();

            bool didHit = PhysicsUtils.RaycastHitAllow(
                targets,
                Vector3.back * 5,
                Vector3.forward,
                Mathf.Infinity,
                ~0,
                QueryTriggerInteraction.Ignore,
                out RaycastHit hit);

            if (targets.Length > 0)
            {
                Assert.IsTrue(didHit);
                Assert.AreEqual(hit.collider.gameObject, targets[0]);
            }
            else
            {
                Assert.IsFalse(didHit);
                Assert.AreEqual(hit.distance, Mathf.Infinity);
            }
        }

        [Test]
        public void Validate_FilterForFirstHitIgnore([NUnit.Framework.Range(0, 2, 1)] int index)
        {
            GameObject ignore = objectList[index];
            bool didHit = PhysicsUtils.RaycastFirstHitIgnore(
                ignore,
                Vector3.back * 5,
                Vector3.forward,
                Mathf.Infinity,
                ~0,
                QueryTriggerInteraction.Ignore,
                out RaycastHit hit);
            Assert.IsTrue(didHit);
            Assert.AreEqual(hit.collider.gameObject, index == 0 ? objectTwo : objectOne);
        }

        [Test]
        public void Validate_FilterForFirstHitIgnore_Set([NUnit.Framework.Range(0, 0b111, 1)] int mask)
        {
            GameObject[] ignoreList = Enumerable.Range(0, 3)
                .Where(index => (mask & (1 << index)) > 0)
                .Select(index => objectList[index])
                .ToArray();
            GameObject[] allowList = Enumerable.Range(0, 3)
                .Where(index => (mask & (1 << index)) == 0)
                .Select(index => objectList[index])
                .ToArray();

            bool didHit = PhysicsUtils.RaycastFirstHitIgnore(
                ignoreList,
                Vector3.back * 5,
                Vector3.forward,
                Mathf.Infinity,
                ~0,
                QueryTriggerInteraction.Ignore,
                out RaycastHit hit);

            if (allowList.Length > 0)
            {
                Assert.IsTrue(didHit);
                Assert.AreEqual(hit.collider.gameObject, allowList[0]);
            }
            else
            {
                Assert.IsFalse(didHit);
                Assert.AreEqual(hit.distance, Mathf.Infinity);
            }
        }

        [Test]
        public void Validate_SphereCast_FilterForFirstHit([NUnit.Framework.Range(0, 2, 1)] int index)
        {
            GameObject target = objectList[index];
            bool didHit = PhysicsUtils.SphereCastAllow(
                target,
                Vector3.back * 5,
                1.0f,
                Vector3.forward,
                Mathf.Infinity,
                ~0,
                QueryTriggerInteraction.Ignore,
                out RaycastHit allowHit);
            Assert.IsTrue(didHit);
            Assert.AreEqual(allowHit.collider.gameObject, target);

            didHit = PhysicsUtils.SphereCastFirstHitIgnore(
                target,
                Vector3.back * 5,
                1.0f,
                Vector3.forward,
                Mathf.Infinity,
                ~0,
                QueryTriggerInteraction.Ignore,
                out RaycastHit ignoreHit);
            Assert.IsTrue(didHit);
            Assert.AreEqual(ignoreHit.collider.gameObject, index == 0 ? objectTwo : objectOne);
        }

        [Test]
        public void Validate_SphereCast_FilterForFirstHit_Set([NUnit.Framework.Range(0, 0b111, 1)] int mask)
        {
            GameObject[] ignoreList = Enumerable.Range(0, 3)
                .Where(index => (mask & (1 << index)) > 0)
                .Select(index => objectList[index])
                .ToArray();
            GameObject[] allowList = Enumerable.Range(0, 3)
                .Where(index => (mask & (1 << index)) == 0)
                .Select(index => objectList[index])
                .ToArray();

            bool didHit = PhysicsUtils.SphereCastFirstHitIgnore(
                ignoreList,
                Vector3.back * 5,
                1.0f,
                Vector3.forward,
                Mathf.Infinity,
                ~0,
                QueryTriggerInteraction.Ignore,
                out RaycastHit ignoreHit);

            if (allowList.Length > 0)
            {
                Assert.IsTrue(didHit);
                Assert.AreEqual(ignoreHit.collider.gameObject, allowList[0]);
            }
            else
            {
                Assert.IsFalse(didHit);
                Assert.AreEqual(ignoreHit.distance, Mathf.Infinity);
            }
        }
    }
}
