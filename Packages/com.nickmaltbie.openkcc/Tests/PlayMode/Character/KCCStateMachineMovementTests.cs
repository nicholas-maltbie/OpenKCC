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

using System.Collections;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Tests.PlayMode.Character
{
    public class KCCStateMachineMovementTests : KCCStateMachineTestBase
    {
        private GameObject floor;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            floor = new GameObject();
            BoxCollider box = floor.AddComponent<BoxCollider>();
            box.center = new Vector3(0, -0.5f, 0);
            box.size = new Vector3(10, 1, 10);
            box.transform.position = Vector3.zero;
            box.transform.rotation = Quaternion.identity;
            kccStateMachine.transform.position = Vector3.zero;
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            Object.DestroyImmediate(floor);
        }

        [UnityTest]
        public IEnumerator Validate_KCCStateMachine_NoMovement()
        {
            Vector3 initialPosition = kccStateMachine.transform.position;

            // Wait for a second, ensure no player movement ocurrs
            for (int i = 0; i < 60; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            Assert.AreEqual(initialPosition, kccStateMachine.transform.position);
        }

        [UnityTest]
        public IEnumerator Validate_KCCStateMachine_BasicMovement()
        {
            Vector3 initialPosition = kccStateMachine.transform.position;
            kccStateMachine.walkingSpeed = 1.0f;
            kccStateMachine.Update();
            Set(moveStick, Vector2.up);

            // Wait for a second, ensure no player movement ocurrs
            for (int i = 0; i < 60; i++)
            {
                yield return new WaitForFixedUpdate();
            }

            TestUtils.AssertInBounds(
                initialPosition + Vector3.forward * kccStateMachine.walkingSpeed,
                kccStateMachine.transform.position, 0.25f);
        }
    }
}
