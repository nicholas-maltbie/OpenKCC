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
using nickmaltbie.OpenKCC.Character;
using nickmaltbie.OpenKCC.Tests.TestCommon;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.OpenKCC.Utils.ColliderCast;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using UnityEngine;
using UnityEngine.TestTools;

namespace nickmaltbie.OpenKCC.Tests.PlayMode.Character
{
    public class KCCMovementEngineTests : TestBase
    {
        [UnityTest]
        public IEnumerator Verify_KCCMovementEngine_NoSlide()
        {
            GameObject go = CreateGameObject();
            CapsuleCollider capsuleCollider = go.AddComponent<CapsuleCollider>();
            capsuleCollider.center = new Vector3(0, 1, 0);
            capsuleCollider.height = 2.0f;
            capsuleCollider.radius = 0.5f;
            go.AddComponent<CapsuleColliderCast>();
            Rigidbody rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            KCCMovementEngine moveEngine = go.AddComponent<KCCMovementEngine>();

            GameObject ground = CreateGameObject();
            BoxCollider box = ground.AddComponent<BoxCollider>();
            box.size = new Vector3(1000, 1, 1000);

            // Setup positions of ground and player.
            ground.transform.position = new Vector3(0.2f, -Mathf.Sqrt(2));
            ground.transform.rotation = Quaternion.Euler(0, 0, 45);
            Vector3 initialPosition = Vector3.zero;
            moveEngine.transform.position = initialPosition;

            yield return new WaitForFixedUpdate();

            // Ensure the player does not slide when they are upadted a lot of times.
            for (int i = 0; i < 100 * 60; i++)
            {
                // Assume plaer inputs no movement.
                moveEngine.MovePlayer(Vector3.zero);
            }

            yield return new WaitForFixedUpdate();

            // Assert that player has not moved.
            TestUtils.AssertInBounds(moveEngine.transform.position, initialPosition);
        }
    }
}
