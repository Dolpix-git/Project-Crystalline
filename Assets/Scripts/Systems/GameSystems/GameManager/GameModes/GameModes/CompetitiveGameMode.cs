using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Linq;
using UnityEngine;

public class CompetitiveGameMode : BaseGameMode {
    [SerializeField] private ItemData spikePrefab;
    #region Private.
    private bool roundInProgress;



    private bool timerCompleted;
    private bool defenderObjective, attackerObjective;
    private bool defenderWipe, attackerWipe;



    private int round = 0;

    private int maxPoints = 5;
    private int halfTime = 4;
    [SerializeField] private float roundTime = 30;
    #endregion

    #region Setup and Destroy.
    public override void OnStartServer() {
        base.OnStartServer();
        GameManager.Instance.gameTimer.OnChange += timeRemaining_OnChange;
        ObjectiveEventManager.Instance.OnObjectiveComplete += Instance_OnObjectiveComplete; 
        ObjectiveEventManager.Instance.OnObjectiveStarted += Instance_OnObjectiveStarted;
        PlayerEventManager.Instance.OnPlayerConnected += Instance_OnPlayerConnected;
        PlayerEventManager.Instance.OnPlayerDeath += Instance_OnPlayerDeath;
    }
    public override void OnStopServer() {
        base.OnStopServer();
        GameManager.Instance.gameTimer.OnChange -= timeRemaining_OnChange;
        ObjectiveEventManager.Instance.OnObjectiveComplete -= Instance_OnObjectiveComplete;
        ObjectiveEventManager.Instance.OnObjectiveStarted -= Instance_OnObjectiveStarted;
        PlayerEventManager.Instance.OnPlayerConnected -= Instance_OnPlayerConnected;
        PlayerEventManager.Instance.OnPlayerDeath -= Instance_OnPlayerDeath;
    }
    #endregion

    #region Games.
    public override void EndGame() {
        Log.LogMsg(LogCategories.Game, "Game End");
        // Game is no longer in progress
        gameInProgress = false;
        GameEventManager.Instance.InvokeGameEnd();
        // Log Winner
        TeamManager.Instance.DescoverWhoWon();

        // Clean up game
        round = 0;
        TeamManager.Instance.ResetTeams();

        // Inform game manager
        Manager.GameHasEnded();
    }
    public override void RestartGame() {
        Log.LogMsg(LogCategories.Game, "Game Restart");
        EndGame();
        StartGame();
    }
    public override void StartGame() {
        Log.LogMsg(LogCategories.Game, "Game Start");
        gameInProgress = true;
        GameEventManager.Instance.InvokeGameStart();

        TeamManager.Instance.SetUpTeams();

        StartRound();
    }
    #endregion

    #region Rounds.
    [Server]
    private void StartRound() {
        Log.LogMsg(LogCategories.Round, "Round Start");
        RoundEventManager.Instance.InvokeRoundStart();
        // Round is now in progress
        roundInProgress = true;
        // Respawn Players (dead and non dead)
        SpawnManager.Instance.RespawnTeams();
        // Give A player the spike
        GiveSpikeToRandomPlayer();
        // Start the timer
        GameManager.Instance.gameTimer.StartTimer(roundTime, true);
    }
    [Server]
    private void EndRound() {
        Log.LogMsg(LogCategories.Round, "Round End");
        roundInProgress = false;
        // Clean up
        defenderWipe = attackerWipe = timerCompleted = defenderObjective = attackerObjective = false;
        GameManager.Instance.gameTimer.StopTimer();
        IRound[] roundOb = FindObjectsOfType<MonoBehaviour>().OfType<IRound>().ToArray();
        foreach (var ob in roundOb) {
            ob.RoundEnded();
        }
        // Detect game over condition
        round++;
        if (TeamManager.Instance.teamsDict[Teams.TeamOne].points >= maxPoints) {
            EndGame();
            return;
        } else if (TeamManager.Instance.teamsDict[Teams.TeamTwo].points >= maxPoints) {
            EndGame();
            return;
        }
        Log.LogMsg(LogCategories.Round, $"Round: {round} TeamOne points: {TeamManager.Instance.teamsDict[Teams.TeamOne].points} TeamTwo points: {TeamManager.Instance.teamsDict[Teams.TeamTwo].points}");
        // Detect round flipflop
        if (round == halfTime) {
            TeamManager.Instance.TeamFlipFlop();
            RoundEventManager.Instance.InvokeTeamFlipFlop();
        }
        // Take Spike away
        TakeSpikeAwayFromPlayers();
        // Start new round
        StartRound();
    }
    #endregion

    private void Update() {
        GameManager.Instance.gameTimer.Update(Time.deltaTime);

        if (!roundInProgress || !base.IsServer) { return; }

        if (timerCompleted || attackerWipe || defenderObjective) {
            Teams team = TeamManager.Instance.GetTeamFromObjective(Objectives.Defenders);
            RoundEventManager.Instance.InvokeRoundEnd(team);
            Log.LogMsg(LogCategories.Round, $"Defenders won that round and now have: {TeamManager.Instance.teamsDict[team].points} points");
            
            EndRound();
        } else if (attackerObjective || defenderWipe) {
            Teams team = TeamManager.Instance.GetTeamFromObjective(Objectives.Attackers);
            RoundEventManager.Instance.InvokeRoundEnd(team);
            Log.LogMsg(LogCategories.Round, $"Attackers won that round and now have: {TeamManager.Instance.teamsDict[team].points} points");
            
            EndRound();
        }
    }

    #region Events.
    private void Instance_OnObjectiveStarted(Teams arg1, NetworkConnection arg2) {
        if (!base.IsServer) { return; }
        Log.LogMsg(LogCategories.Round, "Timer stoped");
        GameManager.Instance.gameTimer.PauseTimer();
    }

    private void Instance_OnObjectiveComplete(Teams team, NetworkConnection arg2) {
        if (!base.IsServer) { return; }
        if (TeamManager.Instance.teamsDict[team].objective == Objectives.Attackers) {
            Log.LogMsg(LogCategories.Round, $"Attackers objective complete");
            attackerObjective = true;
        } else if (TeamManager.Instance.teamsDict[team].objective == Objectives.Defenders) {
            Log.LogMsg(LogCategories.Round, $"Defenders objective complete");
            defenderObjective = true;
        }
    }

    private void timeRemaining_OnChange(SyncTimerOperation op, float prev, float next, bool asServer) {
        if (op == SyncTimerOperation.Finished && asServer) {
            Log.LogMsg(LogCategories.Round, $"The timer has completed!");
            timerCompleted = true;
        }
    }
    private void Instance_OnPlayerDeath(NetworkConnection arg1, NetworkConnection arg2, NetworkConnection arg3) {
        Log.LogMsg(LogCategories.Round, "Checking For Wipe");
        attackerWipe = TeamManager.Instance.CheckTeamWipe(TeamManager.Instance.GetTeamFromObjective(Objectives.Attackers));
        defenderWipe = TeamManager.Instance.CheckTeamWipe(TeamManager.Instance.GetTeamFromObjective(Objectives.Defenders));
    }
    private void Instance_OnPlayerConnected(NetworkConnection conn) {
        Log.LogMsg(LogCategories.Round, $"Adding extra player {conn}");
        TeamManager.Instance.AddLateJoiner(conn, TeamManager.Instance.GetTeamWithLeastMembers());
    }
    #endregion

    #region Methods.
    private void GiveSpikeToRandomPlayer() {
        Log.LogMsg(LogCategories.Round, "Giving someone the spike");
        Teams attackingTeam = TeamManager.Instance.GetTeamFromObjective(Objectives.Attackers);
        NetworkConnection conn = TeamManager.Instance.GetRandomPlayerFromTeam(attackingTeam);
        if (conn != null) {
            InventorySystem inventorySystem = PlayerManager.Instance.players[conn].GetComponent<InventorySystem>();
            if (inventorySystem != null)
                inventorySystem.AtttemptToAddItem(spikePrefab,1,out int rem);
        }
    }
    private void TakeSpikeAwayFromPlayers() {
        Log.LogMsg(LogCategories.Round, $"Taking away spike");
        foreach (NetworkObject player in PlayerManager.Instance.players.Values) {
            InventorySystem inventorySystem = player.GetComponent<InventorySystem>();
            if (inventorySystem != null)
                inventorySystem.WipeItemFromInventory(spikePrefab);
        }
    }
    #endregion
}
