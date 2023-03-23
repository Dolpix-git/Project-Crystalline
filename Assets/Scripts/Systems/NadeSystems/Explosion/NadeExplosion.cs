using UnityEngine;

public class NadeExplosion : MonoBehaviour, IRound{
    [SerializeField] private float explosionDespawnTime;
    private float explosionSpawnTime;

    private void Start() {
        explosionSpawnTime = Time.time;
    }

    private void Update() {
        if (Time.time - explosionSpawnTime < explosionDespawnTime) return;

        Destroy(gameObject);
    }

    public void RoundEnded() {
        Destroy(gameObject);
    }
}
