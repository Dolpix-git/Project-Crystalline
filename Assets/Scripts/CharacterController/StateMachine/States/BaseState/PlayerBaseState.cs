public abstract class PlayerBaseState{
    #region Private.
    private bool isRootState = false;
    private PlayerStateMachine ctx;
    private PlayerStateCashe cashe;
    private PlayerBaseState currentSubState;
    private PlayerBaseState currentSuperState;
    #endregion
    #region Protected.
    protected bool IsRootState { set { isRootState = value; } }
    protected PlayerStateMachine Ctx { get { return ctx; } }
    protected PlayerStateCashe Cashe { get { return cashe; } }
    #endregion
    #region Public.
    public PlayerBaseState CurrentSubState { get => currentSubState; set => currentSubState = value; }
    #endregion

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory){
        ctx = currentContext;
        cashe = playerStateFactory;
    }

    #region States.
    public abstract void EnterState();
    public abstract void UpdateState(); 
    public abstract void ExitState();
    public abstract void InitiatizeSubState();
    public abstract void CheckSwitchStates(); // NOTE: Must be at the bottom of update state
    /// <summary>
    /// Call for getting this state/
    /// </summary>
    /// <returns>Player state</returns>
    public abstract PlayerStates PlayerState();
    #endregion
    #region Methods.
    /// <summary>
    /// Updates this state and all child states.
    /// </summary>
    public void UpdateStates(){ 
        UpdateState();
        if (currentSubState is not null){
            currentSubState.UpdateStates();
        }
    }
    /// <summary>
    /// Switches out the state for another state.
    /// </summary>
    /// <param name="newState">The state you want to switch to</param>
    protected void SwitchState(PlayerBaseState newState) { 
        // current state exits state
        ExitState();

        // enter new state
        newState.EnterState();

        if (isRootState){
            // switch current state of context
            ctx.CurrentState = newState;
        } else if (currentSuperState != null){
            currentSuperState.SetSubState(newState);
        }
    }
    protected void SetSuperState(PlayerBaseState newSuperState) {
        currentSuperState = newSuperState;
    }
    protected void SetSubState(PlayerBaseState newSubState) {
        currentSubState = newSubState;
        newSubState.SetSuperState(this);
    }
    /// <summary>
    /// Used when the player is reconsiling and needs to return to a previous state with no transitions or questions.
    /// </summary>
    /// <param name="playerStates"></param>
    public void SetSubStateReconsile(PlayerStates playerStates) {
        SetSubState(Cashe.GetState(playerStates));
    }
    #endregion
}
