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

using nickmaltbie.OpenKCC.Character.Config;
using nickmaltbie.TestUtilsUnity;
using Unity.Netcode;
using UnityEngine;

namespace nickmaltbie.OpenKCC.netcode.Utils
{
    [DefaultExecutionOrder(10000)]
    public class NetworkRelativeTransform : NetworkBehaviour
    {
        public IUnityService unityService = UnityService.Instance;
        public float smoothRate = 10;

        protected Vector3 worldPosition;
        protected Vector3 relativePos;
        protected ulong previousParent;
        protected bool previousActive;

        protected NetworkVariable<NetworkRelativeParent> networkConstraint =
            new NetworkVariable<NetworkRelativeParent>(
                writePerm: NetworkVariableWritePermission.Owner,
                readPerm: NetworkVariableReadPermission.Everyone);

        public NetworkObject Floor { get; set; }

        public void UpdateState(RelativeParentConfig config)
        {
            Floor = config.previousParent?.GetComponent<NetworkObject>();
        }

        public void Awake()
        {
            worldPosition = transform.position;
            previousActive = false;
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
                bool enableNetworkParent = Floor != null;
                if (enableNetworkParent)
                {
                    // Update the relative parent
                    Vector3 relativePos = Quaternion.Inverse(Floor.transform.rotation) * (transform.position - Floor.transform.position);
                    networkConstraint.Value = new NetworkRelativeParent
                    {
                        active = true,
                        parentTransform = Floor,
                        relativePosition = relativePos,
                        worldPos = transform.position,
                        rotation = transform.rotation,
                    };
                }
                else
                {
                    // Check if it is currently active, if so, set to not active
                    networkConstraint.Value = new NetworkRelativeParent
                    {
                        active = false,
                        parentTransform = default,
                        relativePosition = default,
                        worldPos = transform.position,
                        rotation = transform.rotation,
                    };
                }
            }
            else
            {
                NetworkRelativeParent source = networkConstraint.Value;

                // If not owner, update the player based on the current
                // relative parent configuration
                NetworkObject targetObject = null;
                bool validParent = source.active && source.parentTransform.TryGet(out targetObject);

                if (validParent && (!previousActive || previousParent != source.parentTransform.NetworkObjectId))
                {
                    // Avoid jitter by setting initial relative position
                    // to current offset from parent
                    relativePos = transform.position - targetObject.transform.position;
                }
                else if (previousActive && !validParent)
                {
                    // Avoid jitter by setting world pos to current position
                    worldPosition = transform.position;
                }
                else if (validParent)
                {
                    // Smooth relative position to avoid jitter
                    relativePos = Vector3.Lerp(relativePos, source.relativePosition, smoothRate * unityService.deltaTime);

                    Transform floorTransform = targetObject.transform;
                    transform.position = floorTransform.rotation * relativePos + floorTransform.position;
                }
                else
                {
                    // Smooth relative position to avoid jitter
                    worldPosition = Vector3.Lerp(worldPosition, source.worldPos, smoothRate * unityService.deltaTime);
                    transform.position = worldPosition;
                }

                previousParent = source.parentTransform.NetworkObjectId;
                previousActive = validParent;
            }
        }
    }
}
