using FishNet.Connection;
using FishNet.Object;
using System.Collections;
using UnityEngine;

public class FreeRoamGameMode : BaseGameMode {
    #region Private.
    #endregion

    #region Setup and Destroy.
    public override void OnStartServer() {
        base.OnStartServer();
        PlayerEventManager.Instance.OnPlayerDeath += Instance_OnPlayerDeath;
        PlayerEventManager.Instance.OnPlayerConnected += Instance_OnPlayerConnected;
    }

    public override void OnStopServer() {
        base.OnStopServer();
        PlayerEventManager.Instance.OnPlayerDeath -= Instance_OnPlayerDeath;
        PlayerEventManager.Instance.OnPlayerConnected -= Instance_OnPlayerConnected;
    }
    #endregion

    #region Games.
    public override void EndGame() {
        Log.LogMsg(LogCategories.Game, "Game End");
        // Game is no longer in progress
        gameInProgress = false;
        GameEventManager.Instance.InvokeGameEnd();

        // Inform game manager
        Manager.GameHasEnded();
    }
    public override void RestartGame() {
        Log.LogMsg(LogCategories.Game, "Game Restart");
        EndGame();
        StartGame();
    }
    public override void StartGame() {
        Log.LogMsg(LogCategories.Game, "Game Start");
        gameInProgress = true;
        GameEventManager.Instance.InvokeGameStart();
    }
    #endregion

    private void Update() {
        GameManager.Instance.gameTimer.Update(Time.deltaTime);
    }

    #region Events.
    [Server]
    private void Instance_OnPlayerDeath(NetworkConnection arg1, NetworkConnection arg2, NetworkConnection arg3) {
        StartCoroutine(RespawnCoroutine(arg1));
    }
    [Server]
    private void Instance_OnPlayerConnected(NetworkConnection obj) {
        StartCoroutine(RespawnCoroutine(obj));
    }

    [Server]
    private IEnumerator RespawnCoroutine(NetworkConnection obj) {
        yield return new WaitForSeconds(3);
        PlayerManager.Instance.players[obj].GetComponent<PlayerHealth>().Respawn();
    }
    #endregion
}
