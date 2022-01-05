﻿using FishNet.Connection;
using FishNet.Documenting;
using FishNet.Object.Synchronizing.Internal;
using UnityEngine;

namespace FishNet.Object
{

    public abstract partial class NetworkBehaviour : MonoBehaviour
    {
        #region Public.
        /// <summary>
        /// True if OnStartServer has been called.
        /// </summary>
        [APIExclude]
        public bool OnStartServerCalled { get; private set; }
        /// <summary>
        /// True if OnStartClient has been called.
        /// </summary>
        [APIExclude]
        public bool OnStartClientCalled { get; private set; }
        #endregion

        #region Private.
        /// <summary>
        /// True if OnStartNetwork has been called.
        /// </summary>
        private bool _onStartNetworkCalled;
        /// <summary>
        /// True if OnStopNetwork has been called.
        /// </summary>
        private bool _onStopNetworkCalled;
        #endregion

        /// <summary>
        /// Invokes cached callbacks on SyncTypes which were held until OnStartXXXXX was called.
        /// </summary>
        /// <param name="asServer"></param>
        private void InvokeSyncTypeCallbacks(bool asServer)
        {
            foreach (SyncBase item in _syncVars.Values)
                item.OnStartCallback(asServer);
            foreach (SyncBase item in _syncObjects.Values)
                item.OnStartCallback(asServer);
        }
        /// <summary>
        /// Invokes the OnStart/StopNetwork.
        /// </summary>
        /// <param name="start"></param>
        internal void InvokeOnNetwork(bool start)
        {
            if (start)
            {
                if (_onStartNetworkCalled)
                    return;
                OnStartNetwork();
            }
            else
            {
                if (_onStopNetworkCalled)
                    return;
                OnStopNetwork();
            }
        }

        /// <summary>
        /// Called when the network has initialized this object. May be called for server or client but will only be called once.
        /// When as host or server this method will run before OnStartServer. 
        /// When as client only the method will run before OnStartClient.
        /// </summary>
        public virtual void OnStartNetwork()
        {
            _onStartNetworkCalled = true;
            _onStopNetworkCalled = false;
        }
        /// <summary>
        /// Called when the network is deinitialized this object. May be called for server or client but will only be called once.
        /// When as host or server this method will run after OnStopServer.
        /// When as client only this method will run after OnStopClient.
        /// </summary>
        public virtual void OnStopNetwork()
        {
            _onStopNetworkCalled = true;
            _onStartNetworkCalled = false;
        }

        /// <summary>
        /// Called on the server after initializing this object.
        /// SyncTypes modified before or during this method will be sent to clients in the spawn message.
        /// </summary> 
        public virtual void OnStartServer()
        {
            OnStartServerCalled = true;
            InvokeSyncTypeCallbacks(true);
        }
        /// <summary>
        /// Called on the server before deinitializing this object.
        /// </summary>
        public virtual void OnStopServer()
        {
            OnStartServerCalled = false;
        }
        /// <summary>
        /// Called on the server after ownership has changed.
        /// </summary>
        /// <param name="prevOwner">Previous owner of this object.</param>
        public virtual void OnOwnershipServer(NetworkConnection prevOwner) { }
        /// <summary>
        /// Called on the server after a spawn message for this object has been sent to clients.
        /// Useful for sending remote calls or data to clients.
        /// </summary>
        /// <param name="connection">Connection the object is being spawned for.</param>
        public virtual void OnSpawnServer(NetworkConnection connection) { }
        /// <summary>
        /// Called on the server before a despawn message for this object has been sent to connection.
        /// Useful for sending remote calls or actions to clients.
        /// </summary>
        public virtual void OnDespawnServer(NetworkConnection connection) { }
        /// <summary>
        /// Called on the client after initializing this object.
        /// </summary>
        public virtual void OnStartClient()
        {
            OnStartClientCalled = true;
            InvokeSyncTypeCallbacks(false);
        }
        /// <summary>
        /// Called on the client before deinitializing this object.
        /// </summary>
        public virtual void OnStopClient()
        {
            OnStartClientCalled = false;
        }
        /// <summary>
        /// Called on the client after gaining or losing ownership.
        /// </summary>
        /// <param name="prevOwner">Previous owner of this object.</param>
        public virtual void OnOwnershipClient(NetworkConnection prevOwner) { }

    }


}