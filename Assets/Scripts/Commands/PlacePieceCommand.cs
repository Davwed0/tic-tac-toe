using UnityEngine;
using Photon.Pun;

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
        // In multiplayer, verify it's your turn and your piece
        if (PhotonNetwork.IsConnected)
        {
            PlayerColor localPlayerColor = NetworkManager.Instance.GetLocalPlayerColor();
            if (piece.player != localPlayerColor || GameManager.Instance.currentPlayer != localPlayerColor)
            {
                Debug.LogWarning("Cannot place piece - not your turn or not your piece");
                return;
            }

            // Send network event for this piece placement
            NetworkManager.Instance.SendPlacePieceEvent(piece, targetHitBox);
        }

        if (piece.transform.parent != null && piece.transform.parent.GetComponent<HitBox>() != null)
        {
            previousHitBox = piece.transform.parent.GetComponent<HitBox>();
            previousHitBox.player = PlayerColor.EMPTY;
        }

        // Check if there's a piece to capture
        capturedPiece = targetHitBox.GetPiece();
        if (capturedPiece != null)
        {
            // Only allow captures after round 3 (turn 6)
            if (GameManager.Instance.currentTurn >= 6)
            {
                // Play capture sound
                AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    AudioClip captureSound = Resources.Load<AudioClip>("Audio/capture");
                    if (captureSound != null)
                        audioSource.PlayOneShot(captureSound);
                }

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
            else
            {
                // Before round 3, can't capture - revert to just move sound
                AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    AudioClip moveSound = Resources.Load<AudioClip>("Audio/move-self");
                    if (moveSound != null)
                        audioSource.PlayOneShot(moveSound);
                }

                // Don't allow placement on occupied spots before round 3
                PlayerTurnState currState = GameManager.Instance.stateMachine.CurrentState as PlayerTurnState;
                if (currState != null)
                {
                    currState.EndTurn();
                }
                GameManager.Instance.selectedPiece = null;
                return;
            }
        }
        else
        {
            // Play move sound (no capture)
            AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                AudioClip moveSound = Resources.Load<AudioClip>("Audio/move-self");
                if (moveSound != null)
                    audioSource.PlayOneShot(moveSound);
            }
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
