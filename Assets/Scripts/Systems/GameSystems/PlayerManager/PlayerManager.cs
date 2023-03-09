using FishNet;
using FishNet.Connection;
using FishNet.Managing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using Unity.VisualScripting;
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

    #region Serialized.
    /// <summary>
    /// Prefab to spawn for the player.
    /// </summary>
    [Tooltip("Prefab to spawn for the player.")]
    [SerializeField]
    private NetworkObject playerPrefab;
    /// <summary>
    /// True to add player to the active scene when no global scenes are specified through the SceneManager.
    /// </summary>
    [Tooltip("True to add player to the active scene when no global scenes are specified through the SceneManager.")]
    [SerializeField]
    private bool addToDefaultScene = true;
    #endregion

    #region Private.
    /// <summary>
    /// NetworkManager on this object or within this objects parents.
    /// </summary>
    private NetworkManager networkManager;
    /// <summary>
    /// Dictionary of the player prefab and there connection.
    /// </summary>
    [SyncObject]
    public readonly SyncDictionary<NetworkConnection, NetworkObject> players = new SyncDictionary<NetworkConnection, NetworkObject>();
    #endregion


    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }
        players.OnChange += Players_OnChange;
    }

    private void Start() {
        InitializeOnce();
    }

    /// <summary>
    /// Initializes this script for use.
    /// </summary>
    private void InitializeOnce() {
        networkManager = InstanceFinder.NetworkManager;
        if (networkManager == null) {
            CustomLogger.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }

        networkManager.SceneManager.OnClientLoadedStartScenes += SceneManager_OnClientLoadedStartScenes;
        networkManager.ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;
    }
    private void OnDestroy() {
        if (networkManager == null) {
            CustomLogger.LogWarning($"PlayerSpawner on {gameObject.name} cannot work as NetworkManager wasn't found on this object or within parent objects.");
            return;
        }
        networkManager.SceneManager.OnClientLoadedStartScenes -= SceneManager_OnClientLoadedStartScenes;
        networkManager.ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;
        players.OnChange -= Players_OnChange;
    }


    public override void OnStopServer() {
        base.OnStopServer();
        players.Clear();
    }


    /// <summary>
    /// Called when a client loads initial scenes after connecting.
    /// </summary>
    private void SceneManager_OnClientLoadedStartScenes(NetworkConnection conn, bool asServer) {
        if (playerPrefab == null) {
            CustomLogger.LogWarning($"Player prefab is empty and cannot be spawned for connection {conn.ClientId}.");
            return;
        }

        if (asServer) {
            NetworkObject nob = networkManager.GetPooledInstantiated(playerPrefab, true);
            nob.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            networkManager.ServerManager.Spawn(nob, conn);

            nob.GetComponent<Health>().Disable();

            //If there are no global scenes 
            if (addToDefaultScene)
                networkManager.SceneManager.AddOwnerToDefaultScene(nob);

            players.Add(conn, nob);
            PlayerEventManager.Instance.InvokePlayerConnected(conn);
        }
    }

    private void ServerManager_OnRemoteConnectionState(NetworkConnection conn, RemoteConnectionStateArgs args) {
        if (args.ConnectionState == RemoteConnectionState.Stopped) {
            CustomLogger.Log(LogCategories.PlayerManager, args.ConnectionState);
            PlayerEventManager.Instance.InvokePlayerDisconnected(conn);
            players.Remove(conn);
        }
    }
    private void Players_OnChange(SyncDictionaryOperation op, NetworkConnection key, NetworkObject value, bool asServer) {
        if (op == SyncDictionaryOperation.Add) {
            if (value != null && !asServer) {
                PlayerEventManager.Instance.InvokePlayerClentConnected(value);
            }
        }
    }
}