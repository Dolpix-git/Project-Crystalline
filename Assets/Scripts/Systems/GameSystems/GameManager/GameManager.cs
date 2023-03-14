using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class GameManager : NetworkBehaviour{
    public static GameManager _instance;
    public static GameManager Instance {
        get {
            if (_instance is null) {
                _instance = FindObjectOfType<GameManager>();
                if (_instance is null) {
                    var obj = Instantiate(new GameObject("GameManager"));
                    Log.LogWarning("GameManager was acsessed and there was no GameManager!");
                    _instance = obj.AddComponent<GameManager>();
                }
            }
            return _instance;
        }
    }

    [SerializeField] private BaseGameMode gameMode;

    [SyncObject]
    public readonly SyncTimer gameTimer = new SyncTimer();

    private bool stoppingServer;

    private void Awake() {
        // If there is an instance, and it's not me, delete myself.
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }
        gameMode.Manager = this;
    }
    public override void OnStartServer() {
        base.OnStartServer();

        if (!base.IsServer) { return; }
        Log.LogMsg(LogCategories.GameManager , "Start");
        gameMode.StartGame();
    }
    public override void OnStopServer() {
        base.OnStopServer();

        if (!base.IsServer) { return; }
        Log.LogMsg(LogCategories.GameManager , "End");
        stoppingServer = true;
        gameMode.EndGame();
    }

    public void GameHasEnded() {
        if (!base.IsServer || stoppingServer) { return; }

        gameMode.StartGame();
    }
}
