using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using System.Linq;
using UnityEngine;

public class EconomyManager : NetworkBehaviour {
    [SerializeField] private int baseMoney = 5;
    [SerializeField] private int roundWinMoney = 5;
    [SerializeField] private int roundLoseMoney = 5;
    [SerializeField] private int playerKillMoney = 5;
    [SerializeField] private int playerKillAssistMoney = 5;
    [SerializeField] private int objectiveStartMoney = 5;
    [SerializeField] private int objectiveEndMoney = 5;
    [SerializeField] private int maxPlayerMoney = 5000;

    private static EconomyManager _instance;
    [SyncObject]
    private readonly SyncDictionary<NetworkConnection, int> playerEconomy = new SyncDictionary<NetworkConnection, int>();

    public event Action OnEcoChange;

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

    public SyncDictionary<NetworkConnection, int> PlayerEconomy => playerEconomy;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }

        playerEconomy.OnChange += PlayerEconomy_OnChange;

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

    private void PlayerEconomy_OnChange(SyncDictionaryOperation op, NetworkConnection key, int value, bool asServer) {
        OnEcoChange?.Invoke();
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
        playerEconomy.Remove(obj);
    }

    private void Instance_OnPlayerConnected(NetworkConnection obj) {
        playerEconomy.Add(obj, 0);
    }

    private void Instance_OnGameStart() {
        // On game start, give base money
        Log.LogMsg(LogCategories.SystEconomyManager, "Giving base money");
        foreach (NetworkConnection player in playerEconomy.Keys.ToList()) {
            playerEconomy[player] = baseMoney;
        }
    }
    private void Instance_OnTeamFlipFlop() {
        // On game flip flop, reset money, give base money 
        Log.LogMsg(LogCategories.SystEconomyManager, "flip teams");
        foreach (NetworkConnection player in playerEconomy.Keys.ToList()) {
            playerEconomy[player] = baseMoney;
        }
    }
    private void Instance_OnGameEnd() {
        // On game end, reset money
        Log.LogMsg(LogCategories.SystEconomyManager, "Reseting money");
        foreach (NetworkConnection player in playerEconomy.Keys.ToList()) {
            playerEconomy[player] = 0;
        }
    }

    private void Instance_OnRoundEnd(Teams team) {
        // On round end, give money for wining to winning team
        Log.LogMsg(LogCategories.SystEconomyManager, $"Giving money to teams, winning team:{team}");
        foreach (NetworkConnection player in playerEconomy.Keys.ToList()) {
            if (TeamManager.Instance.playerTeams[player] == team) {
                playerEconomy[player] = Mathf.Clamp(roundWinMoney + playerEconomy[player], 0, maxPlayerMoney);
            } else {
                playerEconomy[player] = Mathf.Clamp(roundLoseMoney + playerEconomy[player], 0, maxPlayerMoney);
            }
        }
    }

    private void Instance_OnPlayerDeath(NetworkConnection pDeader, NetworkConnection pKiller, NetworkConnection pAssister) {
        // On player death give point to dead, Give reward to killer, Give reward to assister
        Log.LogMsg(LogCategories.SystEconomyManager, $"Giving money to killer:{pKiller} assister:{pAssister}");
        if (pKiller != null) {
            if (playerEconomy.ContainsKey(pKiller)) {
                playerEconomy[pKiller] = Mathf.Clamp(playerKillMoney + playerEconomy[pKiller], 0, maxPlayerMoney);
            }
        }

        if (pAssister != null) {
            if (playerEconomy.ContainsKey(pAssister)) {
                playerEconomy[pAssister] = Mathf.Clamp(playerKillAssistMoney + playerEconomy[pAssister], 0, maxPlayerMoney);
            }
        }

    }


    private void Instance_OnObjectiveStarted(Teams team, NetworkConnection conn) {
        // On bomb plant, give reward to planter
        Log.LogMsg(LogCategories.SystEconomyManager, $"Objective start for team:{team} conn:{conn}");
        if (conn == null)  return; 

        if (playerEconomy.ContainsKey(conn)) {
            playerEconomy[conn] += Mathf.Clamp(objectiveStartMoney + playerEconomy[conn], 0, maxPlayerMoney);
        }
    }
    private void Instance_OnObjectiveComplete(Teams team, NetworkConnection conn) {
        // On bomb defuse, give reward to defuser
        Log.LogMsg(LogCategories.SystEconomyManager, $"Objective complete for team:{team} conn:{conn}");
        if (conn == null) return;

        if (TeamManager.Instance.GetTeamFromObjective(Objectives.Defenders) == team) return;

        if (playerEconomy.ContainsKey(conn)) {
            playerEconomy[conn] += Mathf.Clamp(objectiveEndMoney + playerEconomy[conn], 0, maxPlayerMoney);
        }
    }
    [Server]
    public void PerchaseItem(ItemData item, int amount, int cost, NetworkConnection conn) {
        if (playerEconomy[conn] < amount * cost) return;
        PlayerManager.Instance.players[conn].GetComponent<InventorySystem>().AtttemptToAddItem(item, amount, out int remaning);
        playerEconomy[conn] -= (amount - remaning) * cost;
    }
}
