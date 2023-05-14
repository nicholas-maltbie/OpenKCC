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

using nickmaltbie.OpenKCC.Animation;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Character.Config
{
    [TestFixture]
    public class RelativeParentConfigWithFeetTests : TestBase
    {
        private RelativeParentConfigWithFeet parent;
        private HumanoidFootIK footIK;
        private Transform player;

        [SetUp]
        public void SetUp()
        {
            GameObject go = CreateGameObject();
            player = go.transform;
            footIK = go.AddComponent<HumanoidFootIK>();
            parent = new RelativeParentConfigWithFeet();
        }

        [Test]
        public void Validate_RelativeParentConfigWithFeet_NoMovingGround()
        {
            GameObject.DestroyImmediate(footIK);
            parent.FollowGround(player);
        }

        [Test]
        public void Validate_RelativeParentConfigWithFeet_WithFeet()
        {
            // Attach to ground with relative position of zero
            Transform player = CreateGameObject().transform;
            Transform ground = CreateGameObject().transform;

            // Set both feet as grounded for test
            footIK.LeftFootTarget.StartStride(Vector3.zero, Quaternion.identity, ground.gameObject, Vector3.forward, footIK.LeftFootTarget.GroundNormal, false);
            footIK.RightFootTarget.StartStride(Vector3.zero, Quaternion.identity, ground.gameObject, Vector3.forward, footIK.RightFootTarget.GroundNormal, false);

            parent.UpdateMovingGround(
                Vector3.zero,
                Quaternion.identity,
                new KCCGroundedState(KCCUtils.Epsilon, true, 0, Vector3.up, Vector3.zero, ground.gameObject),
                Vector3.zero,
                0.0f);

            // Delta should initially be zero
            Assert.AreEqual(Vector3.zero, parent.DeltaPosition(player.position));
            Assert.AreEqual(Quaternion.identity, parent.DeltaRotation(player.rotation));
            parent.FollowGround(player);
            Assert.AreEqual(Vector3.zero, footIK.LeftFootTarget.TargetFootPosition);
            Assert.AreEqual(Vector3.zero, footIK.RightFootTarget.TargetFootPosition);

            // Move the ground forward one unit
            ground.position += Vector3.forward;

            // Relative position should move forward one unit
            Assert.AreEqual(Vector3.forward, parent.DeltaPosition(player.position));
            Assert.AreEqual(Quaternion.identity, parent.DeltaRotation(player.rotation));
            parent.FollowGround(player);

            // Relative position should restore to zero
            Assert.AreEqual(Vector3.zero, parent.DeltaPosition(player.position));
            Assert.AreEqual(Quaternion.identity, parent.DeltaRotation(player.rotation));
            parent.FollowGround(player);
        }
    }
}
