using FishNet.Object;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CompTeam {
    private List<NetworkObject> players;
    private Team team;
    private string teamName;
    private int points;

    public List<NetworkObject> Players { get => players; set => players = value; }
    public Team Team { get => team; set => team = value; }
    public string TeamName { get => teamName; set => teamName = value; }
    public int Points { get => points; set => points = value; }
    

    public CompTeam(Team side, string teamName) {
        players = new List<NetworkObject>();
        this.team = side;
        this.teamName = teamName;
        points = 0;
    }

    public bool IsTeamWiped() {
        foreach (NetworkObject player in players) {
            if (!player.GetComponent<Health>().IsDead) {
                return false;
            }
        }
        return true;
    }
    public int GetNumAlive() {
        int i = 0;
        foreach (NetworkObject player in players) {
            if (!player.GetComponent<Health>().IsDead) {
                i++;
            }
        }
        return i;
    }

    public void SetTeam(Team team) {
        this.team = team;
        foreach (NetworkObject player in players) {
            player.GetComponent<ITeamable>().SetTeamID(team);
        }
    }

    public void RespawnTeam() {
        foreach (NetworkObject player in players) {
            player.transform.position = SpawnManager.Instance.GetSpawn(team);

            Health nobHealth = player.GetComponent<Health>();
            if (nobHealth.IsDead) {
                nobHealth.Respawned();
            }
        }
    }
    public void RemovePlayerFromTeam(NetworkObject nob) {
        CustomLogger.Log(LogCategories.GameManager, $"Attempting to remove {nob.name}");
        players.Remove(nob);
    }
    public void AddPlayerToTeam(NetworkObject nob) {
        players.Add(nob);
        nob.GetComponent<ITeamable>().SetTeamID(team);
    }
    public void Reset() {
        foreach (NetworkObject player in players) {
            player.GetComponent<ITeamable>().SetTeamID(Team.None);
        }
        players.Clear();
        team = 0;
        teamName = null;
        points = 0;
    }
}
