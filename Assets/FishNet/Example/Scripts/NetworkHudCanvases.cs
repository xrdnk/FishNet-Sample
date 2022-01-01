﻿using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NetworkHudCanvases : MonoBehaviour
{
    #region Public.
    /// <summary>
    /// True to auto start server and client.
    /// </summary>
    public bool AutoStart = true;
    #endregion

    #region Serialized.
    /// <summary>
    /// Color when socket is stopped.
    /// </summary>
    [Tooltip("Color when socket is stopped.")]
    [SerializeField]
    private Color _stoppedColor;
    /// <summary>
    /// Color when socket is changing.
    /// </summary>
    [Tooltip("Color when socket is changing.")]
    [SerializeField]
    private Color _changingColor;
    /// <summary>
    /// Color when socket is started.
    /// </summary>
    [Tooltip("Color when socket is started.")]
    [SerializeField]
    private Color _startedColor;
    [Header("Indicators")]
    /// <summary>
    /// Indicator for server state.
    /// </summary>
    [Tooltip("Indicator for server state.")]
    [SerializeField]
    private Image _serverIndicator;
    /// <summary>
    /// Indicator for client state.
    /// </summary>
    [Tooltip("Indicator for client state.")]
    [SerializeField]
    private Image _clientIndicator;
    #endregion

    #region Private.
    /// <summary>
    /// Found NetworkManager.
    /// </summary>
    private NetworkManager _networkManager;
    /// <summary>
    /// Current state of client socket.
    /// </summary>
    private LocalConnectionStates _clientState = LocalConnectionStates.Stopped;
    /// <summary>
    /// Current state of server socket.
    /// </summary>
    private LocalConnectionStates _serverState = LocalConnectionStates.Stopped;
    #endregion


    private void Start()
    {
        EventSystem systems = FindObjectOfType<EventSystem>();
        if (systems == null)
            gameObject.AddComponent<EventSystem>();
        BaseInputModule inputModule = FindObjectOfType<BaseInputModule>();
        if (inputModule == null)
            gameObject.AddComponent<StandaloneInputModule>();

        _networkManager = FindObjectOfType<NetworkManager>();
        if (_networkManager == null)
        {
            Debug.LogError("NetworkManager not found, HUD will not function.");
            return;
        }
        else
        {
            UpdateColor(LocalConnectionStates.Stopped, ref _serverIndicator);
            UpdateColor(LocalConnectionStates.Stopped, ref _clientIndicator);
            _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        }

        if (AutoStart)
        {
            OnClick_Server();
            if (!Application.isBatchMode)
                OnClick_Client();
        }
    }

    private void OnDestroy()
    {
        if (_networkManager == null)
            return;

        _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;
    }

    /// <summary>
    /// Updates img color baased on state.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="img"></param>
    private void UpdateColor(LocalConnectionStates state, ref Image img)
    {
        Color c;
        if (state == LocalConnectionStates.Started)
            c = _startedColor;
        else if (state == LocalConnectionStates.Stopped)
            c = _stoppedColor;
        else
            c = _changingColor;

        img.color = c;
    }


    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj)
    {
        _clientState = obj.ConnectionState;
        UpdateColor(obj.ConnectionState, ref _clientIndicator);
    }


    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj)
    {
        _serverState = obj.ConnectionState;
        UpdateColor(obj.ConnectionState, ref _serverIndicator);
    }


    public void OnClick_Server()
    {
        if (_networkManager == null)
            return;

        if (_serverState != LocalConnectionStates.Stopped)
            _networkManager.ServerManager.StopConnection(true);
        else
            _networkManager.ServerManager.StartConnection();
    }


    public void OnClick_Client()
    {
        if (_networkManager == null)
            return;

        if (_clientState != LocalConnectionStates.Stopped)
            _networkManager.ClientManager.StopConnection();
        else
            _networkManager.ClientManager.StartConnection();
    }
}
