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

using nickmaltbie.TestUtilsUnity;
using Unity.Netcode;
using UnityEngine;

namespace nickmaltbie.OpenKCC.netcode.Utils
{
    [DefaultExecutionOrder(100)]
    public class SmoothedNetworkTransform : NetworkBehaviour
    {
        public IUnityService unityService = UnityService.Instance;
        public float smoothRate = 10;

        protected Vector3 worldPosition;
        protected Quaternion worldRotation;

        protected NetworkVariable<Vector3> netPosition =
            new NetworkVariable<Vector3>(
                writePerm: NetworkVariableWritePermission.Owner,
                readPerm: NetworkVariableReadPermission.Everyone);

        protected NetworkVariable<Quaternion> netRotation =
            new NetworkVariable<Quaternion>(
                writePerm: NetworkVariableWritePermission.Owner,
                readPerm: NetworkVariableReadPermission.Everyone);

        public void Awake()
        {
            worldPosition = transform.position;
            worldRotation = transform.rotation;
        }

        public void Update()
        {
            if (!IsSpawned)
            {
                return;
            }

            // Update the network sync if the floor has a network transform
            if (IsOwner)
            {
                netPosition.Value = transform.position;
                netRotation.Value = transform.rotation;
            }
            else
            {
                // Smooth position to avoid jitter
                worldRotation = Quaternion.Lerp(worldRotation, netRotation.Value, smoothRate * unityService.deltaTime);
                worldPosition = Vector3.Lerp(worldPosition, netPosition.Value, smoothRate * unityService.deltaTime);
                transform.position = worldPosition;
                transform.rotation = worldRotation;
            }
        }
    }
}
