using FishNet;
using FishNet.Managing;
using FishNet.Transporting;
using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHud : MonoBehaviour{
    [SerializeField] private UIDocument doc;

    private Label teamOneText;
    private Label teamTwoText;
    private Label timerText;
    private Label healthText;

    private void Awake() {
        VisualElement root = doc.rootVisualElement;
        teamOneText = root.Q<Label>("TeamOneScore");
        teamTwoText = root.Q<Label>("TeamTwoScore");
        timerText = root.Q<Label>("Time");
        healthText = root.Q<Label>("Health");



        PlayerEventManager.Instance.OnPlayerClentConnected += Instance_OnPlayerClentConnected;
        PlayerEventManager.Instance.OnPlayerDeath += Instance_OnPlayerDeath;
        RoundEventManager.Instance.OnRoundStart += Instance_OnRoundStart;
        RoundEventManager.Instance.OnRoundEnd += Instance_OnRoundEnd;
    }

    private void OnDestroy() {
        PlayerEventManager.Instance.OnPlayerClentConnected -= Instance_OnPlayerClentConnected;
        PlayerEventManager.Instance.OnPlayerDeath -= Instance_OnPlayerDeath;
        RoundEventManager.Instance.OnRoundEnd -= Instance_OnRoundEnd;
    }

    private void Update() {
        var ts = TimeSpan.FromSeconds(GameManager.Instance.gameTimer.Remaining);
        timerText.text = string.Format("{0:00}:{1:00}", ts.TotalMinutes, ts.Seconds);
    }


    private void Instance_OnPlayerClentConnected(FishNet.Object.NetworkObject obj) {
        if (obj.Owner != InstanceFinder.ClientManager.Connection) return;

        if (PlayerManager.Instance.players.ContainsKey(InstanceFinder.ClientManager.Connection)) {
            PlayerManager.Instance.players[InstanceFinder.ClientManager.Connection].GetComponent<PlayerHealth>().OnHealthChanged += PlayerHud_OnHealthChanged;
        } else {
            Log.LogWarning($"Tried to get a player and they were not in the list of players Conn:{InstanceFinder.ClientManager.Connection}");
        }
    }
    private void PlayerHud_OnHealthChanged(int arg1, int arg2, int arg3) {
        healthText.text = "Health: " + arg2.ToString();
    }
    private void Instance_OnPlayerDeath(FishNet.Connection.NetworkConnection arg1, FishNet.Connection.NetworkConnection arg2, FishNet.Connection.NetworkConnection arg3) {
        UpdateTeamInfo();
    }
    private void Instance_OnRoundStart() {
        UpdateTeamInfo();
    }
    private void Instance_OnRoundEnd(Teams obj) {
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
}
