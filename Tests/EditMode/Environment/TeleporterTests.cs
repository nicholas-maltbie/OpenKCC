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

using nickmaltbie.OpenKCC.Environment;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Tests.EditMode.Environment
{
    /// <summary>
    /// Basic tests for Teleporter in edit mode.
    /// </summary>
    [TestFixture]
    public class TeleporterTests : TestBase
    {
        [Test]
        public void Verify_TeleporterActions([Values] bool hasCharacterController)
        {
            Teleporter tele = CreateGameObject().AddComponent<Teleporter>();
            Collider co = CreateGameObject().AddComponent<SphereCollider>();

            if (hasCharacterController)
            {
                co.gameObject.AddComponent<CharacterController>();
            }

            tele.transform.position = Vector3.forward * 10;
            tele.transform.rotation = Quaternion.Euler(Vector3.up * 30);
            tele.teleportLocation = tele.transform;

            TestUtils.AssertInBounds(co.transform.position, Vector3.zero);

            tele.OnTriggerEnter(co);

            TestUtils.AssertInBounds(co.transform.position, Vector3.forward * 10);
            TestUtils.AssertInBounds(co.transform.eulerAngles, Vector3.up * 30);
        }
    }
}
