using System;

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
}
