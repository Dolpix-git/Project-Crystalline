using FishNet.Connection;
using FishNet.Object;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class NameDisplayer : NetworkBehaviour{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private GameObject root;

    private string characters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0987654321";

    public override void OnStartClient(){
        base.OnStartClient();
        SetName();
        PlayerNameTracker.OnNameChange += PlayerNameTracker_OnNameChange;

        if (base.IsOwner){
            _text.enabled = false;
            try {
                if (Steamworks.SteamClient.Name.ToString() != null) {
                    PlayerNameTracker.SetName(Steamworks.SteamClient.Name.ToString());
                    Debug.Log(name);
                }
            } catch (NullReferenceException) {
                string randName = "";
                for (int i = 0; i < 10; i++) {
                    randName += characters[UnityEngine.Random.Range(0, characters.Length)];
                }
                Debug.Log(name);
                PlayerNameTracker.SetName(randName);
            }
        }
    }

    public override void OnStopClient(){
        base.OnStopClient();
        PlayerNameTracker.OnNameChange -= PlayerNameTracker_OnNameChange;
    }

    public override void OnOwnershipClient(NetworkConnection prevOwner)
    {
        base.OnOwnershipClient(prevOwner);
        SetName();
    }


    private void PlayerNameTracker_OnNameChange(NetworkConnection arg1, string arg2)
    {
        if (arg1 != base.Owner)
            return;

        SetName();
    }


    /// <summary>
    /// Sets Text to the name for this objects owner.
    /// </summary>
    private void SetName(){
        string result = null;

        if (base.Owner.IsValid)
            result = PlayerNameTracker.GetPlayerName(base.Owner);

        if (string.IsNullOrEmpty(result))
            result = "Unset";

        _text.text = result;

        if (root == null) return;

        root.name = result;
    }
}
