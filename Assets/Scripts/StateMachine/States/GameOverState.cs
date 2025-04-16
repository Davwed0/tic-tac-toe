using UnityEngine;
using System.Collections.Generic;

public class GameOverState : GameState
{
    private PlayerColor winner;
    private List<Vector2Int> winningPositions;

    public GameOverState(GameStateMachine stateMachine, PlayerColor winner, List<Vector2Int> winningPositions) : base(stateMachine)
    {
        this.winner = winner;
        this.winningPositions = winningPositions;
    }

    public override void Enter()
    {
        Debug.Log($"Game Over! Player {winner} has won!");

        // Play victory sound
        GameManager.Instance.PlayAudio("game-end");

        // Update score for the winner
        GameManager.Instance.board.scores[(int)winner].IncrementScore();

        // Highlight the winning positions on the board
        GameManager.Instance.board.HighlightWinningPositions(winningPositions);

        // Start a new game after a delay
        stateMachine.ChangeState(new NewGameState(stateMachine));
    }

    public override void Exit()
    {
        Debug.Log("Exiting Game Over State");
    }
}
