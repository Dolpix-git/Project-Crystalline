using FishNet.Connection;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class EconomyManager : MonoBehaviour {
    private int baseMoney = 5;
    private int roundWinMoney = 5;
    private int roundLoseMoney = 5;
    private int playerKillMoney = 5;
    private int playerKillAssistMoney = 5;
    private int objectiveStartMoney = 5;
    private int objectiveEndMoney = 5;
    private int maxPlayerMoney = 5000;

    private static EconomyManager _instance;
    public static EconomyManager Instance {
        get {
            if (_instance is null) {
                _instance = FindObjectOfType<EconomyManager>();
                if (_instance is null) {
                    var obj = Instantiate(new GameObject("EconomyManager"));
                    _instance = obj.AddComponent<EconomyManager>();
                }
            }
            return _instance;
        }
    }

    [SyncObject]
    private readonly SyncDictionary<NetworkConnection, int> playerEconomy = new SyncDictionary<NetworkConnection, int>();

    private void Awake() {
        GameEventManager.Instance.OnGameStart += Instance_OnGameStart;
        GameEventManager.Instance.OnGameEnd += Instance_OnGameEnd;
        RoundEventManager.Instance.OnRoundEnd += Instance_OnRoundEnd;
        RoundEventManager.Instance.OnTeamFlipFlop += Instance_OnTeamFlipFlop;
        PlayerEventManager.Instance.OnPlayerConnected += Instance_OnPlayerConnected;
        PlayerEventManager.Instance.OnPlayerDisconnected += Instance_OnPlayerDisconnected;
        PlayerEventManager.Instance.OnPlayerDeath += Instance_OnPlayerDeath;
        ObjectiveEventManager.Instance.OnObjectiveStarted += Instance_OnObjectiveStarted;
        ObjectiveEventManager.Instance.OnObjectiveComplete += Instance_OnObjectiveComplete;
    }



    private void OnDestroy() {
        GameEventManager.Instance.OnGameStart -= Instance_OnGameStart;
        GameEventManager.Instance.OnGameEnd -= Instance_OnGameEnd;
        RoundEventManager.Instance.OnRoundEnd -= Instance_OnRoundEnd;
        RoundEventManager.Instance.OnTeamFlipFlop -= Instance_OnTeamFlipFlop;
        PlayerEventManager.Instance.OnPlayerConnected -= Instance_OnPlayerConnected;
        PlayerEventManager.Instance.OnPlayerDisconnected -= Instance_OnPlayerDisconnected;
        PlayerEventManager.Instance.OnPlayerDeath -= Instance_OnPlayerDeath;
        ObjectiveEventManager.Instance.OnObjectiveStarted -= Instance_OnObjectiveStarted;
        ObjectiveEventManager.Instance.OnObjectiveComplete -= Instance_OnObjectiveComplete;
    }

    private void Instance_OnPlayerDisconnected(NetworkConnection obj) {
        playerEconomy.Add(obj, 0);
    }

    private void Instance_OnPlayerConnected(NetworkConnection obj) {
        playerEconomy.Remove(obj);
    }


    private void Instance_OnGameStart() {
        // On game start, give base money
        foreach (NetworkConnection player in playerEconomy.Keys) {
            playerEconomy[player] = baseMoney;
        }
    }
    private void Instance_OnTeamFlipFlop(Teams arg1, Teams arg2) {
        // On game flip flop, reset money, give base money 
        foreach (NetworkConnection player in playerEconomy.Keys) {
            playerEconomy[player] = baseMoney;
        }
    }
    private void Instance_OnGameEnd() {
        // On game end, reset money
        foreach (NetworkConnection player in playerEconomy.Keys) {
            playerEconomy[player] = 0;
        }
    }

    private void Instance_OnRoundEnd(Teams team) {
        // On round end, give money for wining to winning team
        foreach (NetworkConnection player in playerEconomy.Keys) {
            if (TeamManager.Instance.playerTeams[player] == team) {
                playerEconomy[player] = Mathf.Min(roundWinMoney + playerEconomy[player], maxPlayerMoney);
            } else {
                playerEconomy[player] = Mathf.Min(roundLoseMoney + playerEconomy[player], maxPlayerMoney);
            }
        }
    }

    private void Instance_OnPlayerDeath(NetworkConnection pDeader, NetworkConnection pKiller, NetworkConnection pAssister) {
        // On player death give point to dead, Give reward to killer, Give reward to assister
        if (playerEconomy.ContainsKey(pKiller)) {
            playerEconomy[pKiller] = Mathf.Min(playerKillMoney + playerEconomy[pKiller], maxPlayerMoney);
        }
        if (playerEconomy.ContainsKey(pAssister)) {
            playerEconomy[pAssister] = Mathf.Min(playerKillAssistMoney + playerEconomy[pAssister], maxPlayerMoney);
        }
    }


    private void Instance_OnObjectiveStarted(Teams team, NetworkConnection conn) {
        // On bomb plant, give reward to planter
        if (playerEconomy.ContainsKey(conn)) {
            playerEconomy[conn] += Mathf.Min(objectiveStartMoney + playerEconomy[conn], maxPlayerMoney);
        }
    }
    private void Instance_OnObjectiveComplete(Teams team, NetworkConnection conn) {
        // On bomb defuse, give reward to defuser
        if (playerEconomy.ContainsKey(conn)) {
            playerEconomy[conn] += Mathf.Min(objectiveEndMoney + playerEconomy[conn], maxPlayerMoney);
        }
    }
}
