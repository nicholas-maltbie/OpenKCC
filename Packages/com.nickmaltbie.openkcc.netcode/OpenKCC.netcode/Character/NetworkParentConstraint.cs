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

using Unity.Netcode;
using UnityEngine;

namespace nickmaltbie.OpenKCC.netcode.Character
{
    /// <summary>
    /// Script to attach player to parent constraint.
    /// </summary>
    public struct NetworkParentConstraint : INetworkSerializable
    {
        public bool active;
        public NetworkObjectReference parentTransform;
        public Vector3 relativePosition;
        public Vector3 translationAtRest;
        public Vector3 rotationAtRest;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            bool previousActive = active;
            ulong previousRef = parentTransform.NetworkObjectId;
            Vector3 previousRelativePos = relativePosition;
            Vector3 previousTranslationAtRest = translationAtRest;
            serializer.SerializeValue(ref active);
            serializer.SerializeValue(ref parentTransform);
            serializer.SerializeValue(ref relativePosition);
            serializer.SerializeValue(ref translationAtRest);
            serializer.SerializeValue(ref rotationAtRest);

            if (previousActive && active && previousRef == parentTransform.NetworkObjectId)
            {
                relativePosition = Vector3.Lerp(previousRelativePos, relativePosition, 0.1f * Time.deltaTime);
                previousTranslationAtRest = Vector3.Lerp(previousTranslationAtRest, relativePosition, 0.1f * Time.deltaTime);
            }
        }
    }
}
