using UnityEngine;

public abstract class GameState
{
    protected GameStateMachine stateMachine;

    public GameState(GameStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }
    public virtual void HandleInput() { }
}
