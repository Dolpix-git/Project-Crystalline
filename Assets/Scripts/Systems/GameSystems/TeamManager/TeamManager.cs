using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class TeamManager : NetworkBehaviour {
    private static TeamManager _instance;
    public static TeamManager Instance {
        get {
            if (_instance is null) {
                _instance = FindObjectOfType<TeamManager>();
                if (_instance is null) {
                    var obj = Instantiate(new GameObject("TeamManager"));
                    _instance = obj.AddComponent<TeamManager>();
                }
            }
            return _instance;
        }
    }

    [SyncObject]
    public readonly SyncDictionary<NetworkConnection, Teams> playerTeams = new SyncDictionary<NetworkConnection, Teams>();
    [SyncObject]
    public readonly SyncDictionary<Teams, TeamData> teamsDict = new SyncDictionary<Teams, TeamData>();


    private void Awake() {
        PlayerEventManager.Instance.OnPlayerConnected += Instance_OnPlayerConnected;
        PlayerEventManager.Instance.OnPlayerDisconnected += Instance_OnPlayerDisconnected;
    }

    private void OnDestroy() {
        PlayerEventManager.Instance.OnPlayerConnected -= Instance_OnPlayerConnected;
        PlayerEventManager.Instance.OnPlayerDisconnected -= Instance_OnPlayerDisconnected;
    }


    private void Instance_OnPlayerDisconnected(NetworkConnection obj) {
        playerTeams.Add(obj,Teams.None);
    }

    private void Instance_OnPlayerConnected(NetworkConnection obj) {
        playerTeams.Remove(obj);
    }
}
