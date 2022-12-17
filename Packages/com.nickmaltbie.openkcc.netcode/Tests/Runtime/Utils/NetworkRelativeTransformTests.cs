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
using nickmaltbie.openkcc.Tests.netcode.TestCommon;
using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.OpenKCC.netcode.Utils;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.TestTools;

namespace nickmaltbie.openkcc.Tests.netcode.Runtime.Utils
{
    /// <summary>
    /// Simple tests for NetworkRelativeTransform.
    /// </summary>
    [TestFixture]
    public class NetworkRelativeTransformTests : NetcodeInputRuntimeTest<NetworkRelativeTransform>
    {
        protected override int NumberOfClients => 2;
        private GameObject parentPrefab;

        [UnityTest]
        public IEnumerator Verify_NetworkRelativeTransform_Move()
        {
            // Spawn a parent object to attach to later
            yield return ServerSpawnAndWait<NetworkObject>(parentPrefab);
            GameObject parentObj = GetTestableNetworkBehaviour(typeof(NetworkObject), 0, 0).gameObject;
            GameObject sourceObj = GetTestableNetworkBehaviour(0, 0).gameObject;

            // Assign object 0's parent to be parent prefab
            NetworkRelativeTransform obj = GetAttachedNetworkBehaviour(0, 0);
            obj.transform.position = Vector3.up;
            obj.UpdateState(new RelativeParentConfig
            {
                relativePos = Vector3.up,
                previousParent = parentObj.transform,
            });

            // Now when we move the parent object to position Vector3.forward, it should move on all servers
            parentObj.transform.position = Vector3.forward;

            yield return TestUtils.WaitUntil(() => ForAllPlayers(0, tr =>
                (tr.transform.position - sourceObj.transform.position).magnitude <= 0.001f));

            // Now detach the parent
            obj.UpdateState(new RelativeParentConfig
            {
                relativePos = Vector3.zero,
                previousParent = null,
            });
            obj.transform.position = Vector3.up;
            yield return TestUtils.WaitUntil(() => ForAllPlayers(0, tr =>
                (tr.transform.position - sourceObj.transform.position).magnitude <= 0.001f));
        }

        public override void SetupPrefab(GameObject go)
        {
            parentPrefab = CreateNetworkObjectPrefab("ParentPrefab");
            parentPrefab.AddComponent<NetworkTransform>();
        }
    }
}
