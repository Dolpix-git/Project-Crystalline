using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using UnityEngine;


public class Health : NetworkBehaviour {
    #region Public.
    /// <summary>
    /// Dispatched when health changes with old, new, and max health values.
    /// </summary>
    public event Action<int, int, int> OnHealthChanged;
    /// <summary>
    /// Dispatched when health is depleted.
    /// </summary>
    public event Action OnDeath;
    /// <summary>
    /// Dispatched after being respawned.
    /// </summary>
    public event Action OnRespawned;
    /// <summary>
    /// Current health.
    /// </summary>
    [SyncVar(Channel = Channel.Unreliable, OnChange = nameof(On_Health))]
    private int currentHealth;
    /// <summary>
    /// Maximum amount of health character can currently achieve.
    /// </summary>
    public int MaximumHealth { get { return baseHealth; } }

    public int CurrentHealth { get => currentHealth; }
    public bool IsDead { get { return currentHealth <= 0f; } }
    #endregion

    #region Serialized.
    /// <summary>
    /// Health to start with.
    /// </summary>
    [Tooltip("Health to start with.")]
    [SerializeField]
    private int baseHealth = 100;
    #endregion


    private void Awake() {
        if (base.IsServer) { currentHealth = MaximumHealth; }
    }

    /// <summary>
    /// Restores health to maximum health.
    /// </summary>
    public void RestoreHealth() {
        if (!base.IsServer) { return; }
        int oldHealth = currentHealth;
        currentHealth = MaximumHealth;

        OnHealthChanged?.Invoke(oldHealth, currentHealth, MaximumHealth);
    }

    /// <summary>
    /// Called when respawned.
    /// </summary>
    public void Respawned() {
        if (!base.IsServer) { return; }
        OnRespawned?.Invoke();
        RestoreHealth();

        ObserversRespawned();
    }
    /// <summary>
    /// Sent to clients when character is respawned.
    /// </summary>
    [ObserversRpc(ExcludeServer = true)]
    private void ObserversRespawned() {
        if (base.IsServer)
            return;

        OnRespawned?.Invoke();
    }


    /// <summary>
    /// Removes health.
    /// </summary>
    /// <param name="value"></param>
    public void RemoveHealth(int value) {
        if (!base.IsServer) { return; }
        int oldHealth = currentHealth;
        currentHealth -= value;

        OnHealthChanged?.Invoke(oldHealth, currentHealth, MaximumHealth);

        if (currentHealth <= 0f) {
            GameManager.Instance.PlayerHasDied(GetComponent<NetworkObject>());
            OnDeath?.Invoke();

            ObserverDeath();
        }
    }

    /// <summary>
    /// Called when health is depleted.
    /// </summary>
    [ObserversRpc(ExcludeServer = true)]
    public virtual void ObserverDeath() {
        Debug.Log("Player Has Died");
        OnDeath?.Invoke();
    }


    private void On_Health(int prev, int next, bool asServer) {
        OnHealthChanged?.Invoke(prev, next, MaximumHealth);
    }

    private GUIStyle _style = new GUIStyle();

    private void OnGUI() {
        //No need to perform these actions on server.
#if !UNITY_EDITOR && UNITY_SERVER
            return;
#endif

        //Only clients can see pings.
        if (!base.IsOwner)
            return;

        _style.normal.textColor = Color.white;
        _style.fontSize = 15;
        float width = 85f;
        float height = 15f;
        float edge = 10f;

        float horizontal = Screen.width - width - edge;
        float vertical = Screen.height - height - edge - 25;

        GUI.Label(new Rect(horizontal, vertical, width, height), $"Health: {currentHealth}", _style);
    }
}
