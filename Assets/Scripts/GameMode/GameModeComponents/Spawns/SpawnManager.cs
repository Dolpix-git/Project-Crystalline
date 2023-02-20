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

    [SerializeField] private Transform teamOneSpawn;
    [SerializeField] private Transform teamTwoSpawn;

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }
    }

    public Vector3 GetTeamOneSpawn() {
        if (teamOneSpawn == null) {
            CustomLogger.LogError("No TeamOne spawn set!");
            return Vector3.zero;
        }
        return teamOneSpawn.position;
    }
    public Vector3 GetTeamTwoSpawn() {
        if (teamOneSpawn == null) {
            CustomLogger.LogError("No TeamTwo spawn set!");
            return Vector3.zero;
        }
        return teamTwoSpawn.position;
    }
}
