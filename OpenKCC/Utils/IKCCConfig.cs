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

using nickmaltbie.OpenKCC.Character;
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
        /// Push decay factor for player movement.
        /// </summary>
        float PushDecay { get; }

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
        /// Character push for checking if and how the character should push objects.
        /// </summary>
        ICharacterPush Push { get; }

        /// <summary>
        /// Max velocity at which the player can be launched
        /// when gaining momentum from a floor object without
        /// an IMovingGround attached to it.
        /// </summary>
        float MaxDefaultLaunchVelocity { get; }
    }

    /// <summary>
    /// fixed structure for configuration for controlling KCC Movement.
    /// </summary>
    public struct KCCConfig : IKCCConfig
    {
        /// <summary>
        /// Maximum bounces when moving the player.
        /// </summary>
        public int maxBounces;

        /// <summary>
        /// Push decay factor for player movement.
        /// </summary>
        public float pushDecay;

        /// <summary>
        /// Vertical snap up distance the player can snap up.
        /// </summary>
        public float verticalSnapUp;

        /// <summary>
        /// Minimum depth required for a stair when moving onto a step.
        /// </summary>
        public float stepUpDepth;

        /// <summary>
        /// Angle power for decaying momentum when bouncing off a surface.
        /// </summary>
        public float anglePower;

        /// <summary>
        /// Can the player snap up steps during this movement.
        /// </summary>
        public bool canSnapUp;

        /// <summary>
        /// Position to start player movement from.
        /// </summary>
        public Vector3 up;

        /// <summary>
        /// Collider cast for checking what the player is colliding with.
        /// </summary>
        public IColliderCast colliderCast;

        /// <summary>
        /// Character push for checking if and how the character should push objects.
        /// </summary>
        public ICharacterPush push;

        /// <inheritdoc/>
        public int MaxBounces { get => maxBounces; set => maxBounces = value; }

        /// <inheritdoc/>
        public float PushDecay { get => pushDecay; set => pushDecay = value; }

        /// <inheritdoc/>
        public float VerticalSnapUp { get => verticalSnapUp; set => verticalSnapUp = value; }

        /// <inheritdoc/>
        public float StepUpDepth { get => stepUpDepth; set => stepUpDepth = value; }

        /// <inheritdoc/>
        public float AnglePower { get => anglePower; set => anglePower = value; }

        /// <inheritdoc/>
        public bool CanSnapUp { get => canSnapUp; set => canSnapUp = value; }

        /// <inheritdoc/>
        public Vector3 Up { get => up; set => up = value; }

        /// <inheritdoc/>
        public IColliderCast ColliderCast { get => colliderCast; set => colliderCast = value; }

        /// <inheritdoc/>
        public ICharacterPush Push { get => push; set => push = value; }

        /// <inheritdoc/>
        public float MaxDefaultLaunchVelocity { get; set; }
    }
}
