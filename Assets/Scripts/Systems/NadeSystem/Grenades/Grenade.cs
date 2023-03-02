using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class Grenade : NetworkBehaviour, IThrowable, IRound{
    #region Serialized.
    /// <summary>
    /// How long after spawn to detonate.
    /// </summary>
    [Tooltip("How long after spawn to detonate.")]
    [SerializeField]
    private float detonationDelay = 3f;
    // Radius of damage.
    [Tooltip("Radius of damage.")]
    [SerializeField]
    private float damageRadius = 5f;
    /// <summary>
    /// Default layer
    /// </summary>
    /// <summary>
    /// Default layer
    /// </summary>
    [Tooltip("The player layer")]
    [SerializeField]
    private LayerMask playerLayer = 0;
    #endregion
    #region Private.
    /// <summary>
    /// When to detonate.
    /// </summary>
    private float detonationTime = -1f;
    /// <summary>
    /// The team the grenade is on to prevent frendly fire.
    /// </summary>
    private NetworkConnection ownerConn;
    #endregion


    private void Update() {
        if (!base.IsServer) { return; }
        CheckDetonate();
    }
    /// <summary>
    /// Sets up the grenade, called at startup.
    /// </summary>
    /// <param name="pt"></param>
    /// <param name="force"></param>
    public void Initialize(Vector3 force, NetworkConnection conn) {
        detonationTime = Time.time + detonationDelay;
        ownerConn = conn;

        GetComponent<Rigidbody>().AddForce(force);
    }
    /// <summary>
    /// Checks for detonation.
    /// </summary>
    protected void CheckDetonate() {
        if (detonationTime == -1f)
            return;
        if (Time.time < detonationTime)
            return;

        /* If here then detonate. */
        detonationTime = -1f;

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
                Health h = hits[i].GetComponent<Health>();
                if (h != null) {
                    NetworkConnection conn = h.Owner;
                    CustomLogger.Log($"Attempting to kill {TeamManager.Instance.playerTeams[conn]}");
                    CustomLogger.Log($"Attempting to kill {TeamManager.Instance.playerTeams[ownerConn]}");
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
        //Too Do: add a particle effect to spawn

        //If also client host destroy here.
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
