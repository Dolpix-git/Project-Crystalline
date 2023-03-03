using FishNet.Managing;
using FishNet.Transporting;
using UnityEngine;
using Steamworks;
using Steamworks.Data;
using Color = UnityEngine.Color;
using System.Collections;

public class NetworkHudCanvases : MonoBehaviour {
    #region Types.
    /// <summary>
    /// Ways the HUD will automatically start a connection.
    /// </summary>
    private enum AutoStartType {
        Disabled,
        Host,
        Server,
        Client
    }
    #endregion

    #region Serialized.
    /// <summary>
    /// What connections to automatically start on play.
    /// </summary>
    [Tooltip("What connections to automatically start on play.")]
    [SerializeField]
    private AutoStartType _autoStartType = AutoStartType.Disabled;
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
    private UnityEngine.UI.Image _serverIndicator;
    /// <summary>
    /// Indicator for client state.
    /// </summary>
    [Tooltip("Indicator for client state.")]
    [SerializeField]
    private UnityEngine.UI.Image _clientIndicator;
    #endregion

    #region Private.
    /// <summary>
    /// Found NetworkManager.
    /// </summary>
    private NetworkManager _networkManager;
    /// <summary>
    /// Current state of client socket.
    /// </summary>
    private LocalConnectionState _clientState = LocalConnectionState.Stopped;
    /// <summary>
    /// Current state of server socket.
    /// </summary>
    private LocalConnectionState _serverState = LocalConnectionState.Stopped;

    public Transport facepunch;

    #endregion

    private void Start() {
        _networkManager = FindObjectOfType<NetworkManager>();
        if (_networkManager == null) {
            Debug.Log("NetworkManager not found, HUD will not function.");
            return;
        } else {
            UpdateColor(LocalConnectionState.Stopped, ref _serverIndicator);
            UpdateColor(LocalConnectionState.Stopped, ref _clientIndicator);
            _networkManager.ServerManager.OnServerConnectionState += ServerManager_OnServerConnectionState;
            _networkManager.ClientManager.OnClientConnectionState += ClientManager_OnClientConnectionState;
        }

        if (_autoStartType == AutoStartType.Host || _autoStartType == AutoStartType.Server)
            OnClick_Server();
        if (!Application.isBatchMode && (_autoStartType == AutoStartType.Host || _autoStartType == AutoStartType.Client))
            OnClick_Client();

        SteamMatchmaking.OnLobbyCreated += OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
    }


    private void OnDestroy() {
        if (_networkManager == null)
            return;

        _networkManager.ServerManager.OnServerConnectionState -= ServerManager_OnServerConnectionState;
        _networkManager.ClientManager.OnClientConnectionState -= ClientManager_OnClientConnectionState;

        SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;
    }

    /// <summary>
    /// Updates img color baased on state.
    /// </summary>
    /// <param name="state"></param>
    /// <param name="img"></param>
    private void UpdateColor(LocalConnectionState state, ref UnityEngine.UI.Image img) {
        Color c;
        if (state == LocalConnectionState.Started)
            c = _startedColor;
        else if (state == LocalConnectionState.Stopped)
            c = _stoppedColor;
        else
            c = _changingColor;

        img.color = c;
    }

    private void ClientManager_OnClientConnectionState(ClientConnectionStateArgs obj) {
        _clientState = obj.ConnectionState;
        UpdateColor(obj.ConnectionState, ref _clientIndicator);
    }


    private void ServerManager_OnServerConnectionState(ServerConnectionStateArgs obj) {
        _serverState = obj.ConnectionState;
        UpdateColor(obj.ConnectionState, ref _serverIndicator);
    }


    public void OnClick_Server() {
        if (_networkManager == null)
            return;

        if (_serverState != LocalConnectionState.Stopped) {
            StartCoroutine(ShutDownServer());
        } else if (_serverState == LocalConnectionState.Stopped) {
            _networkManager.ServerManager.StartConnection();
            StartHost(10);
        }
    }

    private IEnumerator ShutDownServer() {
        yield return new WaitUntil(() => _serverState == LocalConnectionState.Started);
        _networkManager.ServerManager.StopConnection(true);
    }
    

    public void OnClick_Client() {
        if (_networkManager == null)
            return;

        if (_clientState != LocalConnectionState.Stopped)
            _networkManager.ClientManager.StopConnection();
        else
            _networkManager.ClientManager.StartConnection();
    }









    public Lobby? CurrentLobby { get; private set; } = null;

    private void OnApplicationQuit() => Disconnect();

    public async void StartHost(uint maxMembers) {
        if (facepunch != null) { CurrentLobby = await SteamMatchmaking.CreateLobbyAsync((int)maxMembers); }
    }

    public void StartClient(SteamId id) {
        if (facepunch != null) {
            facepunch.SetClientAddress(id.ToString());
        }

        OnClick_Client();

        Debug.Log($"Joining room hosted by {id}", this);
    }

    public void Disconnect() {
        CurrentLobby?.Leave();
    }





    #region Steam Callbacks

    private void OnGameLobbyJoinRequested(Lobby lobby, SteamId id) {
        bool isSame = lobby.Owner.Id.Equals(id);

        Debug.Log($"Owner: {lobby.Owner}");
        Debug.Log($"Id: {id}");
        Debug.Log($"IsSame: {isSame}", this);

        StartClient(id);
    }

    private void OnLobbyInvite(Friend friend, Lobby lobby) => Debug.Log($"You got a invite from {friend.Name}", this);

    private void OnLobbyMemberLeave(Lobby lobby, Friend friend) { }

    private void OnLobbyMemberJoined(Lobby lobby, Friend friend) { }

    private void OnLobbyEntered(Lobby lobby) {
        StartClient(lobby.Owner.Id);
    }

    private void OnLobbyCreated(Result result, Lobby lobby) {
        if (result != Result.OK) {
            Debug.LogError($"Lobby couldn't be created!, {result}", this);
            return;
        }

        lobby.SetPublic();
        lobby.SetData("name", "Random Cool Lobby");
        lobby.SetJoinable(true);

        Debug.Log("Lobby has been created!");
    }

    #endregion
}
