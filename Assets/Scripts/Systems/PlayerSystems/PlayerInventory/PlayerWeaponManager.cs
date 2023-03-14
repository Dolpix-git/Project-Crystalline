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

    [SerializeField] private NadeCommandScriptableObject nadeToAdd;
    [SerializeField] private int inventoryLength = 3;
    private int currentInventoryIndex;

    [SyncVar]
    private Command[] toolBelt;


    private void Awake() {
        gameObject.GetComponent<Health>().OnDisabled += PlayerWeaponManager_OnDisabled;
        gameObject.GetComponent<Health>().OnRespawned += PlayerWeaponManager_OnRespawned;
        gameObject.GetComponent<Health>().OnDeath += PlayerWeaponManager_OnDeath;
    }
    private void Start() {
        if (base.IsServer) {
            Debug.Log("Attempting to add an item to the inventory");
            toolBelt = new Command[inventoryLength];
            NadeCommand nade = new();
            nade.Setup(nadeToAdd, this, nadeToAdd.MaxStack, 0);
            toolBelt[0] = nade;
        }
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
        // IF drop nade pressed (Drop nade)

        // IF change inventory key (Change to that inventory slot)

        // IF change inventory up or down (Change the inventory slot up or down)

        //if (PlayerInputManager.Instance.GetPlantSpike()) {
        //    PlantSpike(transform.position + (transform.forward * 2f));
        //}
        //if (PlayerInputManager.Instance.GetInteraction()) {
        //    StartInteraction();
        //}
    }

    [Server]
    public int AddObjectToInventory(Command objectToAdd, int amount) {
        // loop through all the tool belt items
        // if empty slot then add object and then add as much of the amount as possible, subtract the amount we were able to add
        // else if slot is the same as the object we are trying to add, then attempt to add as much of the amount as possible , subtract the amount we were able to add

        // if we ran out of amount to give, early exit with 0
        
        // else if we looped through everything, then return the amount remaining

        //for (int i = 0; i < toolBelt.Length; i++) {
        //    if (toolBelt[i] == null) {
        //        toolBelt[i] = new oftype objectToAdd;
        //        toolBelt[i] = 
        //    }
        //}
        return amount;
    }
    [Server]
    public void RemoveObjectFromInventory(int index) {
        toolBelt[index] = null;
    }
    [ServerRpc]
    public void PreviousInventoryIndex() {
        currentInventoryIndex = Mathf.Clamp(currentInventoryIndex --, 0, toolBelt.Length - 1);
    }
    [ServerRpc]
    public void NextInventoryIndex() {
        currentInventoryIndex = Mathf.Clamp(currentInventoryIndex ++, 0, toolBelt.Length - 1);
    }
    [ServerRpc]
    public void ChangeInventoryIndex(int newIndex) {
        currentInventoryIndex = Mathf.Clamp(newIndex,0, toolBelt.Length - 1);
    }

    [ServerRpc]
    public void Activate(Vector3 lookVector) {
        toolBelt[currentInventoryIndex].Activate(lookVector);
    }
    [ServerRpc]
    public void DropObject(Vector3 lookVector) {
        toolBelt[currentInventoryIndex].Drop(lookVector);
    }

    [Server]
    public GameObject SpawnObject(GameObject obj, Vector3 position, Quaternion rotation) {
        GameObject result = Instantiate(obj, position, rotation);
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