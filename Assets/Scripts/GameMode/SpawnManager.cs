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
    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }
    }
}
