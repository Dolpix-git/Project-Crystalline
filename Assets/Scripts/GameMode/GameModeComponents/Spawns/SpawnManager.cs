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

    public Vector3 GetSpawn(Team teamSide) {
        if (teamSide == Team.Attackers) {
            if (attackerSpawn == null) {
                CustomLogger.LogError("No Attacker spawn set!");
                return Vector3.zero;
            } else {
                return attackerSpawn.position;
            }
        } else if (teamSide == Team.Defenders) {
            if (defenderSpawn == null) {
                CustomLogger.LogError("No Defender spawn set!");
                return Vector3.zero;
            } else {
                return defenderSpawn.position;
            }
        } else {
            return Vector3.zero;
        }
    }
}
