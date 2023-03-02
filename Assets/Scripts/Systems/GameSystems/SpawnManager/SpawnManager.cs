using FishNet.Connection;
using UnityEngine;

public class SpawnManager : MonoBehaviour{
    private static SpawnManager _instance;
    public static SpawnManager Instance { get {
            if (_instance is null) {
                _instance = FindObjectOfType<SpawnManager>();
                if (_instance is null) {
                    var obj = Instantiate(new GameObject("SpawnManager"));
                    _instance = obj.AddComponent<SpawnManager>();
                }
            }
            return _instance;
        } 
    }

    [SerializeField] private Transform attackerSpawn;
    [SerializeField] private Transform defenderSpawn;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }
    }

    public void RespawnTeams() {
        foreach (NetworkConnection player in PlayerManager.Instance.players.Keys) {
            if (TeamManager.Instance.teamsDict[TeamManager.Instance.playerTeams[player]].objective == Objectives.Defenders) {
                PlayerManager.Instance.players[player].GetComponent<Health>().Respawn();
                PlayerManager.Instance.players[player].transform.position = defenderSpawn.position; 
            } else if (TeamManager.Instance.teamsDict[TeamManager.Instance.playerTeams[player]].objective == Objectives.Attackers) {
                PlayerManager.Instance.players[player].GetComponent<Health>().Respawn();
                PlayerManager.Instance.players[player].transform.position = attackerSpawn.position;
            } else {
                CustomLogger.Log(LogCategories.Round, "A player was not under eather attackers or defenders");
            }
        }
    }
}
