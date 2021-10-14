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
        /// <param name="deltaTime">delta time for computing velocity.</param>
        /// <returns>Velocity that the object is moving at the point.</returns>
        Vector3 GetVelocityAtPoint(Vector3 point, float deltaTime);

        /// <summary>
        /// Get displacement of the moving object at a given point on the
        /// surface of the object (in world space) for the current fixed update.
        /// </summary>
        /// <param name="point">Point on the surface of the object (in world space).</param>
        /// <returns>Displacement on the surface of this object from that point
        /// for the current fixed update</returns>
        Vector3 GetDisplacementAtPoint(Vector3 point, float deltaTime);

        /// <summary>
        /// Get the weight of movement for a given player's velocity at a given point.
        /// </summary>
        /// <param name="point">Point where player is standing on the object.</param>
        /// <param name="playerVelocity">Velocity of the player.</param>
        /// <returns>Weight of player's attachment to the object given these
        /// parameters. Will be between 0 (not attached at all) and 1 (fully attached).</returns>
        float GetMovementWeight(Vector3 point, Vector3 playerVelocity, float deltaTime);

        /// <summary>
        /// Get the weight of movement of transfering momentum when a player leaves this object
        /// </summary>
        /// <param name="point">Point where player is standing on the object.</param>
        /// <param name="playerVelocity">Velocity of the player.</param>
        /// <returns>How much relative velocity teh player should retain when departing from the surface of this object
        /// via jump or fall.</returns>
        float GetTransferMomentumWeight(Vector3 point, Vector3 playerVelocity, float deltaTime);

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