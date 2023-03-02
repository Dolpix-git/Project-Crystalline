using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseGameMode : NetworkBehaviour{
    private GameManager manager;
    protected bool gameInProgress;

    public GameManager Manager { get => manager; set => manager = value; }
    public bool GameInProgress { get => gameInProgress; }

    public abstract void RestartGame();
    public abstract void StartGame();
    public abstract void EndGame();
}
