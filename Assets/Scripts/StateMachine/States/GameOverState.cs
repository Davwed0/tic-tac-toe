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

        GameManager.Instance.PlayAudio("game-end");

        GameManager.Instance.board.scores[(int)winner].IncrementScore();
        GameManager.Instance.board.HighlightWinningPositions(winningPositions);

        stateMachine.ChangeState(new NewGameState(stateMachine));
    }

    public override void Exit()
    {
        Debug.Log("Exiting Game Over State");
    }
}
