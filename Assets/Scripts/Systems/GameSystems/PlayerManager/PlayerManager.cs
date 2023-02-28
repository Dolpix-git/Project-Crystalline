using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using System.Collections.Generic;
using UnityEngine;


public class PlayerManager : NetworkBehaviour {
    #region Singleton Pattern.
    private static PlayerManager _instance;
    public static PlayerManager Instance {
        get {
            if (_instance is null) {
                _instance = FindObjectOfType<PlayerManager>();
                if (_instance is null) {
                    var obj = Instantiate(new GameObject("PlayerManager"));
                    _instance = obj.AddComponent<PlayerManager>();
                }
            }
            return _instance;
        }
    }
    #endregion
    #region Public.
    /// <summary>
    /// Called on the server when a player is spawned.
    /// </summary>
    public event Action<NetworkObject> OnSpawned;
    /// <summary>
    /// Called on the server when a player has disconected.
    /// </summary>
    public event Action<NetworkObject> OnDisconect;
    #endregion

    #region Serialized.
    /// <summary>
    /// Prefab to spawn for the player.
    /// </summary>
    [Tooltip("Prefab to spawn for the player.")]
    [SerializeField]
    private NetworkObject _playerPrefab;
    /// <summary>
    /// True to add player to the active scene when no global scenes are specified through the SceneManager.
    /// </summary>
    [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
    [SerializeField]
    private bool _addToDefaultScene = true;
    #endregion

    #region Private.
    /// <summary>
    /// NetworkManager on this object or within this objects parents.
    /// </summary>
    private NetworkManager _networkManager;
    /// <summary>
    /// Dictionary of the player prefab and there connection.
    /// </summary>
    private Dictionary<NetworkConnection, NetworkObject> players;
    #endregion

    #region Getter Setter.
    public Dictionary<NetworkConnection, NetworkObject> Players { 
        get {
            if (players is null) {
                players = new Dictionary<NetworkConnection, NetworkObject>();
            }
            return players;
        } }
    #endregion


    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }
    }

    private void Start() {
        InitializeOnce();
    }

    /// <summary>
    /// Initializes this script for use.
    /// </summary>
    private void InitializeOnce() {
        _networkManager = InstanceFinder.NetworkManager;
        if (_networkManager == null) {
            CustomLogger.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }

        _networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
        _networkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
    }
    private void OnDestroy() {
        if (_networkManager != null)
            _networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
        _networkManager.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
    }

    public override void OnStartServer() {
        base.OnStartServer();
        players = new Dictionary<NetworkConnection, NetworkObject>();
    }

    public override void OnStopServer() {
        base.OnStopServer();
        players.Clear();
    }


    /// <summary>
    /// Called when a client loads initial scenes after connecting.
    /// </summary>
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer) {
        if (!asServer)
            return;
        if (_playerPrefab == null) {
            CustomLogger.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }


        NetworkObject nob = _networkManager.GetPooledInstantiated(_playerPrefab, true);
        nob.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        _networkManager.ServerManager.Spawn(nob, conn);

        nob.GetComponent<Health>().Disable();

        //If there are no global scenes 
        if (_addToDefaultScene)
            _networkManager.SceneManager.AddOwnerToDefaultScene(nob);

        players.Add(conn,nob);
        OnSpawned?.Invoke(nob);
    }

    private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args) {
        if (args.ConnectionState == RemoteConnectionState.Stopped) {
            CustomLogger.Log(LogCategories.PlayerManager, args.ConnectionState);
            OnDisconect?.Invoke(players[conn]);
            players.Remove(conn);
        }
    }
}