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

    private List<NetworkObject> defenderPlayers;
    private List<NetworkObject> attackerPlayers;

    private bool timerCompleted;
    private bool defenderObjective, attackerObjective;
    private bool defenderWipe, attackerWipe;

    [SyncVar, HideInInspector]
    private int defendersLeft, attackersLeft;
    [SyncVar, HideInInspector]
    private int defenderPoints, attackerPoints;

    private int round;

    private int maxPoints = 5;
    private int halfTime = 2;
    private float roundTime = 30;

    private bool HoldSpike;



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
        Debug.Log("Game End");

        gameInProgress = false;
        // Log Winner
        if (defenderPoints == 5) {
            Debug.Log("Defenders Win!");
        } else if (attackerPoints == 5) {
            Debug.Log("Attackers Win!");
        }

        // Clean up game
        round = defenderPoints = attackerPoints = 0;
        defenderPlayers = null;
        attackerPlayers = null;

        foreach (NetworkObject player in Manager.Players) {
            if (player != null) {
                player.GetComponent<ITeamable>().SetTeamID(Teams.Solo);
            }
        }

        // Inform game manager
        Manager.GameHasEnded();
    }

    public override void RestartGame() {
        Debug.Log("Game Restart");

        // End Game
        EndGame();

        // Start Game
        StartGame();
    }

    public override void StartGame() {
        Debug.Log("Game Start");
        // Set game to start
        gameInProgress = true;

        defenderPlayers = new List<NetworkObject>();
        attackerPlayers = new List<NetworkObject>();

        // Create Teams!
        CreateTeams();

        // Start rounds
        StartRound();
    }
    #endregion
    #region Rounds.
    private void StartRound() {
        Debug.Log("Round Start");
        // Round is now in progress
        roundInProgress = true;

        attackersLeft = attackerPlayers.Count;
        defendersLeft = defenderPlayers.Count;

        // Respawn Players (dead and non dead)
        RespawnPlayers();

        // Give A player the spike
        GiveSpikeToRandomPlayer();

        // Start the timer
        gameTimer.StartTimer(roundTime, true);
    }
    private void EndRound() {
        Debug.Log("Round End");
        roundInProgress = false;

        // Clean up
        defenderWipe = attackerWipe = timerCompleted = defenderObjective = attackerObjective = HoldSpike = false;
        gameTimer.StopTimer();
        IRound[] roundOb = FindObjectsOfType<MonoBehaviour>().OfType<IRound>().ToArray();
        foreach (var ob in roundOb) {
            ob.RoundEnded();
        }

        // Detect game over condition
        round++;
        if (defenderPoints >= maxPoints) {
            EndGame();
        } else if (attackerPoints >= maxPoints) {
            EndGame();
        }

        // Detect round flipflop
        if (round == halfTime) {
            TeamFlipFlop();
        }

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
            defenderPoints++;
            Debug.Log($" ---------- Defenders won that round and now have: {defenderPoints} points ----------");
            Debug.Log($" Attackers lost that round and have: {attackerPoints} points");

            EndRound();
        } else if (attackerObjective || defenderWipe) {
            attackerPoints++;
            Debug.Log($" ---------- Attackers won that round and now have: {attackerPoints} points ----------");
            Debug.Log($" Defenders lost that round and have: {defenderPoints} points");
            EndRound();
        }
    }


    private void timeRemaining_OnChange(SyncTimerOperation op, float prev, float next, bool asServer) {
        if (op == SyncTimerOperation.Finished) {
            Debug.Log($"The timer has completed!");
            timerCompleted = true;
        }
    }

    public override void AddLateJoiner(NetworkObject nob) {
        Debug.Log("Adding A late joiner");

        // add them to a team.
        // Known issue, players will be able to late join and join a round in progress will prevent with custom spawning event later.
        if (attackerPlayers.Count < defenderPlayers.Count) {
            attackerPlayers.Add(nob);
            nob.GetComponent<ITeamable>().SetTeamID(Teams.Attackers);
            if (HoldSpike) {
                nob.GetComponent<PlayerWeaponManager>().HasSpike = true;
                HoldSpike = false;
            }
        } else {
            defenderPlayers.Add(nob);
            nob.GetComponent<ITeamable>().SetTeamID(Teams.Defenders);
        }
    }

    public override void PlayerDeathUpdate() {
        if (!roundInProgress) { return; }
        Debug.Log("Updateing player death");
        attackersLeft = 0;
        defendersLeft = 0;
        // Check if a team has won by wipe
        if (attackerPlayers.Count >= 1) {
            attackerWipe = true;
            foreach (NetworkObject player in attackerPlayers) {
                if (!player.GetComponent<Health>().IsDead) {
                    attackerWipe = false;
                    attackersLeft++;
                }
            }
        }
        if (defenderPlayers.Count >= 1) {
            defenderWipe = true;
            foreach (NetworkObject player in defenderPlayers) {
                if (!player.GetComponent<Health>().IsDead) {
                    defenderWipe = false;
                    defendersLeft++;
                }
            }
        }

        if (attackerWipe && defenderWipe) {
            Debug.LogWarning("Some how both teams have full wiped in the same tick! Wow!");
            defenderWipe = false;
        }
    }

    private void CreateTeams() {
        Debug.Log("Creating Teams");
        // evenly spread out teams (no limit yet)
        for (int i = 0; i < Manager.Players.Count; i++) {
            if (i % 2 == 0) {
                attackerPlayers.Add(Manager.Players[i]);
                Manager.Players[i].GetComponent<ITeamable>().SetTeamID(Teams.Attackers);
            } else {
                defenderPlayers.Add(Manager.Players[i]);
                Manager.Players[i].GetComponent<ITeamable>().SetTeamID(Teams.Defenders);
            }
        }
    }

    private void TeamFlipFlop() {
        Debug.Log("TeamFlipFlop");
        int pointStore = attackerPoints;
        NetworkObject[] playerStore = attackerPlayers.ToArray();

        attackerPoints = defenderPoints;
        attackerPlayers = defenderPlayers;

        defenderPoints = pointStore;
        defenderPlayers = playerStore.ToList();

        foreach (NetworkObject player in attackerPlayers) {
            player.GetComponent<ITeamable>().SetTeamID(Teams.Attackers);
        }
        foreach (NetworkObject player in defenderPlayers) {
            player.GetComponent<ITeamable>().SetTeamID(Teams.Defenders);
        }
    }
    void RespawnPlayers() {
        Debug.Log("Respawning Players!");

        foreach (NetworkObject nob in attackerPlayers) {
            Health nobHealth = nob.GetComponent<Health>();
            nob.transform.position = new Vector3(5, 0, Random.Range(-10, 10));
            if (nobHealth.IsDead) {
                nobHealth.Respawned();
            }
        }
        foreach (NetworkObject nob in defenderPlayers) {
            Health nobHealth = nob.GetComponent<Health>();
            nob.transform.position = new Vector3(-5, 0, Random.Range(-10, 10));
            if (nobHealth.IsDead) {
                nobHealth.Respawned();
            }
        }
    }

    void GiveSpikeToRandomPlayer() {
        if (attackerPlayers.Count != 0) {
            attackerPlayers[Random.Range(0, attackerPlayers.Count)].GetComponent<PlayerWeaponManager>().HasSpike = true;
        } else {
            HoldSpike = true;
        }
    }





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
        GUI.Label(new Rect(horizontal, 30, width, height), $"Attackers points: {attackerPoints} Defenders points: {defenderPoints}", _style);
        GUI.Label(new Rect(horizontal, 50, width, height), $"Attackers left: {attackersLeft} Defenders left: {defendersLeft}", _style);
    }
}
