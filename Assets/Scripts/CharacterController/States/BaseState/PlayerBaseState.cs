
using System;
using System.Net;
using Unity.Profiling.LowLevel;
using UnityEngine;

public abstract class PlayerBaseState{
    private bool isRootState = false;
    private PlayerStateMachine ctx;
    private PlayerStateCashe cashe;
    private PlayerBaseState currentSubState;
    private PlayerBaseState currentSuperState;

    protected bool IsRootState { set { isRootState = value; } }
    protected PlayerStateMachine Ctx { get { return ctx; } }
    protected PlayerStateCashe Cashe { get { return cashe; } }

    public PlayerBaseState(PlayerStateMachine currentContext, PlayerStateCashe playerStateFactory){
        ctx = currentContext;
        cashe = playerStateFactory;
    }

    public abstract void EnterState();
    public abstract void UpdateState(); 
    public abstract void ExitState();
    public abstract void InitiatizeSubState();
    public abstract void CheckSwitchStates(); // NOTE: Must be at the bottom of update state



    public void UpdateStates(){ 
        UpdateState();
        if (currentSubState is not null){
            currentSubState.UpdateStates();
        }
    }
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
}
