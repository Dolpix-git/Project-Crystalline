using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class ImpactNade : NetworkBehaviour, IThrowable, IRound{
    #region Serialized.
    // Radius of damage.
    [Tooltip("Radius of damage.")]
    [SerializeField]
    private float damageRadius = 5f;
    /// <summary>
    /// Default layer
    /// </summary>
    [Tooltip("The player layer")]
    [SerializeField]
    private LayerMask playerLayer = 0;

    [SerializeField]
    private GameObject explosionPrefab;
    #endregion

    #region Private.
    /// <summary>
    /// The team the grenade is on to prevent frendly fire.
    /// </summary>
    private NetworkConnection ownerConn;
    #endregion

    /// <summary>
    /// Sets up the grenade, called at startup.
    /// </summary>
    /// <param name="pt"></param>
    /// <param name="force"></param>
    public void Initialize(Vector3 force, NetworkConnection conn) {
        ownerConn = conn;

        GetComponent<Rigidbody>().AddForce(force);
    }

    private void OnCollisionEnter(Collision collision) {
        Detonate();
    }
    /// <summary>
    /// Called when detonated.
    /// </summary>
    protected virtual void Detonate() {
        if (base.IsServer) {
            //Trace for players 
            Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius, playerLayer);
            for (int i = 0; i < hits.Length; i++) {
                PlayerHealth h = hits[i].GetComponent<PlayerHealth>();
                if (h != null) {
                    NetworkConnection conn = h.Owner;
                    Log.LogMsg($"Attempting to kill {TeamManager.Instance.playerTeams[conn]}");
                    if (TeamManager.Instance.playerTeams[conn] != TeamManager.Instance.playerTeams[ownerConn]) {
                        int damage = 25;

                        h.RemoveHealth(damage, ownerConn);
                    }
                }
            }

            ObserversSpawnDetonatePrefab();

            if (base.IsServerOnly)
                base.Despawn();
        }
    }
    /// <summary>
    /// Tells clients to spawn the detonate prefab.
    /// </summary>
    [ObserversRpc]
    private void ObserversSpawnDetonatePrefab() {
        Instantiate(explosionPrefab, transform.position, Quaternion.LookRotation(GetComponent<Rigidbody>().velocity, Vector3.up));

        if (base.IsServer)
            base.Despawn();
    }
    /// <summary>
    /// Called when the round has ended.
    /// </summary>
    public void RoundEnded() {
        if (!base.IsServer) { return; }
        base.Despawn();
    }
}
