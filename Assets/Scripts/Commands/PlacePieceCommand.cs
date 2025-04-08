using UnityEngine;

public class PlacePieceCommand : ICommand
{
    private ChessPiece piece;
    private HitBox targetHitBox;
    private HitBox previousHitBox;
    private ChessPiece capturedPiece;
    private Vector3 capturedPiecePosition;
    private Transform capturedPieceParent;

    public PlacePieceCommand(ChessPiece piece, HitBox hitBox)
    {
        this.piece = piece;
        this.targetHitBox = hitBox;
    }

    public void Execute()
    {
        if (piece.transform.parent != null && piece.transform.parent.GetComponent<HitBox>() != null)
        {
            previousHitBox = piece.transform.parent.GetComponent<HitBox>();
            previousHitBox.player = PlayerColor.EMPTY;
        }

        // Check if there's a piece to capture
        capturedPiece = targetHitBox.GetPiece();
        if (capturedPiece != null)
        {
            // Store information about captured piece for potential undo
            capturedPiecePosition = capturedPiece.transform.position;
            capturedPieceParent = capturedPiece.transform.parent;

            // Get the player of the captured piece before destroying it
            PlayerColor capturedPlayer = capturedPiece.player;
            int capturedIndex = capturedPiece.index;
            Debug.Log($"Captured piece: Player {(int)capturedPlayer}, Index {capturedIndex}");
            GameManager.Instance.board.chessPieces[capturedIndex, (int)capturedPlayer] = null;
            Object.Destroy(capturedPiece.gameObject);

            GameManager.Instance.board.CheckPieceQueues();
        }

        // Place the piece
        piece.isOnBoard = true;
        piece.transform.position = targetHitBox.transform.position;
        piece.transform.SetParent(targetHitBox.transform);
        targetHitBox.player = GameManager.Instance.currentPlayer;

        // End the turn through state machine
        PlayerTurnState state = GameManager.Instance.stateMachine.CurrentState as PlayerTurnState;
        if (state != null)
        {
            state.EndTurn();
        }

        GameManager.Instance.selectedPiece = null;
    }

    public void Undo()
    {
        // Note: We can't restore destroyed pieces with the same instance

        // Restore the position of the moved piece
        if (previousHitBox != null)
        {
            piece.transform.position = previousHitBox.transform.position;
            piece.transform.SetParent(previousHitBox.transform);
            previousHitBox.player = piece.player;
        }
        else
        {
            // If it wasn't on board previously, move it back to hand
            GameManager.Instance.board.MovePieceToHand(piece, piece.player);
        }

        // Reset the target hitbox
        targetHitBox.player = PlayerColor.EMPTY;
    }
}
