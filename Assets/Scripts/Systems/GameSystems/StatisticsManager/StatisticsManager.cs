using FishNet.Connection;
using FishNet.Object.Synchronizing;
using UnityEditor.Rendering.Universal;
using UnityEngine;

public class StatisticsManager : MonoBehaviour{
    private static StatisticsManager _instance;
    public static StatisticsManager Instance {
        get {
            if (_instance is null) {
                _instance = FindObjectOfType<StatisticsManager>();
                if (_instance is null) {
                    var obj = Instantiate(new GameObject("StatisticsManager"));
                    _instance = obj.AddComponent<StatisticsManager>();
                }
            }
            return _instance;
        }
    }

    [SyncObject]
    private readonly SyncDictionary<NetworkConnection, StatisticsData> playerStatistics = new SyncDictionary<NetworkConnection, StatisticsData>();
    [SyncObject]
    private readonly SyncDictionary<NetworkConnection, RoundStatisticsData> playerRoundStatistics = new SyncDictionary<NetworkConnection, RoundStatisticsData>();

    private void Awake() {
        GameEventManager.Instance.OnGameEnd += Instance_OnGameEnd;
        RoundEventManager.Instance.OnRoundStart += Instance_OnRoundStart;
        RoundEventManager.Instance.OnRoundEnd += Instance_OnRoundEnd;
        PlayerEventManager.Instance.OnPlayerConnected += Instance_OnPlayerConnected;
        PlayerEventManager.Instance.OnPlayerDisconnected += Instance_OnPlayerDisconnected;
        PlayerEventManager.Instance.OnPlayerDeath += Instance_OnPlayerDeath;
        ObjectiveEventManager.Instance.OnObjectiveStarted += Instance_OnObjectiveStarted;
        ObjectiveEventManager.Instance.OnObjectiveComplete += Instance_OnObjectiveComplete;
    }


    private void OnDestroy() {
        GameEventManager.Instance.OnGameEnd -= Instance_OnGameEnd;
        RoundEventManager.Instance.OnRoundStart += Instance_OnRoundStart;
        RoundEventManager.Instance.OnRoundEnd -= Instance_OnRoundEnd;
        PlayerEventManager.Instance.OnPlayerConnected -= Instance_OnPlayerConnected;
        PlayerEventManager.Instance.OnPlayerDisconnected -= Instance_OnPlayerDisconnected;
        PlayerEventManager.Instance.OnPlayerDeath -= Instance_OnPlayerDeath;
        ObjectiveEventManager.Instance.OnObjectiveStarted -= Instance_OnObjectiveStarted;
        ObjectiveEventManager.Instance.OnObjectiveComplete -= Instance_OnObjectiveComplete;
    }

    private void Instance_OnPlayerConnected(NetworkConnection obj) {
        playerStatistics.Add(obj, new StatisticsData());
    }
    private void Instance_OnPlayerDisconnected(NetworkConnection obj) {
        playerStatistics.Remove(obj);
        if (playerRoundStatistics.ContainsKey(obj)) {
            playerRoundStatistics.Remove(obj);
        }
    }

    private void Instance_OnGameEnd() {
        foreach (NetworkConnection player in playerStatistics.Keys) {
            playerStatistics[player].Reset();
        }
        playerRoundStatistics.Clear();
    }
    private void Instance_OnRoundStart() {
        foreach (NetworkConnection player in playerStatistics.Keys) {
            playerRoundStatistics.Add(player,default);
        }
    }

    private void Instance_OnRoundEnd(Teams team) {
        NetworkConnection currentMVP = null;
        int currentMVPPoints = -1000;
        bool TiedMVP = false;
        foreach (var player in playerRoundStatistics.Keys) {
            if (playerRoundStatistics[player].AssessValue() > currentMVPPoints) {
                currentMVP = player;
                currentMVPPoints = playerRoundStatistics[player].AssessValue();
                TiedMVP = false;
            } else if (playerRoundStatistics[player].AssessValue() == currentMVPPoints) {
                TiedMVP = true;
            }
        }
        if (!TiedMVP && currentMVP != null) {
            StatisticsData stats = playerStatistics[currentMVP];
            stats.MVPs++;
            playerStatistics.Dirty(currentMVP);
        }
        playerRoundStatistics.Clear();
    }

    private void Instance_OnPlayerDeath(NetworkConnection pDeader, NetworkConnection pKiller, NetworkConnection pAssister) {
        // On player death give point to dead, Give reward to killer, Give reward to assister
        if (playerStatistics.ContainsKey(pDeader)) {
            StatisticsData stats = playerStatistics[pDeader];
            stats.deaths++;
            playerStatistics.Dirty(pDeader);
        }
        if (playerStatistics.ContainsKey(pKiller)) {
            StatisticsData stats = playerStatistics[pKiller];
            stats.kills++;
            playerStatistics.Dirty(pKiller);
        }
        if (playerStatistics.ContainsKey(pAssister)) {
            StatisticsData stats = playerStatistics[pAssister];
            stats.assists++;
            playerStatistics.Dirty(pAssister);
        }

        if (playerRoundStatistics.ContainsKey(pDeader)) {
            RoundStatisticsData stats = playerRoundStatistics[pDeader];
            stats.deaths++;
            playerRoundStatistics.Dirty(pDeader);
        }
        if (playerRoundStatistics.ContainsKey(pKiller)) {
            RoundStatisticsData stats = playerRoundStatistics[pKiller];
            stats.kills++;
            playerRoundStatistics.Dirty(pKiller);
        }
        if (playerRoundStatistics.ContainsKey(pAssister)) {
            RoundStatisticsData stats = playerRoundStatistics[pAssister];
            stats.assists++;
            playerRoundStatistics.Dirty(pAssister);
        }
    }


    private void Instance_OnObjectiveStarted(Teams team, NetworkConnection conn) {
        // On bomb plant, give reward to planter
        if (playerStatistics.ContainsKey(conn)) {
            StatisticsData stats = playerStatistics[conn];
            stats.objectives++;
            playerStatistics.Dirty(conn);
        }
        if (playerRoundStatistics.ContainsKey(conn)) {
            RoundStatisticsData stats = playerRoundStatistics[conn];
            stats.objectives++;
            playerRoundStatistics.Dirty(conn);
        }
    }
    private void Instance_OnObjectiveComplete(Teams team, NetworkConnection conn) {
        // On bomb defuse, give reward to defuser
        if (playerStatistics.ContainsKey(conn)) {
            StatisticsData stats = playerStatistics[conn];
            stats.objectives++;
            playerStatistics.Dirty(conn);
        }
        if (playerRoundStatistics.ContainsKey(conn)) {
            RoundStatisticsData stats = playerRoundStatistics[conn];
            stats.objectives++;
            playerRoundStatistics.Dirty(conn);
        }
    }

}
