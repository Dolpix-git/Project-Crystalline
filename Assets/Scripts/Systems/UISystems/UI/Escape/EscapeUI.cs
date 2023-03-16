using UnityEngine;
using UnityEngine.UIElements;

public class EscapeUI : MonoBehaviour,IPanel{
    [SerializeField] private UIStates[] states;
    private VisualElement root;
    private void Awake() {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.visible = false;
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
