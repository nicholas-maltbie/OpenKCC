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

using System;
using nickmaltbie.OpenKCC.Animation;
using UnityEngine;

namespace nickmaltbie.OpenKCC.Character.Config
{
    /// <summary>
    /// Relative parent config that will update a <see cref="nickmaltbie.OpenKCC.Animation.HumanoidFootIK"/>
    /// if one is attached to the avatar.
    /// </summary>
    [Serializable]
    public class RelativeParentConfigWithFeet : RelativeParentConfig
    {
        /// <summary>
        /// Feet controls attached to this character.
        /// </summary>
        private HumanoidFootIK feet;

        /// <inheritdoc/>
        public override void FollowGround(Transform transform)
        {
            feet ??= transform.gameObject.GetComponentInChildren<HumanoidFootIK>();
            Vector3 delta = DeltaPosition(transform.position);
            feet?.UpdateFeetPositions(delta);
            transform.position += delta;
        }
    }
}
