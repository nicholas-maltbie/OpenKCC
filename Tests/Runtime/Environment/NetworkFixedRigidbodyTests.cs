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
using nickmaltbie.OpenKCC.netcode.Environment;
using nickmaltbie.TestUtilsUnity.Tests.TestCommon;
using NUnit.Framework;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.TestTools;

namespace nickmaltbie.openkcc.Tests.netcode.Runtime.Envionment
{
    /// <summary>
    /// Simple tests for NetworkFixedRigidbodyTests.
    /// </summary>
    [TestFixture]
    public class NetworkFixedRigidbodySetTests : NetcodeInputRuntimeTest<NetworkFixedRigidbodySet>
    {
        protected override int NumberOfClients => 2;
        protected override int SpawnCount => 1;

        public override void SetupPrefab(GameObject go)
        {
            go.AddComponent<NetworkTransform>();
        }

        [UnityTest]
        public IEnumerator Verify_NetworkMovingPlatform_Move()
        {
            NetworkFixedRigidbodySet fixedRb = GetAttachedNetworkBehaviour(0, 0);
            Vector3 linearVelocity = Vector3.forward;
            Vector3 angularVelocity = Vector3.up * 30;
            fixedRb.linearVelocity = linearVelocity;
            fixedRb.angularVelocity = angularVelocity;

            Vector3 startPos = fixedRb.transform.position;
            Vector3 startAngle = fixedRb.transform.rotation.eulerAngles;

            yield return new WaitForSeconds(1.0f);
            yield return TestUtils.WaitUntil(() => ForServerObject(
                    fixedRb =>
                    {
                        Vector3 pos = fixedRb.transform.position;
                        Vector3 rotation = fixedRb.transform.rotation.eulerAngles;

                        float linearDelta = Vector3.Dot(Vector3.Project(pos - startPos, linearVelocity), linearVelocity);
                        float angularDelta = Vector3.Dot(Vector3.Project(rotation - startAngle, angularVelocity), angularVelocity);

                        UnityEngine.Debug.Log($"linearDelta:{linearDelta} angularDelta:{angularDelta}");

                        return linearDelta >= linearVelocity.magnitude && angularDelta >= angularVelocity.magnitude;
                    }));
        }
    }
}
