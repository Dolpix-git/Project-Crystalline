using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System.Collections.Generic;
using System.Linq;
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
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }
        PlayerEventManager.Instance.OnPlayerConnected += Instance_OnPlayerConnected;
        PlayerEventManager.Instance.OnPlayerDisconnected += Instance_OnPlayerDisconnected;
        RoundEventManager.Instance.OnRoundEnd += Instance_OnRoundEnd;
    }



    private void OnDestroy() {
        PlayerEventManager.Instance.OnPlayerConnected -= Instance_OnPlayerConnected;
        PlayerEventManager.Instance.OnPlayerDisconnected -= Instance_OnPlayerDisconnected;
        RoundEventManager.Instance.OnRoundEnd -= Instance_OnRoundEnd;
    }

    private void Instance_OnRoundEnd(Teams team) {
        if (!base.IsServer) { return; }
        Log.LogMsg(LogCategories.SystTeamManager, $"Giving a point to {team} client?{base.IsClient} server?{base.IsServer}");
        if (team == Teams.TeamOne || team == Teams.TeamTwo) {
            Log.LogMsg(LogCategories.SystTeamManager, $"Length {teamsDict.Count} Team one:{teamsDict.ContainsKey(Teams.TeamOne)} Team two:{teamsDict.ContainsKey(Teams.TeamTwo)}");
            if (teamsDict.ContainsKey(Teams.TeamOne) && teamsDict.ContainsKey(Teams.TeamTwo)) {

                TeamData teamD = teamsDict[team];
                teamD.points++;
                teamsDict[team] = teamD;
                teamsDict.Dirty(team);
            }
        }
    }
    private void Instance_OnPlayerConnected(NetworkConnection obj) {
        Log.LogMsg(LogCategories.SystTeamManager, $"Adding {obj} to team roster");
        playerTeams.Add(obj, Teams.None);
    }
    private void Instance_OnPlayerDisconnected(NetworkConnection obj) {
        Log.LogMsg(LogCategories.SystTeamManager, $"Removeing {obj} from team roster");
        playerTeams.Remove(obj);
    }

    public void DescoverWhoWon() {
        Teams winningTeam = Teams.None;
        int winningPoints = 0;
        bool tie = false;
        foreach (Teams team in teamsDict.Keys) {
            if (teamsDict[team].points > winningPoints) {
                winningTeam = team;
                winningPoints = teamsDict[team].points;
                tie = false;
            }else if (teamsDict[team].points == winningPoints) {
                tie = true;
            }
        }

        if (tie) {
            Log.LogMsg(LogCategories.SystTeamManager, "There was a Tie!");
        } else if(winningTeam != Teams.None){
            Log.LogMsg(LogCategories.SystTeamManager, $"{winningTeam} Has won the game!");
        } else {
            Log.LogMsg(LogCategories.SystTeamManager, "There was no winner!");
        }
    }

    public void ResetTeams() {
        Log.LogMsg(LogCategories.SystTeamManager, $"Reseting teams");
        foreach (NetworkConnection player in playerTeams.Keys.ToList()) {
            playerTeams[player] = Teams.None;
        }
        teamsDict.Clear();
    }
    [Server]
    public void SetUpTeams() {
        Log.LogMsg(LogCategories.SystTeamManager, $"Setting up teams");
        TeamData teamOne = new TeamData {
            name = "TeamOne",
            objective = Objectives.Attackers
        };

        teamsDict.Add(Teams.TeamOne, teamOne);
        teamsDict.Dirty(Teams.TeamOne);

        TeamData teamTwo = new TeamData {
            name = "TeamTwo",
            objective = Objectives.Defenders
        };

        teamsDict.Add(Teams.TeamTwo, teamTwo);
        teamsDict.Dirty(Teams.TeamTwo);

        Log.LogMsg(LogCategories.SystEventManager, $"Length of teams: {teamsDict.Count}");

        NetworkConnection[] arrayOfPlayers = Shuffle(playerTeams.Keys.ToArray());
        for (int i = 0; i < arrayOfPlayers.Length; i++) {
            if (i % 2 == 0) {
                playerTeams[arrayOfPlayers[i]] = Teams.TeamOne;
            } else {
                playerTeams[arrayOfPlayers[i]] = Teams.TeamTwo;
            }
        }
    }

    public Teams GetTeamFromObjective(Objectives obj) { 
        foreach (Teams team in teamsDict.Keys) {
            if (teamsDict[team].objective == obj) {
                Log.LogMsg(LogCategories.SystTeamManager, $"Getting team from {obj} team:{team}");
                return team;
            }
        }
        Log.LogMsg(LogCategories.SystTeamManager, $"There was no team for Objective:{obj}");
        return Teams.None;
    }

    public NetworkConnection GetRandomPlayerFromTeam(Teams team) {
        
        List<NetworkConnection> list = new List<NetworkConnection>();
        foreach (NetworkConnection player in playerTeams.Keys) {
            if (playerTeams[player] == team) {
                list.Add(player);
            }
        }
        if(list.Count == 0) {
            Log.LogMsg(LogCategories.SystTeamManager, $"No random players in team:{team}");
            return null;
        }
        int index = Random.Range(0, list.Count);
        Log.LogMsg(LogCategories.SystTeamManager, $"Returning random player from team:{team} player:{list[index]}");
        return list[index];
    }

    private NetworkConnection[] Shuffle(NetworkConnection[] a) {
        for (int i = a.Length - 1; i > 0; i--) {
            int rnd = Random.Range(0, i);
            NetworkConnection temp = a[i];

            a[i] = a[rnd];
            a[rnd] = temp;
        }
        return a;
    }

    public void TeamFlipFlop() {
        Log.LogMsg(LogCategories.SystTeamManager, $"Team Flip flop");
        Objectives teamOneObj = teamsDict[Teams.TeamOne].objective;
        Objectives teamTwoObj = teamsDict[Teams.TeamTwo].objective;

        TeamData teamOneData = teamsDict[Teams.TeamOne];
        TeamData teamTwoData = teamsDict[Teams.TeamTwo];

        teamOneData.objective = teamTwoObj;
        teamTwoData.objective = teamOneObj;

        teamsDict[Teams.TeamOne] = teamOneData;
        teamsDict[Teams.TeamTwo] = teamTwoData;

        teamsDict.Dirty(Teams.TeamOne);
        teamsDict.Dirty(Teams.TeamTwo);
    }

    public void AddLateJoiner(NetworkConnection conn, Teams team) {
        playerTeams[conn] = team;
    } 
    public Teams GetTeamWithLeastMembers() {
        int teamOne = GetTeamCount(Teams.TeamOne);
        int teamTwo = GetTeamCount(Teams.TeamTwo);
        if (teamOne < teamTwo) {
            return Teams.TeamOne;
        } else {
            return Teams.TeamTwo;
        }
    }
    public int GetTeamCount(Teams team) {
        int i = 0;
        foreach (NetworkConnection player in playerTeams.Keys) {
            if (playerTeams[player] == team) {
                i++;
            }
        }
        return i;
    }
    public bool CheckTeamWipe(Teams team) {
        foreach (NetworkConnection player in playerTeams.Keys) {
            if (playerTeams[player] == team) {
                if (!PlayerManager.Instance.players[player].GetComponent<Health>().IsDead) {
                    return false;
                }
            }
        }
        return true;
    }
    public int CheckRemainingTeamMembers(Teams team) {
        int count = 0;
        foreach (NetworkConnection player in playerTeams.Keys) {
            if (playerTeams[player] == team) {
                if (!PlayerManager.Instance.players[player].GetComponent<Health>().IsDead) {
                    count++;
                }
            }
        }
        return count;
    }
}
