using FishNet;
using FishNet.Managing.Timing;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerWeaponManager : NetworkBehaviour {
    [SerializeField] private GameObject throable;
    [SerializeField] private float force;

    [Header("Spike")]
    [SerializeField] private GameObject spike;

    [SyncVar]
    private bool hasSpike;

    [HideInInspector]
    public bool HasSpike { get => hasSpike; set => hasSpike = value; }
    private float interactionRadius = 2;

    private void Update() {
        if (!base.IsOwner) { return; }

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

    private void Fire(PreciseTick pt, Vector3 position, Vector3 forward) {
        Debug.Log("Fired");

        CmdFireBase(pt, position, forward);
    }

    [ServerRpc]
    private void CmdFireBase(PreciseTick pt, Vector3 position, Vector3 forward) {
        GameObject result = Instantiate(throable, position, Quaternion.identity);
        base.Spawn(result);
        ITeamable teamable = result.GetComponent<ITeamable>();
        teamable.SetTeamID(GetComponent<ITeamable>().GetTeamID()); 
        IThrowable throwable = result.GetComponent<IThrowable>();
        throwable.Initialize(pt, forward * force);
    }

    private void PlantSpike(Vector3 position) {
        Debug.Log("PlantSpike");

        CmdPlantSpike(position);
    }

    [ServerRpc]
    private void CmdPlantSpike(Vector3 position) {
        if (hasSpike) {
            GameObject result = Instantiate(spike, position, Quaternion.identity);
            base.Spawn(result);
            hasSpike = false;
        }
    }

    private void StartInteraction() {
        CmdStartInteraction();
    }    


    [ServerRpc]
    private void CmdStartInteraction() {
        Interaction();
    }

    private void Interaction() {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRadius);
        for (int i = 0; i < hits.Length; i++) {
            IInteractable interaction = hits[i].GetComponent<IInteractable>();
            if (interaction != null) {
                interaction.Interact();
            }
        }
    }

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
}