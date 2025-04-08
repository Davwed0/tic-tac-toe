using UnityEngine;

public class GameStateMachine
{
    public GameState CurrentState { get; private set; }

    public void Initialize(GameState startingState)
    {
        CurrentState = startingState;
        CurrentState.Enter();
    }

    public void ChangeState(GameState newState)
    {
        CurrentState.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update()
    {
        if (CurrentState != null)
        {
            CurrentState.Update();
        }
    }

    public void HandleInput()
    {
        if (CurrentState != null)
        {
            CurrentState.HandleInput();
        }
    }
}
