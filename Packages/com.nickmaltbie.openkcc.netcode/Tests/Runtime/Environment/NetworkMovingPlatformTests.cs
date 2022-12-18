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
using System.Linq;
using nickmaltbie.openkcc.Tests.netcode.TestCommon;
using nickmaltbie.OpenKCC.netcode.Environment;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.TestTools;

namespace nickmaltbie.openkcc.Tests.netcode.Runtime.Envionment
{
    /// <summary>
    /// Simple tests for NetworkMovingPlatformTests.
    /// </summary>
    [TestFixture]
    public class NetworkMovingPlatformTests : NetcodeInputRuntimeTest<NetworkMovingPlatform>
    {
        protected override int NumberOfClients => 2;
        protected override int SpawnCount => 1;
        protected List<GameObject> positions;

        [UnitySetUp]
        public override IEnumerator UnitySetUp()
        {
            positions = new List<GameObject>();
            positions.Add(new GameObject());
            positions.Add(new GameObject());

            positions[0].name = "pos0";
            positions[0].transform.position = Vector3.zero;

            positions[1].name = "pos1";
            positions[1].transform.position = Vector3.left * 100;

            yield return base.UnitySetUp();
        }

        [UnityTearDown]
        public override IEnumerator UnityTearDown()
        {
            yield return base.UnityTearDown();
            while (positions.Count > 0)
            {
                GameObject.Destroy(positions[0]);
                positions.RemoveAt(0);
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator Verify_NetworkMovingPlatform_Move()
        {
            // When the platform moves on the server, assert that it moves on the clients as well
            // Assert that after a second, the platform has moved towards the first target
            Vector3 start = Vector3.zero;
            Vector3 dir = (positions[1].transform.position - positions[0].transform.position).normalized;

            yield return new WaitForSeconds(1.0f);
            yield return TestUtils.WaitUntil(() => ForServerObject(
                    movingPlatform =>
                    {
                        Vector3 pos = movingPlatform.transform.position;
                        float delta = Vector3.Dot(Vector3.Project(pos - start, dir), dir);
                        return delta >= movingPlatform.linearSpeed;
                    }));
        }

        [UnityTest]
        public IEnumerator Verify_InvalidTarget()
        {
            GetAttachedNetworkBehaviour(0, 0).targetsList = null;
            GetAttachedNetworkBehaviour(0, 0).Update();
            yield return null;
        }

        public override void SetupPrefab(GameObject go)
        {
            NetworkMovingPlatform movingPlatofrm = go.GetComponent<NetworkMovingPlatform>();
            go.AddComponent<NetworkTransform>();
            movingPlatofrm.targetsList = positions.Select(v => v.transform).ToList();
        }
    }
}
