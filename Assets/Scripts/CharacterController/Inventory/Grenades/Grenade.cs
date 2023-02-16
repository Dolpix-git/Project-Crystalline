using FishNet.Managing.Timing;
using FishNet.Object;
using UnityEngine;

public class Grenade : NetworkBehaviour, IThrowable, ITeamable, IRound{
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
    #endregion
    public const float MAXIMUM_LATENCY_COMPENSATION = 20f;

    public override void OnStartServer() {
        base.OnStartServer();
        base.TimeManager.OnTick += TimeManager_OnTick;
    }

    public override void OnStopServer() {
        base.OnStopServer();
        base.TimeManager.OnTick -= TimeManager_OnTick;
    }

    protected virtual void Update() {
        PerformUpdate(false);
    }

    private void TimeManager_OnTick() {
        PerformUpdate(true);
    }

    private void PerformUpdate(bool onTick) {
        /* If a tick but also host then do not
         * update. The update will occur outside of
         * OnTick, using the update loop. */
        if (onTick && base.IsHost)
            return;
        /* If not called from OnTick and is server
         * only then exit. OnTick will handle movements. */
        else if (!onTick && base.IsServerOnly)
            return;

        CheckDetonate();
    }

    public void Initialize(PreciseTick pt, Vector3 force) {
        detonationTime = Time.time + detonationDelay;

        GetComponent<Rigidbody>().AddForce(force);
    }

    protected void CheckDetonate() {
        if (detonationTime == -1f)
            return;
        if (Time.time < detonationTime)
            return;

        /* If here then detonate. */
        detonationTime = -1f;

        Detonate();
    }

    protected virtual void Detonate() {
        if (base.IsServer) {
            //Trace for players 
            Collider[] hits = Physics.OverlapSphere(transform.position, damageRadius, playerLayer);
            for (int i = 0; i < hits.Length; i++) {
                Health h = hits[i].GetComponent<Health>();
                if (h != null && (h.GetComponent<ITeamable>().GetTeamID() != teamID || teamID == Teams.Solo)) {
                    int damage = 25;

                    h.RemoveHealth(damage);
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


    private Teams teamID;

    public Teams GetTeamID() {
        return teamID;
    }
    public void SetTeamID(Teams teamID) {
        this.teamID = teamID;
    }

    public void RoundEnded() {
        if (!base.IsServer) { return; }
        base.Despawn();
    }
}
