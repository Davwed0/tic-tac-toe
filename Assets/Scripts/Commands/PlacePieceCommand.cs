using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class PlacePieceCommand : ICommand, IOnEventCallback
{
    private const byte PLACE_PIECE_EVENT = 2;

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
                int capturedPlayer = (int)capturedPiece.player;
                int capturedIndex = capturedPiece.index;
                Debug.Log($"Captured piece: Player {capturedPlayer}, Index {capturedIndex}");
                Object.Destroy(capturedPiece.gameObject);
                GameManager.Instance.board.chessPieces[capturedIndex, capturedPlayer] = null;

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

        // Send placement data to other clients
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.AddCallbackTarget(this);
            SendPlacementData();
        }

        // End the turn through state machine
        PlayerTurnState state = GameManager.Instance.stateMachine.CurrentState as PlayerTurnState;
        if (state != null)
        {
            state.EndTurn();
        }

        GameManager.Instance.selectedPiece = null;
    }

    private void SendPlacementData()
    {
        // Get position from hitbox
        int[] position = targetHitBox.GetPosition();
        int row = position[0];
        int col = position[1];

        // Create data object to send
        int[] data = new int[4];
        data[0] = piece.index;
        data[1] = (int)piece.player;
        data[2] = row;
        data[3] = col;

        // Send to other players
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(PLACE_PIECE_EVENT, data, raiseEventOptions, SendOptions.SendReliable);

        Debug.Log($"Sent piece placement: Piece {piece.index}, Player {piece.player}, Position [{row},{col}]");
    }

    public void OnEvent(EventData photonEvent)
    {
        return;
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
