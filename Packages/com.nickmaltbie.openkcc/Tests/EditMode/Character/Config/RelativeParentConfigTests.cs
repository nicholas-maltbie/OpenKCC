﻿// Copyright (C) 2023 Nicholas Maltbie
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

using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Environment.MovingGround;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character.Config
{
    [TestFixture]
    public class RelativeParentConfigTests : TestBase
    {
        private RelativeParentConfig parent;

        [SetUp]
        public void SetUp()
        {
            parent = new RelativeParentConfig();
        }

        [Test]
        public void Validate_RelativeParentConfig_NoDelta()
        {
            Assert.AreEqual(parent.DeltaPosition(Vector3.zero), Vector3.zero);
            Assert.AreEqual(parent.DeltaRotation(Quaternion.identity), Quaternion.identity);
        }

        [Test]
        public void Validate_RelativeParentConfig_AttachDetach()
        {
            Transform ground = CreateGameObject().transform;
            Assert.IsFalse(parent.OnMovingGround);
            Assert.AreEqual(null, parent.PreviousParent);

            parent.UpdateMovingGround(
                Vector3.zero,
                Quaternion.identity,
                new KCCGroundedState(KCCUtils.Epsilon, true, 0, Vector3.up, Vector3.zero, ground.gameObject),
                Vector3.zero,
                0.0f);

            Assert.IsTrue(parent.OnMovingGround);
            Assert.AreEqual(ground, parent.PreviousParent);

            parent.UpdateMovingGround(
                Vector3.zero,
                Quaternion.identity,
                new KCCGroundedState(KCCUtils.Epsilon, true, 0, Vector3.up, Vector3.zero, ground.gameObject),
                Vector3.zero,
                0.0f);

            Assert.IsTrue(parent.OnMovingGround);
            Assert.AreEqual(ground, parent.PreviousParent);

            parent.UpdateMovingGround(
                Vector3.zero,
                Quaternion.identity,
                new KCCGroundedState(Mathf.Infinity, false, 0, Vector3.up, Vector3.zero, null),
                Vector3.zero,
                0.0f);

            Assert.IsFalse(parent.OnMovingGround);
            Assert.AreEqual(null, parent.PreviousParent);
        }

        [Test]
        public void Validate_RelativeParentConfig_MovingGround()
        {
            GameObject go = CreateGameObject();
            MovementTracking tracking = go.AddComponent<MovementTracking>();
            tracking.shouldAttach = false;
            Transform ground = go.transform;

            Assert.IsFalse(parent.OnMovingGround);
            Assert.AreEqual(null, parent.PreviousParent);

            parent.UpdateMovingGround(
                Vector3.zero,
                Quaternion.identity,
                new KCCGroundedState(KCCUtils.Epsilon, true, 0, Vector3.up, Vector3.zero, ground.gameObject),
                Vector3.zero,
                0.0f);

            Assert.IsFalse(parent.OnMovingGround);
            Assert.AreEqual(null, parent.PreviousParent);
        }

        [Test]
        public void Validate_RelativeParentConfig_DistanceDelta()
        {
            // Attach to ground with relative position of zero
            Transform player = CreateGameObject().transform;
            Transform ground = CreateGameObject().transform;
            parent.UpdateMovingGround(
                Vector3.zero,
                Quaternion.identity,
                new KCCGroundedState(KCCUtils.Epsilon, true, 0, Vector3.up, Vector3.zero, ground.gameObject),
                Vector3.zero,
                0.0f);

            // Delta should initially be zero
            Assert.AreEqual(Vector3.zero, parent.DeltaPosition(Vector3.zero));
            Assert.AreEqual(Quaternion.identity, parent.DeltaRotation(Quaternion.identity));

            // Move the ground forward one unit
            ground.position += Vector3.forward;

            // Relative position should move forward one unit
            Assert.AreEqual(Vector3.forward, parent.DeltaPosition(Vector3.zero));
            Assert.AreEqual(Quaternion.identity, parent.DeltaRotation(Quaternion.identity));

            // Moving the player to follow ground should reset this
            parent.FollowGround(player);
            Assert.AreEqual(Vector3.forward, player.transform.position);
            Assert.AreEqual(Quaternion.identity, player.transform.rotation);

            // Delta from this new position should be zero
            Assert.AreEqual(Vector3.zero, parent.DeltaPosition(player.position));
            Assert.AreEqual(Quaternion.identity, parent.DeltaRotation(player.rotation));
        }
    }
}
