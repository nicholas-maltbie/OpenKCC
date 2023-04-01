// Copyright (C) 2023 Nicholas Maltbie
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.OpenKCC.Utils.ColliderCast;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Utils.ColliderCast
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.Utils.ColliderCast.PrimitiveColliderCastTests"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class PrimitiveColliderCastTests : TestBase
    {
        public static readonly ColliderType[] ColliderTypes = Enum.GetValues(typeof(ColliderType)) as ColliderType[];

        private PrimitiveColliderCast colliderCast;

        [SetUp]
        public void SetUp()
        {
            GameObject go = CreateGameObject();
            colliderCast = go.AddComponent<PrimitiveColliderCast>();
            colliderCast.config = new ColliderConfiguration
            {
                type = ColliderType.Box,
                
                boxCenter = Vector3.zero,
                boxSize = Vector3.one,

                capsuleCenter = Vector3.zero,
                capsuleHeight = 2.0f,
                capsuleRadius = 0.5f,
                capsuleDirection = CapsuleDirection.Y,

                sphereCenter = Vector3.zero,
                sphereRadius = 0.5f,

                pointCenter = Vector3.zero,
            };
        }

        [Test]
        public void Validate_GetBottom([ValueSource(nameof(ColliderTypes))] ColliderType colliderType)
        {
            Vector3 bottom = Vector3.zero;
            colliderCast.config.type = colliderType;
            switch (colliderType)
            {
                case ColliderType.Box:
                    bottom = Vector3.down * colliderCast.config.boxSize.y / 2;
                    break;
                case ColliderType.Sphere:
                    bottom = Vector3.down * colliderCast.config.sphereRadius;
                    break;
                case ColliderType.Capsule:
                    bottom = Vector3.down * colliderCast.config.capsuleHeight / 2;
                    break;
            }

            TestUtils.AssertInBounds(
                colliderCast.GetBottom(Vector3.zero, Quaternion.identity),
                bottom,
                0.05f);
            Quaternion flipped = Quaternion.Euler(0, 0, 180);
            TestUtils.AssertInBounds(
                colliderCast.GetBottom(Vector3.zero, flipped),
                flipped * bottom,
                0.05f);
            Quaternion rotate = Quaternion.Euler(0, 0, 90);
            TestUtils.AssertInBounds(
                colliderCast.GetBottom(Vector3.zero, Quaternion.Euler(0, 0, 90)),
                rotate * bottom,
                0.05f);
        }

        [Test]
        public void Validate_GetHits_NoHits([ValueSource(nameof(ColliderTypes))] ColliderType colliderType)
        {
            colliderCast.config.type = colliderType;
            bool didHit = colliderCast.CastSelf(Vector3.zero, Quaternion.identity, Vector3.forward, 1, out _);
            Assert.IsFalse(didHit);
        }


        [UnityTest]
        public IEnumerator Validate_GetHits_OneHit(
            [ValueSource(nameof(ColliderTypes))] ColliderType colliderType,
            [NUnit.Framework.Range(1, 10, 2)] float dist)
        {
            colliderCast.config.type = colliderType;
            GameObject target = MakeCube(Vector3.forward * dist);
            yield return null;
            Assert.IsTrue(colliderCast.CastSelf(Vector3.zero, Quaternion.identity, Vector3.forward, dist + 1, out IRaycastHit hit));
            Assert.IsTrue(hit.rigidbody.gameObject == target);
        }

        [Test]
        public void Validate_GetOverlapping(
            [ValueSource(nameof(ColliderTypes))] ColliderType colliderType,
            [NUnit.Framework.Range(1, 10, 2)] int numOverlap)
        {
            colliderCast.config.type = colliderType;
            GameObject[] targets = Enumerable.Range(0, numOverlap).Select(_ => MakeCube()).ToArray();
            IEnumerable<Collider> overlapping = colliderCast.GetOverlapping(Vector3.zero, Quaternion.identity);
            Assert.IsTrue(new HashSet<GameObject>(overlapping.Select(o => o.gameObject)).SetEquals(targets));
        }

        [Test]
        public void Validate_PushOutOverlapping_NoOverlap([ValueSource(nameof(ColliderTypes))] ColliderType colliderType)
        {
            colliderCast.config.type = colliderType;
            colliderCast.Awake();
            TestUtils.AssertInBounds(
                colliderCast.PushOutOverlapping(Vector3.zero, Quaternion.identity, 10.0f),
                Vector3.zero,
                0.01f);
        }

        [UnityTest]
        public IEnumerator Validate_PushOutOverlapping_OneOverlap([ValueSource(nameof(ColliderTypes))] ColliderType colliderType)
        {
            colliderCast.config.type = colliderType;
            colliderCast.Awake();
            MakeCube();
            yield return null;
            TestUtils.AssertInBounds(
                colliderCast.PushOutOverlapping(Vector3.zero, Quaternion.identity, 10.0f).magnitude,
                colliderType == ColliderType.Point ? 0.5f : 1.0f,
                0.01f);
        }

        [UnityTest]
        public IEnumerator Validate_PushOutOverlapping_MultipleOverlap([ValueSource(nameof(ColliderTypes))] ColliderType colliderType)
        {
            colliderCast.config.type = colliderType;
            colliderCast.Awake();
            MakeCube();
            var target1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            target1.transform.localScale = Vector3.one;
            RegisterGameObject(target1);

            yield return null;
            TestUtils.AssertInBounds(
                colliderCast.PushOutOverlapping(Vector3.zero, Quaternion.identity, 10.0f).magnitude,
                colliderType == ColliderType.Point ? 0.5f : 1.0f,
                0.01f);
        }

        [Test]
        public void Validate_DoRaycastInDirection_NoHit([ValueSource(nameof(ColliderTypes))] ColliderType colliderType)
        {
            colliderCast.config.type = colliderType;
            Assert.IsFalse(colliderCast.DoRaycastInDirection(Vector3.zero, Vector3.forward, 1, out IRaycastHit _));
        }

        [UnityTest]
        public IEnumerator Validate_DoRaycastInDirection_Hit([ValueSource(nameof(ColliderTypes))] ColliderType colliderType)
        {
            colliderCast.config.type = colliderType;
            GameObject target = MakeCube(Vector3.forward * 3);

            yield return null;
            Assert.IsTrue(colliderCast.CastSelf(Vector3.zero, Quaternion.identity, Vector3.forward, 5, out IRaycastHit hit));
            Assert.IsTrue(hit.rigidbody.gameObject == target);
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
