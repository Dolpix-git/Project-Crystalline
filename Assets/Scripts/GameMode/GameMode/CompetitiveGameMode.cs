using FishNet;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CompetitiveGameMode : BaseGameMode {
    private bool roundInProgress;

    [SyncObject]
    private readonly SyncTimer gameTimer = new SyncTimer();

    private bool timerCompleted;
    private bool defenderObjective, attackerObjective;
    private bool defenderWipe, attackerWipe;

    private CompTeam teamOne, teamTwo;
    private CompTeam DefenderTeam {
        get {
            if (teamOne.Team == Team.Defenders) {
                return teamOne;
            }else {
                return teamTwo;
            } 
        }
    }
    private CompTeam AttackerTeam {
        get {
            if (teamOne.Team == Team.Attackers) {
                return teamOne;
            } else {
                return teamTwo;
            }
        }
    }

    private int round;

    private int maxPoints = 5;
    private int halfTime = 2;
    private float roundTime = 30;



    #region Setup and Destroy
    public override void OnStartServer() {
        base.OnStartServer();
        gameTimer.OnChange += timeRemaining_OnChange;
        ObjectiveManager.Instance.OnAttackerWin += ObjectiveManager_OnAttackerWin;
        ObjectiveManager.Instance.OnDefenderWin += ObjectiveManager_OnDefenderWin;
        ObjectiveManager.Instance.OnPlanted += ObjectiveManager_OnPlanted;
    }
    private void ObjectiveManager_OnAttackerWin() {
        if (!base.IsServer) { return; }
        attackerObjective = true;
    }
    private void ObjectiveManager_OnDefenderWin() {
        if (!base.IsServer) { return; }
        defenderObjective = true;
    }
    private void ObjectiveManager_OnPlanted() {
        if (!base.IsServer) { return; }
        Debug.Log("Pause timer!");
        gameTimer.PauseTimer();
    }
    public override void OnStopServer() {
        base.OnStopServer();
        gameTimer.OnChange -= timeRemaining_OnChange;
        ObjectiveManager.Instance.OnAttackerWin -= ObjectiveManager_OnAttackerWin;
        ObjectiveManager.Instance.OnDefenderWin -= ObjectiveManager_OnDefenderWin;
        ObjectiveManager.Instance.OnPlanted -= ObjectiveManager_OnPlanted;
    }
    #endregion
    #region Games.
    public override void EndGame() {
        CustomLogger.Log(LogCategories.Game, "Game End");
        // Game is no longer in progress
        gameInProgress = false;

        // Log Winner
        if (teamOne.Points >= maxPoints) {
            CustomLogger.Log(LogCategories.Game, $"----- {teamOne.TeamName} {teamOne.Team} Have Won the game!-----");
        } else if (teamTwo.Points >= maxPoints) {
            CustomLogger.Log(LogCategories.Game, $"----- {teamTwo.TeamName} {teamTwo.Team} Have Won the game! -----");
        } else {
            CustomLogger.Log(LogCategories.Game, "-----  No One Wins!  -----");
        }

        // Clean up game
        round = 0;
        teamOne.Reset();
        teamTwo.Reset();

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
        // Set game to start
        gameInProgress = true;
        teamOne = new CompTeam(Team.Attackers,"Team One");
        teamTwo = new CompTeam(Team.Defenders,"Team Two");
        // Create Teams!
        CreateTeams();
        // Start rounds
        StartRound();
    }
    #endregion
    #region Rounds.
    private void StartRound() {
        CustomLogger.Log(LogCategories.Round , "Round Start");
        // Round is now in progress
        roundInProgress = true;
        // Respawn Players (dead and non dead)
        RespawnPlayers();
        // Give A player the spike
        GiveSpikeToRandomPlayer();
        // Start the timer
        gameTimer.StartTimer(roundTime, true);
    }
    private void EndRound() {
        CustomLogger.Log(LogCategories.Round , "Round End");
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
        if (teamOne.Points >= maxPoints) {
            EndGame();
            return;
        } else if (teamTwo.Points >= maxPoints) {
            EndGame();
            return;
        }
        // Detect round flipflop
        if (round == halfTime) {
            TeamFlipFlop();
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
            DefenderTeam.Points++;
            CustomLogger.Log(LogCategories.Round , $"Defenders won that round and now have: {DefenderTeam.Points} points");

            EndRound();
        } else if (attackerObjective || defenderWipe) {
            AttackerTeam.Points++;
            CustomLogger.Log(LogCategories.Round , $"Attackers won that round and now have: {AttackerTeam.Points} points");
            EndRound();
        }
    }

    private void timeRemaining_OnChange(SyncTimerOperation op, float prev, float next, bool asServer) {
        if (op == SyncTimerOperation.Finished && asServer) {
            CustomLogger.Log(LogCategories.Round, $"The timer has completed!");
            timerCompleted = true;
        }
    }

    public override void AddLateJoiner(NetworkObject nob) {
        CustomLogger.Log(LogCategories.Game , "Adding A late joiner");

        if (teamOne.Players.Count > teamTwo.Players.Count) {
            teamOne.AddPlayerToTeam(nob);
        } else {
            teamTwo.AddPlayerToTeam(nob);
        }
    }
    public override void PlayerDeathUpdate() {
        if (!roundInProgress) { return; }
        CustomLogger.Log(LogCategories.Game , "Updateing player death");
        attackerWipe = AttackerTeam.IsTeamWiped();
        defenderWipe = DefenderTeam.IsTeamWiped();

        if (attackerWipe && defenderWipe) {
            CustomLogger.LogWarning(LogCategories.Round , "Some how both teams have full wiped in the same tick! Wow!");
            defenderWipe = false;
        }
    }


    #region Methods.
    private void CreateTeams() {
        CustomLogger.Log(LogCategories.Game , "Creating Teams");
        teamOne.SetTeam(Team.Attackers);
        teamTwo.SetTeam(Team.Defenders);

        // evenly spread out teams (no limit yet)
        for (int i = 0; i < Manager.Players.Count; i++) {
            if (i % 2 == 0) {
                teamOne.AddPlayerToTeam(Manager.Players[i]);
            } else {
                teamTwo.AddPlayerToTeam(Manager.Players[i]);
            }
        }
    }
    private void TeamFlipFlop() {
        CustomLogger.Log(LogCategories.Round , "TeamFlipFlop");
        CompTeam newAttackers = DefenderTeam;
        CompTeam newDefenders = AttackerTeam;

        newAttackers.SetTeam(Team.Attackers);
        newDefenders.SetTeam(Team.Defenders);
    }
    private void RespawnPlayers() {
        CustomLogger.Log(LogCategories.Round , "Respawning Players!");

        teamOne.RespawnTeam();
        teamTwo.RespawnTeam();
    }
    private void GiveSpikeToRandomPlayer() {
        CustomLogger.Log(LogCategories.Round, "Giving someone the spike"); 
        if (AttackerTeam.Players.Count != 0) {
            int player = Random.Range(0, AttackerTeam.Players.Count);
            AttackerTeam.Players[player].GetComponent<PlayerWeaponManager>().HasSpike = true;
        } else {
            CustomLogger.Log(LogCategories.Round, "There was no player to give the spike to");
        }
    }
    private void TakeSpikeAwayFromPlayers() {
        CustomLogger.Log(LogCategories.Round , "Taking away spike");
        foreach (NetworkObject player in Manager.Players) {
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
        GUI.Label(new Rect(horizontal, 30, width, height), $"Attackers points: {AttackerTeam.Points} Defenders points: {DefenderTeam.Points}", _style);
        GUI.Label(new Rect(horizontal, 50, width, height), $"Attackers left: {AttackerTeam.GetNumAlive()} Defenders left: {DefenderTeam.GetNumAlive()}", _style);
    }
}
