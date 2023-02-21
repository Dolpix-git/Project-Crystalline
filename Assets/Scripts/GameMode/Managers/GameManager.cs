using FishNet;
using FishNet.Component.Spawning;
using FishNet.Connection;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : NetworkBehaviour{
    public static GameManager _instance;
    public static GameManager Instance {
        get {
            if (_instance is null) {
                _instance = FindObjectOfType<GameManager>();
                if (_instance is null) {
                    var obj = Instantiate(new GameObject("GameManager"));
                    CustomLogger.LogWarning("GameManager was acsessed and there was no GameManager!");
                    _instance = obj.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    [SerializeField] private BaseGameMode gameMode;

    public Dictionary<NetworkConnection,NetworkObject> Players { get => PlayerManager.Instance.Players; }
    private bool stoppingServer;

    private void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }
        gameMode.Manager = this;

        PlayerManager.Instance.OnSpawned += PlayerSpawner_OnSpawned;
        PlayerManager.Instance.OnDisconect += PlayerSpawner_OnDisconect;
    }
    public override void OnStartServer() {
        base.OnStartServer();

        if (!base.IsServer) { return; }
        CustomLogger.Log(LogCategories.GameManager , "Start");
        gameMode.StartGame();
    }
    public override void OnStopServer() {
        base.OnStopServer();

        if (!base.IsServer) { return; }
        CustomLogger.Log(LogCategories.GameManager , "End");
        stoppingServer = true;
        gameMode.EndGame();

        PlayerManager.Instance.OnSpawned -= PlayerSpawner_OnSpawned;
        PlayerManager.Instance.OnDisconect -= PlayerSpawner_OnDisconect;
    }

    public void GameHasEnded() {
        if (!base.IsServer || stoppingServer) { return; }

        gameMode.StartGame();
    }

    private void PlayerSpawner_OnSpawned(NetworkObject nob) {
        if (!base.IsServer) { return; }
        nob.GetComponent<Health>().OnDeath += PlayerHasDied;

        if (gameMode.GameInProgress) {
            gameMode.AddLateJoiner(nob);
        }
    }
    private void PlayerSpawner_OnDisconect(NetworkObject nob) {
        if (!base.IsServer) { return; }
        gameMode.PlayerLeaveGamemode(nob);
    }
    public void PlayerHasDied() {
        if (!base.IsServer) { return; }
        gameMode.PlayerDeathUpdate();
    }
}
