using FishNet;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerHud : MonoBehaviour, IPanel{
    [SerializeField] private UIStates[] states;

    private VisualElement root;

    private Label healthText;

    private void Awake() {
        root = GetComponent<UIDocument>().rootVisualElement;
        healthText = root.Q<Label>("Health");

        root.visible = false;

        PlayerEventManager.Instance.OnPlayerClentConnected += Instance_OnPlayerClentConnected;
    }

    private void OnDestroy() {
        PlayerEventManager.Instance.OnPlayerClentConnected -= Instance_OnPlayerClentConnected;
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
