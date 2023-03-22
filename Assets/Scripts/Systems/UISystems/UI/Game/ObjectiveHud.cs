using System;
using UnityEngine;
using UnityEngine.UIElements;

public class ObjectiveHud : MonoBehaviour, IPanel {
    [SerializeField] private UIStates[] states;

    private VisualElement root;

    private Label teamOneText;
    private Label teamTwoText;
    private Label timerText;

    private void Awake() {
        root = GetComponent<UIDocument>().rootVisualElement;
        teamOneText = root.Q<Label>("TeamOneScore");
        teamTwoText = root.Q<Label>("TeamTwoScore");
        timerText = root.Q<Label>("Time");

        root.visible = false;

        PlayerEventManager.Instance.OnPlayerDeath += Instance_OnPlayerDeath;
        TeamManager.Instance.OnTeamsChanged += Instance_OnTeamsChanged;
    }



    private void OnDestroy() {
        PlayerEventManager.Instance.OnPlayerDeath -= Instance_OnPlayerDeath;
        TeamManager.Instance.OnTeamsChanged -= Instance_OnTeamsChanged;
    }

    private void Update() {
        var ts = TimeSpan.FromSeconds(GameManager.Instance.gameTimer.Remaining);
        timerText.text = string.Format("{0:00}:{1:00}", ts.TotalMinutes, ts.Seconds);
    }

    private void Instance_OnPlayerDeath(FishNet.Connection.NetworkConnection arg1, FishNet.Connection.NetworkConnection arg2, FishNet.Connection.NetworkConnection arg3) {
        UpdateTeamInfo();
    }
    private void Instance_OnTeamsChanged() {
        UpdateTeamInfo();
    }

    private void UpdateTeamInfo() {
        if (TeamManager.Instance.teamsDict.ContainsKey(Teams.TeamOne)) {
            teamOneText.text = $" {TeamManager.Instance.teamsDict[Teams.TeamOne].name}\n " +
                $"Points:{TeamManager.Instance.teamsDict[Teams.TeamOne].points}\n " +
                $"Remaining:{TeamManager.Instance.CheckRemainingTeamMembers(Teams.TeamOne)}\n " +
                $"Objective:{TeamManager.Instance.teamsDict[Teams.TeamOne].objective}";
        }
        if (TeamManager.Instance.teamsDict.ContainsKey(Teams.TeamTwo)) {
            teamTwoText.text = $" {TeamManager.Instance.teamsDict[Teams.TeamTwo].name}\n " +
                $"Points:{TeamManager.Instance.teamsDict[Teams.TeamTwo].points}\n " +
                $"Remaining:{TeamManager.Instance.CheckRemainingTeamMembers(Teams.TeamTwo)}\n " +
                $"Objective:{TeamManager.Instance.teamsDict[Teams.TeamTwo].objective}";
        }

    }

    public bool HasState(UIStates state) {
        for (int i = 0; i < states.Length; i++) {
            if (states[i] == state) return true;
        }
        return false;
    }
    public void SetVisible(bool val) {
        root.visible = val;
    }
}
