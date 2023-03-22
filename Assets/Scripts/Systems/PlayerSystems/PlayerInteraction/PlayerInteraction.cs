using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;

public class PlayerInteraction : NetworkBehaviour{
    [SerializeField] private float interactionRadius;

    private PlayerHealth playerHealth;

    [SyncVar] private bool isInteracting;
    [SyncVar] private bool isInteractionPause;

    public bool IsInteractionPause { get => isInteractionPause; }

    private void Awake() {
        playerHealth = GetComponent<PlayerHealth>();

        PlayerInputManager.Instance.OnIntercationChange += Instance_OnIntercationChange;
    }
    private void OnDestroy() {
        PlayerInputManager.Instance.OnIntercationChange -= Instance_OnIntercationChange;
    }


    private void Update() {
        if (!base.IsServer) return;
        isInteractionPause = false;
        if (playerHealth.IsDisabled || playerHealth.IsDead) return;
        if (isInteracting) Interaction();
    }


    [Client]
    private void Instance_OnIntercationChange(bool obj) {
        if (!IsOwner) { return; }
        if (playerHealth.IsDisabled || playerHealth.IsDead) return;

        CmdSetInteraction(obj);
    }

    [ServerRpc]
    private void CmdSetInteraction(bool val) {
        isInteracting = val;
    }
    /// <summary>
    /// The interaction method, that does a sphere overlap and finds interactable objects
    /// </summary>
    [Server]
    private void Interaction() {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);
        for (int i = 0; i < hits.Length; i++) {
            IInteractable interaction = hits[i].GetComponent<IInteractable>();
            if (interaction != null) {
                interaction.Interact(Owner);
                isInteractionPause = true;
            }
        }
    }
}
