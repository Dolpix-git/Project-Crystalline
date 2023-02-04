
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PlayerStates{
    idle,
    walk,
    run,
    jump,
    grounded,
    falling,
}

public class PlayerStateCashe{
    PlayerStateMachine context;
    Dictionary<PlayerStates, PlayerBaseState> states = new Dictionary<PlayerStates, PlayerBaseState>();
    public PlayerStateCashe(PlayerStateMachine currentContext){
        context = currentContext;
        states[PlayerStates.idle] = new PlayerIdleState(context, this);
        states[PlayerStates.walk] = new PlayerWalkState(context, this);
        states[PlayerStates.run] = new PlayerRunState(context, this);
        states[PlayerStates.jump] = new PlayerJumpState(context, this);
        states[PlayerStates.grounded] = new PlayerGroundedState(context, this);
        states[PlayerStates.falling] = new PlayerFallingState(context, this);
    }

    public PlayerBaseState Idle() {
        return states[PlayerStates.idle];
    }

    public PlayerBaseState Walk(){
        return states[PlayerStates.walk];
    }

    public PlayerBaseState Run() {
        return states[PlayerStates.run];
    }
        
    public PlayerBaseState Jump(){
        return states[PlayerStates.jump];
    }
    
    public PlayerBaseState Grounded(){
        return states[PlayerStates.grounded];
    }

    public PlayerBaseState Falling(){
        return states[PlayerStates.falling];
    }
}
