using System;
using UnityEngine;

public class GameEventManager {
    private static GameEventManager _instance;
    public static GameEventManager Instance {
        get {
            if (_instance is null) {
                _instance = new GameEventManager();
            }
            return _instance;
        }
    }

    public event Action OnGameStart;
    public event Action OnGameEnd;

    public void InvokeGameStart() {
        Log.LogMsg(LogCategories.SystEventManager, "Game Start");
        OnGameStart?.Invoke();
    }
    public void InvokeGameEnd() {
        Log.LogMsg(LogCategories.SystEventManager, "Game End");
        OnGameEnd?.Invoke();
    }
}
