using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;


public class Spike : NetworkBehaviour, IInteractable, IRound {
    [SyncVar]
    private float defuseCounter;
    private float defuseTime = 1f;
    [SyncObject]
    private readonly SyncTimer spikeTimer = new SyncTimer();
    private float fuseTime = 10;

    private void Start() {
        if (!base.IsServer) { return; }
        Debug.Log("SPIKE PLANTED");
        defuseCounter = defuseTime;
        spikeTimer.OnChange += timeRemaining_OnChange;
        spikeTimer.StartTimer(fuseTime, true);
        Planted();
    }

    public void Interact() {
        Debug.Log("Interaction");
        defuseCounter -= Time.deltaTime;

        if (defuseCounter <= 0) {
            Debug.Log("The spike has been defused!");
            Defused();
        }
    }

    private void Update() {
        spikeTimer.Update(Time.deltaTime);
    }

    private void timeRemaining_OnChange(SyncTimerOperation op, float prev, float next, bool asServer) {
        if (op == SyncTimerOperation.Finished) {
            Debug.Log($"The spike has blown up!");
            BlownUp();
        }
    }
    private void Planted() {
        ObjectiveManager.Instance.Planted();
    }
    private void Defused() {
        ObjectiveManager.Instance.DefenderWin();
    }
    private void BlownUp() {
        ObjectiveManager.Instance.AttackerWin();
        Destroy(gameObject);
    }

    public void RoundEnded() {
        Destroy(gameObject);
    }
}
