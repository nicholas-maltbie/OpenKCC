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

using System.Collections;
using System.Collections.Generic;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Tests.PlayMode.Character
{
    /// <summary>
    /// Test basic movement scenarios for KCCStateMachine with moving ground.
    /// </summary>
    public class KCCStateMachineMovingGroundTests : KCCStateMachineTestBase
    {
        private GameObject floor;

        public static IEnumerable<Vector3> DirectionsOnFlatPlane()
        {
            return new[] {
                Vector3.forward,
                Vector3.left,
                Vector3.forward + Vector3.left,
                Vector3.forward + Vector3.right,
            };
        }

        public static IEnumerable<Vector3> RotationAxis()
        {
            return new[] {
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, 0, 1),
            };
        }

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            floor = CreateGameObject();
            BoxCollider box = floor.AddComponent<BoxCollider>();
            box.center = Vector3.zero;
            box.size = new Vector3(10, 1, 10);
            box.transform.position = Vector3.zero;
            box.transform.rotation = Quaternion.identity;
            floor.transform.position = new Vector3(0, -0.5f, 0);
        }

        [UnityTest]
        public IEnumerator FollowLinearMovement(
            [ValueSource(nameof(TestDirections))] Vector3 dir,
            [ValueSource(nameof(DirectionsOnFlatPlane))] Vector3 relativePos
        )
        {
            kccStateMachine.transform.position = relativePos;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Wait until player is no longer overlapping with object.
            while (kccStateMachine.config.groundedState.DistanceToGround <= 0)
            {
                yield return new WaitForFixedUpdate();
            }

            // Move the box forward and wait a fixed update, the player should
            // move along with the box
            for (int i = 0; i < 10; i++)
            {
                // Account for drift in player overlap with platform.
                relativePos = floor.transform.InverseTransformPoint(kccStateMachine.transform.position);

                // Translate the current floor
                floor.transform.position += dir * 0.1f;

                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();

                Assert.AreEqual(floor, kccStateMachine.config.groundedState.Floor);
                Assert.IsTrue(kccStateMachine.config.groundedState.StandingOnGroundOrOverlap);
                yield return new WaitForFixedUpdate();

                Vector3 expectedPosition = floor.transform.position + relativePos;
                TestUtils.AssertInBounds(kccStateMachine.transform.position, expectedPosition, range: 0.25f);
            }
        }

        [UnityTest]
        public IEnumerator FollowAngularMovement(
            [ValueSource(nameof(RotationAxis))] Vector3 axis,
            [ValueSource(nameof(DirectionsOnFlatPlane))] Vector3 relativePos
        )
        {
            kccStateMachine.transform.position = relativePos;
            yield return null;
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            // Move the box forward and wait a fixed update, the player should
            // move along with the box
            for (int i = 0; i < 10; i++)
            {
                // Account for drift in player overlap with platform.
                relativePos = floor.transform.InverseTransformPoint(kccStateMachine.transform.position);

                // Rotated by the current floor
                floor.transform.rotation *= Quaternion.Euler(axis.normalized * 5);
                yield return null;
                yield return new WaitForFixedUpdate();
                yield return new WaitForFixedUpdate();
                Assert.AreEqual(floor, kccStateMachine.config.groundedState.Floor);
                Assert.IsTrue(kccStateMachine.config.groundedState.StandingOnGroundOrOverlap);

                // Expected position should be relative position
                Vector3 expectedPos = floor.transform.rotation * relativePos;
                TestUtils.AssertInBounds(kccStateMachine.transform.position, expectedPos, 1.0f);
            }
        }
    }
}
