using System;

public class ObjectiveManager {
    private static ObjectiveManager _instance;
    public static ObjectiveManager Instance {
        get {
            if (_instance is null) {
                _instance = new ObjectiveManager();
            }
            return _instance;
        }
    }

    public event Action OnAttackerWin;
    public event Action OnDefenderWin;
    public event Action OnPlanted;

    public void AttackerWin() {
        OnAttackerWin?.Invoke();
    }
    public void DefenderWin() {
        OnDefenderWin?.Invoke();
    }
    public void Planted() {
        OnPlanted?.Invoke();
    }
}
