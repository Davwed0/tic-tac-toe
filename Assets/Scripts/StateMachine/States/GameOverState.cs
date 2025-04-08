using UnityEngine;

public class GameOverState : GameState
{
    private PlayerColor winner;

    public GameOverState(GameStateMachine stateMachine, PlayerColor winner) : base(stateMachine)
    {
        this.winner = winner;
    }

    public override void Enter()
    {
        Debug.Log($"Game Over! Player {winner} has won!");
        GameManager.Instance.board.scores[(int)winner].IncrementScore();

        // Start a new game after short delay
        GameManager.Instance.StartCoroutine(GameManager.Instance.StartNewGame(2.0f));
    }
}
