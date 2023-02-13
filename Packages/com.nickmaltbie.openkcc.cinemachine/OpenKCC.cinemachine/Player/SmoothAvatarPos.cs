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

using nickmaltbie.OpenKCC.Character;
using nickmaltbie.TestUtilsUnity;
using UnityEngine;

namespace nickmaltbie.OpenKCC.cinemachine.Player
{
    public class SmoothAvatarPos : MonoBehaviour, IOnPlayerTeleport
    {
        /// <summary>
        /// Unity service for managing testing.
        /// </summary>
        public IUnityService unityService = UnityService.Instance;

        /// <summary>
        /// Avatar base to rotate.
        /// </summary>
        public GameObject avatarBase;

        /// <summary>
        /// Factor to smooth player position by.
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float smooth = 0.05f;

        /// <summary>
        /// Saved offset of avatar from current position.
        /// </summary>
        private Vector3 offset;

        /// <summary>
        /// Previous position of player.
        /// </summary>
        private Vector3 previousPos;

        public void Awake()
        {
            previousPos = transform.position;
            offset = avatarBase.transform.position - transform.position;
        }

        public void FixedUpdate()
        {
            previousPos = Vector3.Lerp(transform.position, previousPos, smooth);
            avatarBase.transform.position = previousPos + offset;
        }

        public void OnPlayerTeleport(Vector3 destPos, Quaternion destRot)
        {
            previousPos = destPos;
        }
    }
}
