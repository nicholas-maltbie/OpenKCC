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
using System.Linq;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Utils
{
    /// <summary>
    /// Basic tests for CapsuleColliderCast in edit mode.
    /// </summary>
    [TestFixture]
    public class CapsuleColliderCastTests : TestBase
    {
        private CapsuleColliderCast colliderCast;

        private CapsuleCollider collider;

        [SetUp]
        public void SetUp()
        {
            GameObject go = CreateGameObject();
            collider = go.AddComponent<CapsuleCollider>();
            collider.radius = 0.5f;
            collider.center = Vector3.zero;
            collider.height = 2;
            colliderCast = go.AddComponent<CapsuleColliderCast>();
        }

        [Test]
        public void Validate_GetParams()
        {
            (Vector3 top, Vector3 bottom, float radius, float height) =
                colliderCast.GetParams(Vector3.zero, Quaternion.Euler(Vector3.zero));

            Assert.AreEqual(new Vector3(0, collider.height / 2 - collider.radius, 0), top);
            Assert.AreEqual(new Vector3(0, -collider.height / 2 + collider.radius, 0), bottom);
            Assert.AreEqual(collider.radius, radius);
            Assert.AreEqual(collider.height, height);
        }

        [Test]
        public void Validate_GetBottom()
        {
            TestUtils.AssertInBounds(
                colliderCast.GetBottom(Vector3.zero, Quaternion.identity),
                new Vector3(0, -collider.height / 2, 0),
                0.05f);
            TestUtils.AssertInBounds(
                colliderCast.GetBottom(Vector3.zero, Quaternion.Euler(0, 0, 180)),
                new Vector3(0, collider.height / 2, 0),
                0.05f);
            TestUtils.AssertInBounds(
                colliderCast.GetBottom(Vector3.zero, Quaternion.Euler(0, 0, 90)),
                new Vector3(collider.height / 2, 0, 0),
                0.05f);
        }

        [Test]
        public void Validate_GetHits_NoHits()
        {
            bool didHit = colliderCast.CastSelf(Vector3.zero, Quaternion.identity, Vector3.forward, 1, out _);
            Assert.IsFalse(didHit);
        }

        [Test]
        public void Validate_GetHits_OneHit([NUnit.Framework.Range(1, 10, 2)] float dist)
        {
            GameObject target = MakeCube(Vector3.forward * dist);
            Assert.IsTrue(colliderCast.CastSelf(Vector3.zero, Quaternion.identity, Vector3.forward, dist + 1, out IRaycastHit hit));
            Assert.IsTrue(hit.rigidbody.gameObject == target);
        }

        [Test]
        public void Validate_GetOverlapping([NUnit.Framework.Range(1, 10, 2)] int numOverlap)
        {
            GameObject[] targets = Enumerable.Range(0, numOverlap).Select(_ => MakeCube()).ToArray();
            IEnumerable<Collider> overlapping = colliderCast.GetOverlapping(Vector3.zero, Quaternion.identity);
            Assert.IsTrue(new HashSet<GameObject>(overlapping.Select(o => o.gameObject)).SetEquals(targets));
        }

        [Test]
        public void Validate_PushOutOverlapping_NoOverlap()
        {
            TestUtils.AssertInBounds(
                colliderCast.PushOutOverlapping(Vector3.zero, Quaternion.identity, 10.0f),
                Vector3.zero,
                0.01f);
        }

        [Test]
        public void Validate_PushOutOverlapping_OneOverlap()
        {
            MakeCube();
            TestUtils.AssertInBounds(
                colliderCast.PushOutOverlapping(Vector3.zero, Quaternion.identity, 10.0f).magnitude,
                1.0f,
                0.01f);
        }

        [Test]
        public void Validate_PushOutOverlapping_MultipleOverlap()
        {
            MakeCube();
            var target1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            target1.transform.localScale = Vector3.one * 3;
            RegisterGameObject(target1);
            TestUtils.AssertInBounds(
                colliderCast.PushOutOverlapping(Vector3.zero, Quaternion.identity, 10.0f).magnitude,
                1.0f,
                0.01f);
        }

        [Test]
        public void Validate_DoRaycastInDirection_NoHit()
        {
            Assert.IsFalse(colliderCast.DoRaycastInDirection(Vector3.zero, Vector3.forward, 1, out IRaycastHit _));
        }

        [Test]
        public void Validate_DoRaycastInDirection_Hit()
        {
            GameObject target = MakeCube(Vector3.forward * 1);
            Assert.IsTrue(colliderCast.CastSelf(Vector3.zero, Quaternion.identity, Vector3.forward, 5, out IRaycastHit hit));
            Assert.IsTrue(hit.rigidbody.gameObject == target);
        }

        [Test]
        public void Validate_DebugCapsuleMesh()
        {
            Assert.AreEqual(colliderCast.DebugCapsuleMesh, colliderCast.DebugCapsuleMesh);
        }

        private GameObject MakeCube(Vector3? position = null)
        {
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.AddComponent<BoxCollider>();
            target.AddComponent<Rigidbody>();
            RegisterGameObject(target);
            target.transform.position = position ?? Vector3.zero;
            return target;
        }
    }
}
