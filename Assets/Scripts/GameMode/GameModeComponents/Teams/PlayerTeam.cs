using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

public class PlayerTeam : NetworkBehaviour, ITeamable{

    [SyncVar]
    private Teams teamID;

    public Teams GetTeamID() {
        return teamID;
    }

    public void SetTeamID(Teams teamID) {
        this.teamID = teamID;
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
        float vertical = 70;

        GUI.Label(new Rect(horizontal, vertical, width, height), $"Team: {teamID}", _style);
    }
}
