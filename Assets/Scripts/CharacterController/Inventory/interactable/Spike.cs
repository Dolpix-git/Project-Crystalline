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


    private void Start() {
        if (!base.IsServer) { return; }
        Debug.Log("SPIKE PLANTED");
        defuseCounter = defuseTime;
        spikeTimer.OnChange += timeRemaining_OnChange;
        spikeTimer.StartTimer(fuseTime, true);
        Planted();
    }

    private void Update() {
        spikeTimer.Update(Time.deltaTime);
    }


    #region Methods.
    /// <summary>
    /// Should be called every update, and will lower the defuse counter. When it reches 0 it will blow up.
    /// </summary>
    public void Interact() {
        Debug.Log("Interaction");
        defuseCounter -= Time.deltaTime;

        if (defuseCounter <= 0) {
            Debug.Log("The spike has been defused!");
            Defused();
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
            Debug.Log($"The spike has blown up!");
            BlownUp();
        }
    }
    #endregion
    #region Events.
    /// <summary>
    /// Called when spike is planted.
    /// </summary>
    private void Planted() {
        ObjectiveManager.Instance.Planted();
    }
    /// <summary>
    /// Called when spike is defused.
    /// </summary>
    private void Defused() {
        ObjectiveManager.Instance.DefenderWin();
    }
    /// <summary>
    /// Called when spike has blown up.
    /// </summary>
    private void BlownUp() {
        ObjectiveManager.Instance.AttackerWin();
        Destroy(gameObject);
    }
    #endregion
}
