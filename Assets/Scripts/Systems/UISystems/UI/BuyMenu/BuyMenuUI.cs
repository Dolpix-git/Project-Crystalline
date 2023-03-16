using FishNet;
using FishNet.Object;
using UnityEngine;
using UnityEngine.UIElements;

public class BuyMenuUI : MonoBehaviour, IPanel {
    [SerializeField] private UIStates[] states;
    [SerializeField] public ItemData[] itemToBuy;
    private VisualElement root;

    private Label money;

    private static BuyMenuUI _instance;
    public static BuyMenuUI Instance {
        get {
            if (_instance is null) {
                _instance = FindObjectOfType<BuyMenuUI>();
                if (_instance is null) {
                    var obj = Instantiate(new GameObject("BuyMenuUI"));
                    _instance = obj.AddComponent<BuyMenuUI>();
                }
            }
            return _instance;
        }
    }

    private void Awake() {
        if (_instance != null && _instance != this) {
            Destroy(this);
        } else {
            _instance = this;
        }
        root = GetComponent<UIDocument>().rootVisualElement;
        root.visible = false;

        EconomyManager.Instance.OnEcoChange += Instance_OnEcoChange;

        money = root.Q<Label>("Money");
        Button buyNadeButton = root.Q<Button>("BuyNade");
        buyNadeButton.clicked += () => BuyNade();
    }

    private void Instance_OnEcoChange() {
        if (EconomyManager.Instance.PlayerEconomy.ContainsKey(InstanceFinder.ClientManager.Connection))
            money.text = $"Money {EconomyManager.Instance.PlayerEconomy[InstanceFinder.ClientManager.Connection]}$";
    }
    private void BuyNade() {
        PlayerManager.Instance.players[InstanceFinder.ClientManager.Connection].GetComponent<PlayerHotbarManager>().PerchaseItem(0, 1, 5);
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