using FishNet;
using FishNet.Component.Spawning;
using FishNet.Object;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : NetworkBehaviour{
    public static GameManager Instance { get; private set; }
    
    private List<NetworkObject> players = new List<NetworkObject>();
    [SerializeField] private BaseGameMode gameMode;

    public List<NetworkObject> Players { get => players; set => players = value; }
    private bool stoppingServer;

    private void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this) {
            Destroy(this);
        } else {
            Instance = this;
        }
        players = new List<NetworkObject>();
        gameMode.Manager = this;

        InstanceFinder.NetworkManager.gameObject.GetComponent<PlayerSpawner>().OnSpawned += PlayerSpawner_OnSpawned;
    }
    public override void OnStartServer() {
        base.OnStartServer();

        if (!base.IsServer) { return; }
        CustomLogger.Log(LogCategories.GameManager , "Start");
        players.Clear();
        gameMode.StartGame();
    }
    public override void OnStopServer() {
        base.OnStopServer();

        if (!base.IsServer) { return; }
        CustomLogger.Log(LogCategories.GameManager , "End");
        stoppingServer = true;
        gameMode.EndGame();
        players.Clear();

        //InstanceFinder.NetworkManager.gameObject.GetComponent<PlayerSpawner>().OnSpawned -= PlayerSpawner_OnSpawned;
    }

    public void GameHasEnded() {
        if (!base.IsServer || stoppingServer) { return; }

        gameMode.StartGame();
    }

    private void PlayerSpawner_OnSpawned(NetworkObject nob) {
        if (!base.IsServer) { return; }
        players.Add(nob);
        if (gameMode.GameInProgress) {
            gameMode.AddLateJoiner(nob);
        }
    }
    private void PlayerSpawner_OnDisconect(NetworkObject nob) {
        if (!base.IsServer) { return; }
        players.Remove(nob);
        // Code to handle removing this person from the team.
    }
    public void PlayerHasDied(NetworkObject nob) {
        if (!base.IsServer) { return; }
        gameMode.PlayerDeathUpdate();
    }
}
