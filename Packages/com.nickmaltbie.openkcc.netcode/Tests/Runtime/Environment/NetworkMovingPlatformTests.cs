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
using NUnit.Framework;
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

        public override void SetupPrefab(GameObject go)
        {
            NetworkMovingPlatform platform = go.GetComponent<NetworkMovingPlatform>();
            positions = new List<GameObject>();
            positions.Add(new GameObject());
            positions.Add(new GameObject());

            positions[0].transform.position = Vector3.zero;
            positions[1].transform.position = Vector3.left * 10;

            platform.targetsList = positions.Select(v => v.transform).ToList();
        }

        [UnityTearDown]
        public override IEnumerator UnityTearDown()
        {
            yield return UnityTearDown();
            
            while (positions.Count > 0)
            {
                GameObject.Destroy(positions[0]);
                positions.RemoveAt(0);
            }
        }

        [UnityTest]
        public IEnumerator Verify_NetworkMovingPlatform_Move()
        {
            yield return null;
        }
    }
}
