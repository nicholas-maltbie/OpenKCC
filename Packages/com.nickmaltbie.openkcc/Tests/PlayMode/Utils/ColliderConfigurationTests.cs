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
using nickmaltbie.OpenKCC.Utils.ColliderCast;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.PlayMode.Utils.ColliderCast
{
    /// <summary>
    /// Basic tests for <see cref="nickmaltbie.OpenKCC.Utils.ColliderCast.ColliderConfiguration"/> in edit mode.
    /// </summary>
    [TestFixture]
    public class ColliderConfigurationTests : TestBase
    {
        public static readonly ColliderType[] ColliderTypes = Enum.GetValues(typeof(ColliderType)) as ColliderType[];

        [Test]
        public void Verify_ColliderConfigSettings([ValueSource(nameof(ColliderTypes))] ColliderType type)
        {
            var config = new ColliderConfiguration
            {
                type = type,

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

            Assert.AreEqual(config.Center, Vector3.zero);
            Assert.AreEqual(config.Size, config.boxSize);
            Assert.AreEqual(config.Height, config.capsuleHeight);
            Assert.AreEqual(config.CapsuleDirection, config.capsuleDirection);
            Assert.AreEqual(config.Radius, type == ColliderType.Sphere || type == ColliderType.Capsule ? 0.5f : 0.0f);

            config.Center = Vector3.up * 2.0f;
            config.Radius = 1.0f;

            GameObject go = CreateGameObject();
            config.AttachCollider(go);
            config.AttachCollider(go);
        }
    }
}
