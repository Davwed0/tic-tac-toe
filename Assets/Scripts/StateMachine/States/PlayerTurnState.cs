using UnityEngine;
using System.Collections.Generic;

public class PlayerTurnState : GameState
{
    private PlayerColor currentPlayer;

    public PlayerTurnState(GameStateMachine stateMachine, PlayerColor player) : base(stateMachine)
    {
        currentPlayer = player;
    }

    public override void Enter()
    {
        Debug.Log($"Player {currentPlayer} turn started");
        if (currentPlayer == PlayerColor.WHITE)
        {
            GameManager.Instance.board.GenerateNewPieces();
        }
        GameManager.Instance.currentPlayer = currentPlayer;
        GameManager.Instance.board.CheckPieceQueues();

        // Clear any previous selections
        GameManager.Instance.selectedPiece = null;
    }

    public override void Update()
    {
        // Update hitbox rendering based on selected piece
        if (GameManager.Instance.selectedPiece != null)
        {
            GameManager.Instance.RenderValidMoves();
        }
        else
        {
            GameManager.Instance.DestroyHitBoxes();
        }
    }

    public override void Exit()
    {
        GameManager.Instance.DestroyHitBoxes();
    }

    public void EndTurn()
    {
        PlayerColor nextPlayer = (currentPlayer == PlayerColor.WHITE) ? PlayerColor.BLACK : PlayerColor.WHITE;

        // Check if there's a winner before changing player
        var result = GameManager.Instance.board.CheckWin();
        PlayerColor winner = result.winner;
        List<Vector2Int> positions = result.positions;
        if (winner != PlayerColor.EMPTY)
        {
            stateMachine.ChangeState(new GameOverState(stateMachine, winner, positions));
        }
        else
        {
            GameManager.Instance.IncrementTurn();
            stateMachine.ChangeState(new PlayerTurnState(stateMachine, nextPlayer));
        }
    }
}
