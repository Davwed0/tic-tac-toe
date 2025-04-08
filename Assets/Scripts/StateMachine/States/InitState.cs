using UnityEngine;

public class InitState : GameState
{
    public InitState(GameStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        Debug.Log("Entering Init State");
        GameManager.Instance.board.InitializeBoard();
        GameManager.Instance.board.InitializePieces();
        GameManager.Instance.board.InitializeSlots();
        GameManager.Instance.board.InitializeScores();

        GameManager.Instance.board.GenerateNewPieces();
        // Transition to PlayerTurnState after initialization
        stateMachine.ChangeState(new PlayerTurnState(stateMachine, PlayerColor.WHITE));
    }
}
