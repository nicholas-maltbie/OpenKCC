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

using UnityEngine;

namespace nickmaltbie.OpenKCC.Environment.MovingGround
{
    /// <summary>
    /// Moving ground object that a player can move along with
    /// </summary>
    public interface IMovingGround
    {
        /// <summary>
        /// Get the velocity (in units per second) that the object is moving
        /// at a given point on the surface of the object (in world space).
        /// </summary>
        /// <param name="point">Point on the surface of the object (in world space).</param>
        /// <returns>Velocity that the object is moving at the point.</returns>
        Vector3 GetVelocityAtPoint(Vector3 point);

        /// <summary>
        /// Get the weight of movement for a given player's velocity at a given point.
        /// </summary>
        /// <param name="point">Point where player is standing on the object.</param>
        /// <param name="playerVelocity">Velocity of the player.</param>
        /// <returns>Weight of player's attachment to the object given these
        /// parameters. Will be between 0 (not attached at all) and 1 (fully attached).</returns>
        float GetMovementWeight(Vector3 point, Vector3 playerVelocity);

        /// <summary>
        /// Get the weight of movement of transfering momentum when a player leaves this object
        /// </summary>
        /// <param name="point">Point where player is standing on the object.</param>
        /// <param name="playerVelocity">Velocity of the player.</param>
        /// <returns>How much relative velocity teh player should retain when departing from the surface of this object
        /// via jump or fall.</returns>
        float GetTransferMomentumWeight(Vector3 point, Vector3 playerVelocity);

        /// <summary>
        /// Should momentum be transferred to players when they
        /// leave this object.
        /// </summary>
        bool AvoidTransferMomentum();

        /// <summary>
        /// When following this object, should the player attach themselves
        /// to the object to follow it properly? This is important
        /// for rapidly moving objects. Additionally, if the object does
        /// not move but wants to push the player (such as a conveyer belt),
        /// then players should definitely not attach to the object.
        /// </summary>
        bool ShouldAttach();
    }
}
