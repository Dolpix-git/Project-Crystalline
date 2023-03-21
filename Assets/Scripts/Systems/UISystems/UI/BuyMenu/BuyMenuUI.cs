using FishNet;
using UnityEngine;
using UnityEngine.UIElements;

public class BuyMenuUI : MonoBehaviour, IPanel {
    [SerializeField] private UIStates[] states;
    [SerializeField] public ItemData[] itemToBuy;
    [SerializeField] VisualTreeAsset listEntryTemplate;


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

        InitializeCharacterList();
    }

    private void Instance_OnEcoChange() {
        if (EconomyManager.Instance.PlayerEconomy.ContainsKey(InstanceFinder.ClientManager.Connection))
            money.text = $"Money {EconomyManager.Instance.PlayerEconomy[InstanceFinder.ClientManager.Connection]}$";
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



    ListView buyItemList;
    private void InitializeCharacterList() {
        buyItemList = root.Q<ListView>("item-list");

        FillCharacterList();
    }
    void FillCharacterList() {
        // Set up a make item function for a list entry
        buyItemList.makeItem = () => {
            // Instantiate the UXML template for the entry
            var newListEntry = listEntryTemplate.Instantiate();

            // Instantiate a controller for the data
            var newListEntryLogic = new BuyListEntryController();

            // Assign the controller script to the visual element
            newListEntry.userData = newListEntryLogic;

            // Initialize the controller script
            newListEntryLogic.SetVisualElement(newListEntry);

            // Return the root of the instantiated visual tree
            return newListEntry;
        };

        // Set up bind function for a specific list entry
        buyItemList.bindItem = (item, index) => {
            (item.userData as BuyListEntryController).SetItemData(itemToBuy[index], index);
        };

        // Set a fixed item height
        buyItemList.fixedItemHeight = 45;

        // Set the actual item's source list/array
        buyItemList.itemsSource = itemToBuy;
    }
}

public class BuyListEntryController {
    Button buyNadeButton;
    int index;
    public void SetVisualElement(VisualElement visualElement) {
        buyNadeButton = visualElement.Q<Button>("buy-item");

        buyNadeButton.clicked += () => BuyNade(index);
    }
    private void BuyNade(int index) {
        PlayerManager.Instance.players[InstanceFinder.ClientManager.Connection].GetComponent<PlayerHotbarManager>().PerchaseItem(index, 1);
    }
    public void SetItemData(ItemData itemData, int index) {
        buyNadeButton.text = $"{itemData.name} Cost: {itemData.Cost}";
        this.index = index;
    }
}