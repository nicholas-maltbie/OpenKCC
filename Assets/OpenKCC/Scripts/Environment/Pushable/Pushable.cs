using UnityEngine;
using MLAPI;
using MLAPI.Messaging;

namespace nickmaltbie.OpenKCC.Environment.Pushable
{
    /// <summary>
    /// Pushable object that can be shoved with a given force
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class Pushable : NetworkBehaviour, IPushable
    {
        /// <summary>
        /// Object rigidbody for pushing by players.
        /// </summary>
        private Rigidbody objRigidbody;

        /// <summary>
        /// Network object associated with this pushable object.
        /// </summary>
        private NetworkObject networkObject;

        /// <summary>
        /// Cooldown in seconds between how soon the owner can change.
        /// </summary>
        private float ownerChangeCooldown = 0.05f;

        /// <summary>
        /// Cooldown to revert ownership back to server.
        /// </summary>
        private float revertOwnerCooldown = 1.0f;

        /// <summary>
        /// Last time the owner was changed.
        /// </summary>
        private float lastOwnerChangeTime = Mathf.NegativeInfinity;

        public void Awake()
        {
            objRigidbody = GetComponent<Rigidbody>();
            networkObject = GetComponent<NetworkObject>();

            networkObject.DontDestroyWithOwner = true;
        }

        /// <inheritdoc/>
        [ServerRpc(RequireOwnership = false)]
        public void PushObjectServerRpc(Vector3 force, Vector3 point, int forceMode, ulong sourceId)
        {
            if (Time.time - lastOwnerChangeTime >= ownerChangeCooldown)
            {
                // networkObject.ChangeOwnership(sourceId);
                lastOwnerChangeTime = Time.time;
            }
            if (sourceId != NetworkManager.ServerClientId)
            {
                objRigidbody.AddForce(force, (ForceMode)forceMode);
            }
        }

        public void Update()
        {
            if (IsServer && !networkObject.IsOwnedByServer &&
                Time.time - lastOwnerChangeTime >= revertOwnerCooldown)
            {
                networkObject.ChangeOwnership(NetworkManager.ServerClientId);
            }
        }
    }
}