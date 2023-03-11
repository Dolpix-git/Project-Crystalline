using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;
using System;

[Serializable]
public class PlayerWeaponManager : NetworkBehaviour {
    #region Serialized.
    /// <summary>
    /// The prefab to spawn for throwing.
    /// </summary>
    [Header("Grenade")]
    [SerializeField] private GameObject throable;
    /// <summary>
    /// Force to apply when throwing.
    /// </summary>
    [SerializeField] private float force;
    /// <summary>
    /// The spike(bomb) prefab object.
    /// </summary>
    [Header("Spike")]
    [SerializeField] private GameObject spike;
    #endregion
    #region Private.
    /// <summary>
    /// True if the player has a spike item.
    /// </summary>
    [SyncVar]
    private bool hasSpike;
    /// <summary>
    /// How far the sphere cast for interaction is
    /// </summary>
    private float interactionRadius = 2;
    [SyncVar]
    public bool disabled;
    #endregion
    #region Getters Setters.
    /// <summary>
    /// Retrurns true if the player has the spike
    /// </summary>
    [HideInInspector]
    public bool HasSpike { get => hasSpike; set => hasSpike = value; }
    #endregion

    [SerializeField] NadeCommandScriptableObject nadeToAdd;

    [SyncVar]
    private Command inventorySlotOne;


    private void Awake() {
        NadeCommand nade = new();
        nade.Setup(nadeToAdd,this,nadeToAdd.MaxStack);
        inventorySlotOne = nade;



        gameObject.GetComponent<Health>().OnDisabled += PlayerWeaponManager_OnDisabled;
        gameObject.GetComponent<Health>().OnRespawned += PlayerWeaponManager_OnRespawned;
        gameObject.GetComponent<Health>().OnDeath += PlayerWeaponManager_OnDeath;
    }
    private void OnDestroy() {
        gameObject.GetComponent<Health>().OnDisabled += PlayerWeaponManager_OnDisabled;
        gameObject.GetComponent<Health>().OnRespawned += PlayerWeaponManager_OnRespawned;
        gameObject.GetComponent<Health>().OnDeath += PlayerWeaponManager_OnDeath;
    }

    private void PlayerWeaponManager_OnDeath() {
        if (!base.IsServer) { return; }
        disabled = true;
    }

    private void PlayerWeaponManager_OnRespawned() {
        if (!base.IsServer) { return; }
        disabled = false;
    }

    private void PlayerWeaponManager_OnDisabled() {
        if (!base.IsServer) { return; }
        disabled = true;
    }

    private void Update() { // Too Do: Command structure
        if (!IsOwner) { return; }

        if (PlayerInputManager.Instance.GetWeaponsInput()) {
            Activate(PlayerInputManager.Instance.GetCamForward());
        }

        //if (PlayerInputManager.Instance.GetPlantSpike()) {
        //    PlantSpike(transform.position + (transform.forward * 2f));
        //}
        //if (PlayerInputManager.Instance.GetInteraction()) {
        //    StartInteraction();
        //}
    }
    [ServerRpc]
    public void Activate(Vector3 lookVector) {
        inventorySlotOne.Activate(lookVector);
    }
    [Server]
    public GameObject SpawnObject(GameObject nadeObj, Vector3 position, Quaternion rotation) {
        GameObject result = Instantiate(nadeObj, position, rotation);
        base.Spawn(result);
        return result;
    }

    //#region Plant.
    ///// <summary>
    ///// Request a (plant the spike) to the (server).
    ///// </summary>
    ///// <param name="position"></param>
    //private void PlantSpike(Vector3 position) {
    //    if (!base.IsOwner) { return; }
    //    CmdPlantSpike(position);
    //}
    //[ServerRpc]
    //private void CmdPlantSpike(Vector3 position) {
    //    if (disabled) { return; }
    //    if (hasSpike) {
    //        GameObject result = Instantiate(spike, position, Quaternion.identity);
    //        base.Spawn(result);
    //        result.GetComponent<Spike>().Initilised(Owner);
    //        hasSpike = false;
    //    }
    //}
    //#endregion
    //#region Interaction.
    ///// <summary>
    ///// Start sending interaction requests to the server
    ///// </summary>
    //private void StartInteraction() {
    //    if (!base.IsOwner) { return; }
    //    CmdStartInteraction();
    //}
    //[ServerRpc]
    //private void CmdStartInteraction() {
    //    if (disabled) { return; }
    //    Interaction();
    //}
    ///// <summary>
    ///// The interaction method, that does a sphere overlap and finds interactable objects
    ///// </summary>
    //private void Interaction() {
    //    Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);
    //    for (int i = 0; i < hits.Length; i++) {
    //        IInteractable interaction = hits[i].GetComponent<IInteractable>();
    //        if (interaction != null) {
    //            interaction.Interact(Owner);
    //        }
    //    }
    //}
    //#endregion
}