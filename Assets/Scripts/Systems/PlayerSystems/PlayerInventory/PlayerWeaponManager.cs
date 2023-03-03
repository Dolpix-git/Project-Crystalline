using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

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
    private bool disabled;
    #endregion
    #region Getters Setters.
    /// <summary>
    /// Retrurns true if the player has the spike
    /// </summary>
    [HideInInspector]
    public bool HasSpike { get => hasSpike; set => hasSpike = value; }
    #endregion

    private void Awake() {
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

    private void Update() {
        if (PlayerInputManager.Instance.GetWeaponsInput()) {
            Fire(base.TimeManager.GetPreciseTick(TickType.Tick), transform.position + (transform.forward * 2f), PlayerInputManager.Instance.GetCamForward() + Vector3.up);
        }
        if (PlayerInputManager.Instance.GetPlantSpike()) {
            PlantSpike(transform.position + (transform.forward * 2f));
        }
        if (PlayerInputManager.Instance.GetInteraction()) {
            StartInteraction();
        }
    }


    #region Fire.
    /// <summary>
    /// Request a fire(grendade throw) to the (server).
    /// </summary>
    /// <param name="pt">Persice tick</param>
    /// <param name="position">Position to spawn</param>
    /// <param name="forward">Forward vector</param>
    private void Fire(PreciseTick pt, Vector3 position, Vector3 forward) {
        if (!base.IsOwner) { return; }
        CmdFireBase(pt, position, forward);
    }
    [ServerRpc]
    private void CmdFireBase(PreciseTick pt, Vector3 position, Vector3 forward) {
        if (disabled) { return; }
        GameObject result = Instantiate(throable, position, Quaternion.identity);
        base.Spawn(result);
        IThrowable throwable = result.GetComponent<IThrowable>();
        throwable.Initialize(forward * force, Owner);
    }
    #endregion
    #region Plant.
    /// <summary>
    /// Request a (plant the spike) to the (server).
    /// </summary>
    /// <param name="position"></param>
    private void PlantSpike(Vector3 position) {
        if (!base.IsOwner) { return; }
        CmdPlantSpike(position);
    }
    [ServerRpc]
    private void CmdPlantSpike(Vector3 position) {
        if (disabled) { return; }
        if (hasSpike) {
            GameObject result = Instantiate(spike, position, Quaternion.identity);
            base.Spawn(result);
            result.GetComponent<Spike>().Initilised(Owner);
            hasSpike = false;
        }
    }
    #endregion
    #region Interaction.
    /// <summary>
    /// Start sending interaction requests to the server
    /// </summary>
    private void StartInteraction() {
        if (!base.IsOwner) { return; }
        CmdStartInteraction();
    }
    [ServerRpc]
    private void CmdStartInteraction() {
        if (disabled) { return; }
        Interaction();
    }
    /// <summary>
    /// The interaction method, that does a sphere overlap and finds interactable objects
    /// </summary>
    private void Interaction() {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);
        for (int i = 0; i < hits.Length; i++) {
            IInteractable interaction = hits[i].GetComponent<IInteractable>();
            if (interaction != null) {
                interaction.Interact(Owner);
            }
        }
    }
    #endregion



    #region OnGUI.
    /// <summary>
    /// Will be removed or relocated for logging
    /// </summary>
    private GUIStyle _style = new GUIStyle();
    private void OnGUI() {
        //No need to perform these actions on server.
#if !UNITY_EDITOR && UNITY_SERVER
            return;
#endif

        //Only clients can see pings.
        if (!base.IsOwner)
            return;

        _style.normal.textColor = Color.white;
        _style.fontSize = 15;
        float width = 85f;
        float height = 15f;
        float edge = 10f;

        float horizontal = (Screen.width * 0.5f) - width - edge;

        GUI.Label(new Rect(horizontal, 90, width, height), $"Spike: {hasSpike}", _style);
    }
    #endregion
}