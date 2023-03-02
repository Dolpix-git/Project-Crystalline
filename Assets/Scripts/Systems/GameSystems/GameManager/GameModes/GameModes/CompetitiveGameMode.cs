using FishNet;
using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Linq;
using UnityEngine;

public class CompetitiveGameMode : BaseGameMode {
    #region Private.
    private bool roundInProgress;

    [SyncObject]
    private readonly SyncTimer gameTimer = new SyncTimer();

    private bool timerCompleted;
    private bool defenderObjective, attackerObjective;
    private bool defenderWipe, attackerWipe;



    private int round = 0;

    private int maxPoints = 5;
    private int halfTime = 4;
    private float roundTime = 30;
    #endregion


    #region Setup and Destroy.
    public override void OnStartServer() {
        base.OnStartServer();
        gameTimer.OnChange += timeRemaining_OnChange;
        ObjectiveEventManager.Instance.OnObjectiveComplete += Instance_OnObjectiveComplete; 
        ObjectiveEventManager.Instance.OnObjectiveStarted += Instance_OnObjectiveStarted;
        PlayerEventManager.Instance.OnPlayerConnected += Instance_OnPlayerConnected;
        PlayerEventManager.Instance.OnPlayerDeath += Instance_OnPlayerDeath;
    }
    public override void OnStopServer() {
        base.OnStopServer();
        gameTimer.OnChange -= timeRemaining_OnChange;
        ObjectiveEventManager.Instance.OnObjectiveComplete -= Instance_OnObjectiveComplete;
        ObjectiveEventManager.Instance.OnObjectiveStarted -= Instance_OnObjectiveStarted;
        PlayerEventManager.Instance.OnPlayerConnected -= Instance_OnPlayerConnected;
        PlayerEventManager.Instance.OnPlayerDeath -= Instance_OnPlayerDeath;
    }
    #endregion
    #region Games.
    public override void EndGame() {
        CustomLogger.Log(LogCategories.Game, "Game End");
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
        CustomLogger.Log(LogCategories.Game, "Game Restart");
        EndGame();
        StartGame();
    }
    public override void StartGame() {
        CustomLogger.Log(LogCategories.Game, "Game Start");
        gameInProgress = true;
        GameEventManager.Instance.InvokeGameStart();

        TeamManager.Instance.SetUpTeams();

        StartRound();
    }
    #endregion
    #region Rounds.
    private void StartRound() {
        CustomLogger.Log(LogCategories.Round, "Round Start");
        RoundEventManager.Instance.InvokeRoundStart();
        // Round is now in progress
        roundInProgress = true;
        // Respawn Players (dead and non dead)
        SpawnManager.Instance.RespawnTeams();
        // Give A player the spike
        GiveSpikeToRandomPlayer();
        // Start the timer
        gameTimer.StartTimer(roundTime, true);
    }
    private void EndRound() {
        CustomLogger.Log(LogCategories.Round, "Round End");
        roundInProgress = false;
        // Clean up
        defenderWipe = attackerWipe = timerCompleted = defenderObjective = attackerObjective = false;
        gameTimer.StopTimer();
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
        CustomLogger.Log(LogCategories.Round, $"Round: {round} TeamOne points: {TeamManager.Instance.teamsDict[Teams.TeamOne].points} TeamTwo points: {TeamManager.Instance.teamsDict[Teams.TeamTwo].points}");
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
        gameTimer.Update(Time.deltaTime);

        if (!gameTimer.Paused) {
            timeRemaning = gameTimer.Remaining;
        }

        if (!roundInProgress || !base.IsServer) { return; }

        if (timerCompleted || attackerWipe || defenderObjective) {
            Teams team = TeamManager.Instance.GetTeamFromObjective(Objectives.Defenders);
            RoundEventManager.Instance.InvokeRoundEnd(team);
            CustomLogger.Log(LogCategories.Round, $"Defenders won that round and now have: {TeamManager.Instance.teamsDict[team].points} points");
            
            EndRound();
        } else if (attackerObjective || defenderWipe) {
            Teams team = TeamManager.Instance.GetTeamFromObjective(Objectives.Attackers);
            RoundEventManager.Instance.InvokeRoundEnd(team);
            CustomLogger.Log(LogCategories.Round, $"Attackers won that round and now have: {TeamManager.Instance.teamsDict[team].points} points");
            
            EndRound();
        }
    }

    #region Events.
    private void Instance_OnObjectiveStarted(Teams arg1, NetworkConnection arg2) {
        if (!base.IsServer) { return; }
        CustomLogger.Log(LogCategories.Round, "Timer stoped");
        gameTimer.PauseTimer();
    }

    private void Instance_OnObjectiveComplete(Teams team, NetworkConnection arg2) {
        if (!base.IsServer) { return; }
        if (TeamManager.Instance.teamsDict[team].objective == Objectives.Attackers) {
            CustomLogger.Log(LogCategories.Round, $"Attackers objective complete");
            attackerObjective = true;
        } else if (TeamManager.Instance.teamsDict[team].objective == Objectives.Defenders) {
            CustomLogger.Log(LogCategories.Round, $"Defenders objective complete");
            defenderObjective = true;
        }
    }

    private void timeRemaining_OnChange(SyncTimerOperation op, float prev, float next, bool asServer) {
        if (op == SyncTimerOperation.Finished && asServer) {
            CustomLogger.Log(LogCategories.Round, $"The timer has completed!");
            timerCompleted = true;
        }
    }
    private void Instance_OnPlayerDeath(NetworkConnection arg1, NetworkConnection arg2, NetworkConnection arg3) {
        CustomLogger.Log(LogCategories.Round, "Checking For Wipe");
        attackerWipe = TeamManager.Instance.CheckTeamWipe(TeamManager.Instance.GetTeamFromObjective(Objectives.Attackers));
        defenderWipe = TeamManager.Instance.CheckTeamWipe(TeamManager.Instance.GetTeamFromObjective(Objectives.Defenders));
    }
    private void Instance_OnPlayerConnected(NetworkConnection conn) {
        CustomLogger.Log(LogCategories.Round, $"Adding extra player {conn}");
        TeamManager.Instance.AddLateJoiner(conn, TeamManager.Instance.GetTeamWithLeastMembers());
    }
    #endregion
    #region Methods.
    private void GiveSpikeToRandomPlayer() {
        CustomLogger.Log(LogCategories.Round, "Giving someone the spike");
        Teams attackingTeam = TeamManager.Instance.GetTeamFromObjective(Objectives.Attackers);
        NetworkConnection conn = TeamManager.Instance.GetRandomPlayerFromTeam(attackingTeam);
        if (conn != null) {
            PlayerManager.Instance.players[conn].GetComponent<PlayerWeaponManager>().HasSpike = true;
        }
    }
    private void TakeSpikeAwayFromPlayers() {
        CustomLogger.Log(LogCategories.Round, "Taking away spike");
        foreach (NetworkObject player in PlayerManager.Instance.players.Values) {
            player.GetComponent<PlayerWeaponManager>().HasSpike = false;
        }
    }
    #endregion




    float timeRemaning;

    private GUIStyle _style = new GUIStyle();
    private void OnGUI() {
        //No need to perform these actions on server.
#if !UNITY_EDITOR && UNITY_SERVER
            return;
#endif

        //Only clients can see pings.
        if (!InstanceFinder.IsClient)
            return;

        _style.normal.textColor = Color.white;
        _style.fontSize = 15;
        float width = 85f;
        float height = 15f;
        float edge = 10f;

        float horizontal = (Screen.width * 0.5f) - width - edge;
        float vertical = 10f;

        GUI.Label(new Rect(horizontal, vertical, width, height), $"GameTime: {timeRemaning} ", _style);
        //GUI.Label(new Rect(horizontal, 30, width, height), $"Attackers points: {TeamManager.Instance.teamsDict[TeamManager.Instance.GetTeamFromObjective(Objectives.Attackers)].points} Defenders points: {TeamManager.Instance.teamsDict[TeamManager.Instance.GetTeamFromObjective(Objectives.Defenders)].points}", _style);
        //GUI.Label(new Rect(horizontal, 50, width, height), $"Attackers left: {AttackerTeam.GetNumAlive()} Defenders left: {DefenderTeam.GetNumAlive()}", _style);
    }
}
