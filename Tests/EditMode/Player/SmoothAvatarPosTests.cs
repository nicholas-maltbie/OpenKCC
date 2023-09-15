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

using nickmaltbie.OpenKCC.cinemachine.Player;
using nickmaltbie.OpenKCC.Utils;
using nickmaltbie.TestUtilsUnity;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.cinemachine.EditMode.Player
{
    [TestFixture]
    public class SmoothAvatarPosTests : TestBase
    {
        [Test]
        public void Verify_SmoothAvatarPos([NUnit.Framework.Range(0, 0.8f, 0.1f)] float smoothFactor)
        {
            GameObject avatar = CreateGameObject();
            GameObject parent = CreateGameObject();

            avatar.transform.SetParent(parent.transform);

            SmoothAvatarPos smooth = parent.AddComponent<SmoothAvatarPos>();
            smooth.avatarBase = avatar;
            smooth.smooth = smoothFactor;

            var unityServiceMock = new MockUnityService();
            unityServiceMock.deltaTime = 1.0f;
            smooth.unityService = unityServiceMock;

            smooth.Awake();
            smooth.FixedUpdate();

            // Assert that as the player moves, it is smoothed between
            // the previous position and the destination position
            Vector3 previous = parent.transform.position;
            parent.transform.position += Vector3.forward;
            Vector3 destination = parent.transform.position;

            smooth.FixedUpdate();
            int maxUpdate = 1000;
            int update = 0;
            while (Vector3.Distance(avatar.transform.position, parent.transform.position) > KCCUtils.Epsilon)
            {
                smooth.FixedUpdate();

                // Assert that the player is between the initial and final pos
                Vector3 deltaStart = avatar.transform.position - previous;
                Vector3 deltaEnd = destination - avatar.transform.position;
                TestUtils.AssertInBounds(deltaStart + deltaEnd, Vector3.forward, 0.01f);

                update++;
                Assert.IsTrue(update <= maxUpdate);
            }

            // Verify teleporting
            smooth.OnPlayerTeleport(Vector3.forward, Quaternion.identity);
        }
    }
}
