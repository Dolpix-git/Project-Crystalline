using FishNet.Connection;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;


public class Spike : NetworkBehaviour, IInteractable, IRound {
    #region Private.
    [SyncVar]
    private float defuseCounter;
    private float defuseTime = 1f;
    [SyncObject]
    private readonly SyncTimer spikeTimer = new SyncTimer();
    private float fuseTime = 10;
    #endregion


    public void Initilised(NetworkConnection conn) {
        if (!base.IsServer) { return; }
        defuseCounter = defuseTime;
        spikeTimer.OnChange += timeRemaining_OnChange;
        spikeTimer.StartTimer(fuseTime, true);
        Planted(conn);
    }

    private void Update() {
        spikeTimer.Update(Time.deltaTime);
    }


    #region Methods.
    /// <summary>
    /// Should be called every update, and will lower the defuse counter. When it reches 0 it will blow up.
    /// </summary>
    public void Interact(NetworkConnection conn) {
        defuseCounter -= Time.deltaTime;

        if (defuseCounter <= 0) {
            Defused(conn);
        }
    }
    /// <summary>
    /// Called when the round has ended.
    /// </summary>
    public void RoundEnded() {
        if (!base.IsServer) { return; }
        base.Despawn();
    }
    /// <summary>
    /// A function that is called when the timer has Finished.
    /// </summary>
    /// <param name="op">Type of operation</param>
    /// <param name="prev">The previous value</param>
    /// <param name="next">The next value</param>
    /// <param name="asServer">True if as server</param>
    private void timeRemaining_OnChange(SyncTimerOperation op, float prev, float next, bool asServer) {
        if (op == SyncTimerOperation.Finished) {
            BlownUp();
        }
    }
    #endregion
    #region Events.
    /// <summary>
    /// Called when spike is planted.
    /// </summary>
    private void Planted(NetworkConnection conn) {
        ObjectiveEventManager.Instance.InvokeObjectiveStarted(TeamManager.Instance.GetTeamFromObjective(Objectives.Attackers), conn);
    }
    /// <summary>
    /// Called when spike is defused.
    /// </summary>
    private void Defused(NetworkConnection conn) {
        ObjectiveEventManager.Instance.InvokeObjectiveComplete(TeamManager.Instance.GetTeamFromObjective(Objectives.Defenders), conn);
    }
    /// <summary>
    /// Called when spike has blown up.
    /// </summary>
    private void BlownUp() {
        ObjectiveEventManager.Instance.InvokeObjectiveComplete(TeamManager.Instance.GetTeamFromObjective(Objectives.Attackers), null);
        Destroy(gameObject);
    }
    #endregion
}
