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
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.Animations;

namespace nickmaltbie.OpenKCC.netcode.Utils
{
    [RequireComponent(typeof(ParentConstraint))]
    public class NetworkParentConstarint : NetworkBehaviour
    {
        public IUnityService unityService = UnityService.Instance;
        public float smoothRate = 4;

        protected ParentConstraint parentConstraint;
        protected Vector3 worldPosition;
        protected Vector3 relativePos;
        protected ulong previousParent;
        protected bool previousActive;

        protected NetworkVariable<NetworkConstraintSource> networkConstarint =
            new NetworkVariable<NetworkConstraintSource>(
                writePerm: NetworkVariableWritePermission.Owner,
                readPerm: NetworkVariableReadPermission.Everyone);

        public NetworkObject Floor { get; set; }

        /// <summary>
        /// Floor Constraint used for parent constraint to move with ground.
        /// </summary>
        private ConstraintSource floorConstraint;

        public void Awake()
        {
            parentConstraint = GetComponent<ParentConstraint>();
        }

        public void Update()
        {
            // Update the network sync if the floor has a network transform
            if (IsOwner)
            {
                bool enableNetworkParent = Floor != null && parentConstraint.constraintActive;

                if (enableNetworkParent)
                {
                    // Update the relative parent
                    Vector3 relativePos = transform.position - Floor.transform.position;
                    networkConstarint.Value = new NetworkConstraintSource
                    {
                        active = true,
                        parentTransform = Floor,
                        relativePosition = relativePos,
                        translationAtRest = parentConstraint.translationAtRest,
                        rotationAtRest = parentConstraint.rotationAtRest,
                    };
                }
                else
                {
                    // Check if it is currently active, if so, set to not active
                    networkConstarint.Value = new NetworkConstraintSource
                    {
                        active = false,
                        parentTransform = default,
                        relativePosition = Vector3.zero,
                        translationAtRest = transform.position,
                        rotationAtRest = transform.rotation.eulerAngles,
                    };
                }
            }
            else
            {
                NetworkConstraintSource source = networkConstarint.Value;

                // If not owner, update the player based on the current
                // relative parent configuration
                NetworkObject targetObject = null;
                bool validParent = source.active && source.parentTransform.TryGet(out targetObject);

                if (parentConstraint.sourceCount > 0)
                {
                    parentConstraint.RemoveSource(0);
                }

                if (!previousActive && validParent)
                {
                    // Avoid jitter by setting initial relative position
                    // to current offset from parent
                    relativePos = targetObject.transform.InverseTransformDirection(transform.position);
                }
                else if (previousActive && !validParent)
                {
                    // Avoid jitter by setting world pos to current position
                    worldPosition = transform.position;
                }

                if (validParent)
                {
                    // Smooth relative position to avoid jitter
                    relativePos = Vector3.Lerp(relativePos, source.relativePosition, smoothRate * unityService.deltaTime);

                    parentConstraint.translationAtRest = source.translationAtRest;
                    parentConstraint.rotationAtRest = source.rotationAtRest;

                    Transform floorTransform = targetObject.transform;
                    floorConstraint.sourceTransform = floorTransform;
                    floorConstraint.weight = 1.0f;
                    parentConstraint.AddSource(floorConstraint);
                    parentConstraint.SetTranslationOffset(0, floorTransform.InverseTransformDirection(source.relativePosition));
                    parentConstraint.constraintActive = true;
                }
                else
                {
                    // Smooth relative position to avoid jitter
                    worldPosition = Vector3.Lerp(worldPosition, source.translationAtRest, smoothRate * unityService.deltaTime);
                    transform.position = worldPosition;
                }

                parentConstraint.constraintActive = validParent;
                previousParent = source.parentTransform.NetworkObjectId;
                previousActive = validParent;
            }
        }
    }
}
