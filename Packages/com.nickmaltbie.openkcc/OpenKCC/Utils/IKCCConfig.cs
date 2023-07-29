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

using UnityEngine;

namespace nickmaltbie.OpenKCC.Utils
{
    /// <summary>
    /// Configuration for controlling KCC Movement.
    /// </summary>
    public interface IKCCConfig
    {
        /// <summary>
        /// Maximum bounces when moving the player.
        /// </summary>
        int MaxBounces { get; }

        /// <summary>
        /// Vertical snap up distance the player can snap up.
        /// </summary>
        float VerticalSnapUp { get; }

        /// <summary>
        /// Minimum depth required for a stair when moving onto a step.
        /// </summary>
        float StepUpDepth { get; }

        /// <summary>
        /// Angle power for decaying momentum when bouncing off a surface.
        /// </summary>
        float AnglePower { get; }

        /// <summary>
        /// Can the player snap up steps during this movement.
        /// </summary>
        bool CanSnapUp { get; }

        /// <summary>
        /// Position to start player movement from.
        /// </summary>
        Vector3 Up { get; }

        /// <summary>
        /// Collider cast for checking what the player is colliding with.
        /// </summary>
        IColliderCast ColliderCast { get; }

        /// <summary>
        /// Get the layer mask for computing player collisions.
        /// </summary>
        LayerMask LayerMask { get; }

        /// <summary>
        /// The character's collision skin width.
        /// <br/>
        /// This is dependant on the scale of the world, but should be a small, positive non zero value.
        /// <br/>
        /// Reference: CharacterController.skinWidth, Unity Documentation - https://docs.unity3d.com/ScriptReference/CharacterController-skinWidth.html
        /// Reference: Character Controllers: Character Volume, Nvidia PhysX SDK - https://docs.nvidia.com/gameworks/content/gameworkslibrary/physx/guide/Manual/CharacterControllers.html#character-volume
        /// </summary>
        float SkinWidth { get; }
    }

    /// <summary>
    /// fixed configuration for controlling KCC Movement.
    /// </summary>
    public class KCCConfig : IKCCConfig
    {
        /// <inheritdoc/>
        public int MaxBounces { get; set; } = 5;

        /// <inheritdoc/>
        public float VerticalSnapUp { get; set; } = 0.3f;

        /// <inheritdoc/>
        public float StepUpDepth { get; set; } = 0.3f;

        /// <inheritdoc/>
        public float AnglePower { get; set; } = 0.8f;

        /// <inheritdoc/>
        public bool CanSnapUp { get; set; } = true;

        /// <inheritdoc/>
        public Vector3 Up { get; set; } = Vector3.up;

        /// <inheritdoc/>
        public IColliderCast ColliderCast { get; set; }

        /// <inheritdoc/>
        public LayerMask LayerMask { get; set; } = ~0;

        /// <inheritdoc/>
        public float SkinWidth { get; set; } = 0.01f;
    }
}
