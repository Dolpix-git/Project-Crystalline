using FishNet.Object;
using FishNet.Object.Synchronizing;
using FishNet.Transporting;
using System;
using UnityEngine;


public class Health : NetworkBehaviour {
    #region Serialized.
    /// <summary>
    /// Health to start with.
    /// </summary>
    [Tooltip("Health to start with.")]
    [SerializeField]
    private int baseHealth = 100;
    #endregion
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
    /// Dispatched when players are disabled. You can reinable them by calling respawn.
    /// </summary>
    public event Action OnDisabled;
    #endregion
    #region Private.
    /// <summary>
    /// Current health.
    /// </summary>
    [SyncVar(Channel = Channel.Unreliable, OnChange = nameof(On_Health))]
    private int currentHealth;
    #endregion
    #region Getters Setters.
    /// <summary>
    /// Maximum amount of health character can currently achieve.
    /// </summary>
    public int MaximumHealth { get { return baseHealth; } }
    /// <summary>
    /// Returns the current health of character.
    /// </summary>
    public int CurrentHealth { get => currentHealth; }
    /// <summary>
    /// Returns true if player is dead.
    /// </summary>
    public bool IsDead { get { return currentHealth <= 0f; } }
    #endregion


    private void Start() {
        if (base.IsServer) { currentHealth = MaximumHealth; }
    }


    #region Methods.
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
        CustomLogger.Log(LogCategories.Health, $"{gameObject.name} Has been respawned!");
        OnRespawned?.Invoke();
        RestoreHealth();

        ObserversRespawned();
    }
    /// <summary>
    /// Sent to clients when character is respawned.
    /// </summary>
    [ObserversRpc(ExcludeServer = true)]
    private void ObserversRespawned() {
        CustomLogger.Log(LogCategories.Health, $"{gameObject.name} Has been respawned!");
        OnRespawned?.Invoke();
    }
    /// <summary>
    /// Removes health.
    /// </summary>
    /// <param name="value">Amount</param>
    public void RemoveHealth(int value) {
        if (!base.IsServer) { return; }
        int oldHealth = currentHealth;
        currentHealth -= value;

        OnHealthChanged?.Invoke(oldHealth, currentHealth, MaximumHealth);

        if (currentHealth <= 0f) {
            OnDeath?.Invoke();

            ObserverDeath();
        }
    }
    /// <summary>
    /// Called when health is depleted.
    /// </summary>
    [ObserversRpc(ExcludeServer = true)]
    public virtual void ObserverDeath() {
        CustomLogger.Log(LogCategories.Health, $"{gameObject.name} Has Died");
        OnDeath?.Invoke();
    }
    /// <summary>
    /// Called when health changes
    /// </summary>
    /// <param name="prev">Previous health</param>
    /// <param name="next">Next health</param>
    /// <param name="asServer">True if is server</param>
    private void On_Health(int prev, int next, bool asServer) {
        CustomLogger.Log(LogCategories.Health, $"Health has changed: {prev} {next} {MaximumHealth}  is Server:{asServer}");
        OnHealthChanged?.Invoke(prev, next, MaximumHealth);
    }

    public void Disable() {
        if (!base.IsServer) { return; }
        CustomLogger.Log($"Observer disable request. Is server?:{base.IsServer}");
        OnDisabled?.Invoke();
        ObserverDisable();
    }
    [ObserversRpc(ExcludeServer = true)]
    public virtual void ObserverDisable() {
        CustomLogger.Log($"Observer disable request. Is server?:{base.IsServer}");
        OnDisabled?.Invoke();
    }
    #endregion

    #region OnGUI
    /// <summary>
    /// Will be removed or relocated for logging
    /// </summary>
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
    #endregion
}
