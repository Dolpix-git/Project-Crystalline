using FishNet.Connection;
using FishNet.Object;
using UnityEngine;

public class StickyNade : NetworkBehaviour, IThrowable, IRound{
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
    [Tooltip("The player layer")]
    [SerializeField]
    private LayerMask playerLayer = 0;

    [SerializeField]
    private GameObject explosionPrefab;
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

    private Rigidbody rb;
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

        rb = GetComponent<Rigidbody>();
        rb.AddForce(force);
    }

    private void OnCollisionEnter(Collision collision) {
        if (!base.IsServer) return;
        rb.detectCollisions = false;
        rb.isKinematic = true;
        rb.transform.parent = collision.transform;
        DisableCollider();
    }
    [ObserversRpc]
    private void DisableCollider() {
        rb.detectCollisions = false;
        rb.isKinematic = true;
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
