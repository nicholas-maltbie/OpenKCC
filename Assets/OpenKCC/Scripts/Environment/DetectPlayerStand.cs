using MLAPI;
using MLAPI.Messaging;

namespace nickmaltbie.OpenKCC.Environment
{
    /// <summary>
    /// Detect when players stand on this object
    /// </summary>
    public abstract class DetectPlayerStand : NetworkBehaviour
    {
        /// <summary>
        /// When a player steps onto this tile
        /// </summary>
        public virtual void StepOn() { }

        /// <summary>
        /// When a player steps off of this tile
        /// </summary>
        public virtual void StepOff() { }
    }
}