﻿using FishNet.Connection;
using FishNet.Managing.Logging;
using UnityEngine;

namespace FishNet.Object
{
    public sealed partial class NetworkObject : MonoBehaviour
    {
        #region Public.
        /// <summary>
        /// True if the client is active and authenticated.
        /// </summary>
        public bool IsClient => NetworkManager.IsClient;
        /// <summary>
        /// True if only the client is active, and authenticated.
        /// </summary>
        public bool IsClientOnly => NetworkManager.IsClientOnly;
        /// <summary>
        /// True if server is active.
        /// </summary>
        public bool IsServer => NetworkManager.IsServer;
        /// <summary>
        /// True if only the server is active.
        /// </summary>
        public bool IsServerOnly => NetworkManager.IsServerOnly;
        /// <summary>
        /// True if client and server are active.
        /// </summary>
        public bool IsHost => NetworkManager.IsHost;
        /// <summary>
        /// True if client nor server are active.
        /// </summary>
        public bool IsOffline => NetworkManager.IsOffline;
        /// <summary>
        /// True if the local client is the owner of this object.
        /// </summary>
        public bool IsOwner => (NetworkManager == null || !OwnerIsValid || !IsClient) ? false : (NetworkManager.ClientManager.Connection == Owner);
        /// <summary>
        /// Owner of this object.
        /// </summary>
        public NetworkConnection Owner { get; private set; }
        /// <summary>
        /// True if there is an owner.
        /// </summary>
        public bool OwnerIsValid => (Owner == null) ? false : Owner.IsValid;
        /// <summary>
        /// True if there is an owner and their connect is active. This will return false if there is no owner, or if the connection is disconnecting.
        /// </summary>
        public bool OwnerIsActive => (Owner == null) ? false : Owner.IsActive;
        /// <summary>
        /// ClientId for this NetworkObject owner.
        /// </summary>
        public int OwnerId => (!OwnerIsValid) ? -1 : Owner.ClientId;
        /// <summary>
        /// True if the object is initialized for the network.
        /// </summary>
        public bool IsSpawned => (!Deinitializing && ObjectId >= 0);
        /// <summary>
        /// The local connection of the client calling this method.
        /// </summary>
        public NetworkConnection LocalConnection => (NetworkManager == null) ? new NetworkConnection() : NetworkManager.ClientManager.Connection;
        #endregion

        /// <summary>
        /// Despawns this NetworkObject. Only call from the server.
        /// </summary>
        /// <param name="destroyInstantiated">True to also destroy the object if it was instantiated. False will only disable the object.</param>
        public void Despawn()
        {
            NetworkObject nob = this;
            NetworkManager.ServerManager.Despawn(nob);
        }
        /// <summary>
        /// Spawns an object over the network. Only call from the server.
        /// </summary>
        public void Spawn(GameObject go, NetworkConnection ownerConnection = null)
        {
            if (!CanSpawnOrDespawn(true))
                return;
            NetworkManager.ServerManager.Spawn(go, ownerConnection);
        }

        /// <summary>
        /// Returns if Spawn or Despawn can be called.
        /// </summary>
        /// <param name="warn">True to warn if not able to execute spawn or despawn.</param>
        /// <returns></returns>
        internal bool CanSpawnOrDespawn(bool warn)
        {
            bool canExecute = true;

            if (NetworkManager == null)
            {
                canExecute = false;
                if (warn)
                {
                    if (NetworkManager.CanLog(LoggingType.Warning))
                        Debug.LogWarning($"Cannot despawn {gameObject.name}, NetworkManager reference is null. This may occur if the object is not spawned or initialized.");
                }
            }
            else if (!IsServer)
            {
                canExecute = false;
                if (warn)
                {
                    if (NetworkManager.CanLog(LoggingType.Warning))
                        Debug.LogWarning($"Cannot spawn or despawn {gameObject.name}, server is not active.");
                }
            }
            else if (Deinitializing)
            {
                canExecute = false;
                if (warn)
                {
                    if (NetworkManager.CanLog(LoggingType.Warning))
                        Debug.LogWarning($"Cannot despawn {gameObject.name}, it is already deinitializing.");
                }
            }

            return canExecute;
        }

    }

}

