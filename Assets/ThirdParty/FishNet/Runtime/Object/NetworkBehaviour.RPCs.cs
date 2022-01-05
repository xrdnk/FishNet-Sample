﻿using FishNet.Connection;
using FishNet.Documenting;
using FishNet.Managing.Logging;
using FishNet.Object.Delegating;
using FishNet.Serializing;
using FishNet.Transporting;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FishNet.Object
{


    public abstract partial class NetworkBehaviour : MonoBehaviour
    {
        #region Private.
        /// <summary>
        /// Registered ServerRpc methods.
        /// </summary>
        private readonly Dictionary<uint, ServerRpcDelegate> _serverRpcDelegates = new Dictionary<uint, ServerRpcDelegate>();
        /// <summary>
        /// Registered ObserversRpc methods.
        /// </summary>
        private readonly Dictionary<uint, ClientRpcDelegate> _observersRpcDelegates = new Dictionary<uint, ClientRpcDelegate>();
        /// <summary>
        /// Registered TargetRpc methods.
        /// </summary>
        private readonly Dictionary<uint, ClientRpcDelegate> _targetRpcDelegates = new Dictionary<uint, ClientRpcDelegate>();
        /// <summary>
        /// Number of total RPC methods for scripts in the same inheritance tree for this instance.
        /// </summary>
        private uint _rpcMethodCount;
        /// <summary>
        /// Size of every rpcHash for this networkBehaviour.
        /// </summary>
        private byte _rpcHashSize = 1;
        /// <summary>
        /// RPCs buffered for new clients.
        /// </summary>
        private Dictionary<uint, (PooledWriter, Channel)> _bufferedRpcs = new Dictionary<uint, (PooledWriter, Channel)>();
        #endregion

        /// <summary>
        /// Called when buffered RPCs should be sent.
        /// </summary>
        internal void OnSendBufferedRpcs(NetworkConnection conn)
        {
            foreach ((PooledWriter writer, Channel ch) in _bufferedRpcs.Values)
                NetworkObject.NetworkManager.TransportManager.SendToClient((byte)ch, writer.GetArraySegment(), conn);
        }

        /// <summary>
        /// Registers a RPC method.
        /// Internal use.
        /// </summary>
        /// <param name="rpcHash"></param>
        /// <param name="del"></param>
        [APIExclude] //codegen this can be made protected internal then set public via codegen
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal void RegisterServerRpc(uint rpcHash, ServerRpcDelegate del)
        {
            bool contains = _serverRpcDelegates.ContainsKey(rpcHash);
            _serverRpcDelegates[rpcHash] = del;
            if (!contains)
                IncreaseRpcMethodCount();
        }
        /// <summary>
        /// Registers a RPC method.
        /// Internal use.
        /// </summary>
        /// <param name="rpcHash"></param>
        /// <param name="del"></param>
        [APIExclude] //codegen this can be made protected internal then set public via codegen
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal void RegisterObserversRpc(uint rpcHash, ClientRpcDelegate del)
        {
            bool contains = _observersRpcDelegates.ContainsKey(rpcHash);
            _observersRpcDelegates[rpcHash] = del;
            if (!contains)
                IncreaseRpcMethodCount();
        }
        /// <summary>
        /// Registers a RPC method.
        /// Internal use.
        /// </summary>
        /// <param name="rpcHash"></param>
        /// <param name="del"></param>
        [APIExclude] //codegen this can be made protected internal then set public via codegen
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected internal void RegisterTargetRpc(uint rpcHash, ClientRpcDelegate del)
        {
            bool contains = _targetRpcDelegates.ContainsKey(rpcHash);
            _targetRpcDelegates[rpcHash] = del;
            if (!contains)
                IncreaseRpcMethodCount();
        }

        /// <summary>
        /// Increases rpcMethodCount and rpcHashSize.
        /// </summary>
        private void IncreaseRpcMethodCount()
        {
            _rpcMethodCount++;
            if (_rpcMethodCount <= byte.MaxValue)
                _rpcHashSize = 1;
            else
                _rpcHashSize = 2;
        }

        /// <summary>
        /// Clears all buffered RPCs for this NetworkBehaviour.
        /// </summary>
        public void ClearBuffedRpcs()
        {
            foreach ((PooledWriter writer, Channel _) in _bufferedRpcs.Values)
                writer.Dispose();
            _bufferedRpcs.Clear();
        }

        /// <summary>
        /// Reads a RPC hash.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private uint ReadRpcHash(PooledReader reader)
        {
            if (_rpcHashSize == 1)
                return reader.ReadByte();
            else
                return reader.ReadUInt16();
        }
        /// <summary>
        /// Called when a ServerRpc is received.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnServerRpc(PooledReader reader, NetworkConnection sendingClient, Channel channel)
        {
            uint methodHash = ReadRpcHash(reader);

            if (sendingClient == null)
            {
                if (NetworkObject.NetworkManager.CanLog(LoggingType.Error))
                    Debug.LogError($"NetworkConnection is null. ServerRpc {methodHash} will not complete. Remainder of packet may become corrupt.");
                return;
            }

            if (_serverRpcDelegates.TryGetValue(methodHash, out ServerRpcDelegate data))
            {
                data.Invoke(this, reader, channel, sendingClient);
            }
            else
            {
                if (NetworkObject.NetworkManager.CanLog(LoggingType.Warning))
                    Debug.LogWarning($"ServerRpc not found for hash {methodHash}. Remainder of packet may become corrupt.");
            }
        }

        /// <summary>
        /// Called when an ObserversRpc is received.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnObserversRpc(uint? methodHash, PooledReader reader, Channel channel)
        {
            if (methodHash == null)
                methodHash = ReadRpcHash(reader);

            if (_observersRpcDelegates.TryGetValue(methodHash.Value, out ClientRpcDelegate del))
            {
                del.Invoke(this, reader, channel);
            }
            else
            {
                if (NetworkObject.NetworkManager.CanLog(LoggingType.Warning))
                    Debug.LogWarning($"ObserversRpc not found for hash {methodHash.Value}. Remainder of packet may become corrupt.");
            }
        }

        /// <summary>
        /// Called when an TargetRpc is received.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void OnTargetRpc(uint? methodHash, PooledReader reader, Channel channel)
        {
            if (methodHash == null)
                methodHash = ReadRpcHash(reader);

            if (_targetRpcDelegates.TryGetValue(methodHash.Value, out ClientRpcDelegate del))
            {
                del.Invoke(this, reader, channel);
            }
            else
            {
                if (NetworkObject.NetworkManager.CanLog(LoggingType.Warning))
                    Debug.LogWarning($"TargetRpc not found for hash {methodHash.Value}. Remainder of packet may become corrupt.");
            }
        }

        /// <summary>
        /// Sends a RPC to server.
        /// Internal use.
        /// </summary>
        /// <param name="rpcHash"></param>
        /// <param name="methodWriter"></param>
        /// <param name="channel"></param>
        [APIExclude] //codegen this can be made internal then set public via codegen
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendServerRpc(uint rpcHash, PooledWriter methodWriter, Channel channel)
        {
            if (!IsSpawnedWithWarning())
                return;

            PooledWriter writer = CreateRpc(rpcHash, methodWriter, PacketId.ServerRpc, channel);
            NetworkObject.NetworkManager.TransportManager.SendToServer((byte)channel, writer.GetArraySegment());
            writer.Dispose();
        }
        /// <summary>
        /// Sends a RPC to observers.
        /// Internal use.
        /// </summary>
        /// <param name="rpcHash"></param>
        /// <param name="methodWriter"></param>
        /// <param name="channel"></param>
        [APIExclude] //codegen this can be made internal then set public via codegen
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendObserversRpc(uint rpcHash, PooledWriter methodWriter, Channel channel, bool buffered)
        {
            if (!IsSpawnedWithWarning())
                return;

            PooledWriter writer;
            if (_rpcLinks.TryGetValue(rpcHash, out RpcLinkType link))
                writer = CreateLinkedRpc(link, methodWriter, channel);
            else
                writer = CreateRpc(rpcHash, methodWriter, PacketId.ObserversRpc, channel);

            NetworkObject.NetworkManager.TransportManager.SendToClients((byte)channel, writer.GetArraySegment(), NetworkObject.Observers);
            /* If buffered then dispose of any already buffered
             * writers and replace with new one. Writers should
             * automatically dispose when references are lost
             * anyway but better safe than sorry. */
            if (buffered)
            {
                if (_bufferedRpcs.TryGetValue(rpcHash, out (PooledWriter pw, Channel ch) result))
                    result.pw.Dispose();
                _bufferedRpcs[rpcHash] = (writer, channel);
            }
            //If not buffered then dispose immediately.
            else
            {
                writer.Dispose();
            }
        }

        /// <summary>
        /// Sends a RPC to target.
        /// Internal use.
        /// </summary>
        /// <param name="rpcHash"></param>
        /// <param name="methodWriter"></param>
        /// <param name="channel"></param>
        /// <param name="target"></param>
        [APIExclude] //codegen this can be made internal then set public via codegen
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SendTargetRpc(uint rpcHash, PooledWriter methodWriter, Channel channel, NetworkConnection target)
        {
            if (!IsSpawnedWithWarning())
                return;

            /* These checks could be codegened in to save a very very small amount of performance
             * by performing them before the serializer is written, but the odds of these failing
             * are very low and I'd rather keep the complexity out of codegen. */
            if (target == null)
            {
                if (NetworkObject.NetworkManager.CanLog(LoggingType.Warning))
                    Debug.LogWarning($"Action cannot be completed as no Target is specified.");
                return;
            }
            else
            {
                /* If not using observers, sending to owner,
                 * or observers contains target. */
                //bool canSendTotarget = (!NetworkObject.UsingObservers ||
                //    NetworkObject.OwnerId == target.ClientId ||
                //    NetworkObject.Observers.Contains(target));
                bool canSendTotarget = NetworkObject.OwnerId == target.ClientId || NetworkObject.Observers.Contains(target);

                if (!canSendTotarget)
                {
                    if (NetworkObject.NetworkManager.CanLog(LoggingType.Warning))
                        Debug.LogWarning($"Action cannot be completed as Target is not an observer for object {gameObject.name}");
                    return;
                }
            }

            PooledWriter writer;
            if (_rpcLinks.TryGetValue(rpcHash, out RpcLinkType link))
                writer = CreateLinkedRpc(link, methodWriter, channel);
            else
                writer = CreateRpc(rpcHash, methodWriter, PacketId.TargetRpc, channel);

            NetworkObject.NetworkManager.TransportManager.SendToClient((byte)channel, writer.GetArraySegment(), target);
            writer.Dispose();
        }


        /// <summary>
        /// Returns if spawned and throws a warning if not.
        /// </summary>
        /// <returns></returns>
        private bool IsSpawnedWithWarning()
        {
            bool result = this.IsSpawned;
            if (!result)
            {
                if (NetworkObject.NetworkManager.CanLog(LoggingType.Warning))
                    Debug.LogWarning($"Action cannot be completed as object {gameObject.name} is not spawned.");
            }

            return result;
        }

        /// <summary>
        /// Writes a full RPC and returns the writer.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private PooledWriter CreateRpc(uint rpcHash, PooledWriter methodWriter, PacketId packetId, Channel channel)
        {
            //Writer containing full packet.
            PooledWriter writer = WriterPool.GetWriter();
            writer.WritePacketId(packetId);
            writer.WriteNetworkBehaviour(this);
            //Only write length if unreliable.
            if (channel == Channel.Unreliable)
                WriteUnreliableLength(writer, methodWriter.Length + _rpcHashSize);
            //Hash and data.
            WriteRpcHash(rpcHash, writer);
            writer.WriteArraySegment(methodWriter.GetArraySegment());
            return writer;
        }

        /// <summary>
        /// Writes length to a writer for an unreliable packet.
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="length"></param>
        private void WriteUnreliableLength(PooledWriter writer, int length)
        {
            /* Length over short.MaxValue will just
             * dump the packet. It's unrealistic that someone
             * would ever send this much data
             * unreliably. */
            if (length > short.MaxValue)
                writer.WriteInt16(-2);
            else
                writer.WriteInt16((short)length);
        }


        /// <summary>
        /// Writes rpcHash to writer.
        /// </summary>
        /// <param name="rpcHash"></param>
        /// <param name="writer"></param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void WriteRpcHash(uint rpcHash, PooledWriter writer)
        {
            if (_rpcHashSize == 1)
                writer.WriteByte((byte)rpcHash);
            else
                writer.WriteUInt16((byte)rpcHash);
        }
    }


}