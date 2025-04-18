using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class PlayerTurnState : GameState, IOnEventCallback
{
    private const byte SEND_PIECE_EVENT = 1;
    private const byte PLACE_PIECE_EVENT = 2;

    private PlayerColor currentPlayer;
    private bool canMakeMove = false;
    private bool moving = false;

    public PlayerTurnState(GameStateMachine stateMachine, PlayerColor player) : base(stateMachine)
    {
        currentPlayer = player;
    }

    public override void Enter()
    {
        Debug.Log("Entering PlayerTurn State");

        PhotonNetwork.AddCallbackTarget(this);

        PlayerColor localPlayerColor = NetworkManager.Instance.IsMasterClient() ? PlayerColor.WHITE : PlayerColor.BLACK;
        canMakeMove = (localPlayerColor == currentPlayer);

        Debug.Log($"Local player can move: {canMakeMove}");

        if (NetworkManager.Instance.IsMasterClient())
        {
            Debug.Log($"Player {currentPlayer} turn started");
            if (currentPlayer == PlayerColor.WHITE)
            {
                PieceType newPiece = GameManager.Instance.board.GenerateNewPieces();

                SendPieceData(newPiece);
            }
        }
        else
        {
            Debug.Log($"Waiting on Player {currentPlayer}'s turn");
        }

        GameManager.Instance.currentPlayer = currentPlayer;
        GameManager.Instance.selectedPiece = null;
    }

    public override void Update()
    {
        if (!canMakeMove) return;

        if (GameManager.Instance.selectedPiece != null && !moving)
        {
            GameManager.Instance.RenderValidMoves();

            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    HitBox hitBox = hit.collider.GetComponent<HitBox>();
                    if (hitBox != null && hitBox.isValid)
                    {
                        ChessPiece selectedPiece = GameManager.Instance.selectedPiece;
                        int[] position = hitBox.GetPosition();
                        int row = position[0];
                        int col = position[1];

                        PlacePiece(selectedPiece, hitBox);
                        SendMovePlacement(selectedPiece.index, (int)currentPlayer, row, col);

                        EndTurn();
                    }
                }
            }
        }
        else
        {
            GameManager.Instance.DestroyHitBoxes();
        }
    }

    public void SelectPiece(ChessPiece piece)
    {
        if (canMakeMove)
        {
            GameManager.Instance.selectedPiece = piece;
            GameManager.Instance.RenderValidMoves();
        }
    }

    public void PlacePiece(ChessPiece piece, HitBox hitBox)
    {
        if (!canMakeMove) return;

        int[] position = hitBox.GetPosition();
        int row = position[0];
        int col = position[1];

        ChessPiece existingPiece = hitBox.GetPiece();
        if (existingPiece != null && existingPiece != piece)
        {
            int capturedIndex = existingPiece.index;
            int capturedPlayer = (int)existingPiece.player;
            Debug.Log($"Capturing piece: Player {capturedPlayer}, Index {capturedIndex}");
            Object.Destroy(existingPiece.gameObject);
            GameManager.Instance.board.chessPieces[capturedIndex, capturedPlayer] = null;

            GameManager.Instance.PlayAudio("capture");
        }
        else
        {
            GameManager.Instance.PlayAudio("move-self");
        }

        if (piece.transform.parent != null &&
            piece.transform.parent.GetComponent<HitBox>() != null)
        {
            HitBox previousHitBox = piece.transform.parent.GetComponent<HitBox>();
            previousHitBox.player = PlayerColor.EMPTY;
        }

        // Animate movement
        piece.isOnBoard = true;
        Vector3 startPosition = piece.transform.position;
        Vector3 targetPosition = hitBox.transform.position;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float movementSpeed = 125f; // Units per second
        float duration = distance / movementSpeed;

        piece.transform.parent = null; // Detach during animation

        MonoBehaviour mb = GameManager.Instance;
        mb.StartCoroutine(AnimateMovement(piece, startPosition, targetPosition, duration, () =>
        {
            // Callback after animation completes
            piece.transform.SetParent(hitBox.transform);
            hitBox.player = currentPlayer;
            SendMovePlacement(piece.index, (int)currentPlayer, row, col);
            Debug.Log($"Placed piece {piece.index} for player {currentPlayer} at [{row},{col}]");
            EndTurn();
        }));
    }

    private System.Collections.IEnumerator AnimateMovement(ChessPiece piece, Vector3 start, Vector3 target, float duration, System.Action onComplete)
    {
        moving = true;
        float elapsed = 0;
        while (elapsed < duration)
        {
            piece.transform.position = Vector3.Lerp(start, target, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure final position is exact
        piece.transform.position = target;
        moving = false;
        onComplete?.Invoke();
    }

    public void PlacePiece(ChessPiece piece, int row, int col)
    {
        if (!canMakeMove) return;

        HitBox targetBox = GameManager.Instance.board.hitBoxes[row, col];
        PlacePiece(piece, targetBox);
    }

    private void SendMovePlacement(int pieceIndex, int player, int row, int col)
    {
        int[] moveData = new int[] { pieceIndex, player, row, col };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(PLACE_PIECE_EVENT, moveData, raiseEventOptions, SendOptions.SendReliable);

        Debug.Log($"Sent move placement: Piece {pieceIndex}, Player {player}, Position [{row},{col}]");
    }

    private void SendPieceData(PieceType newPiece)
    {
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(SEND_PIECE_EVENT, newPiece, raiseEventOptions, SendOptions.SendReliable);

        Debug.Log("Sent piece data to other clients");
        Debug.Log($"Piece Data: {newPiece} ");
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == SEND_PIECE_EVENT && !NetworkManager.Instance.IsMasterClient())
        {
            PieceType newPieceType = (PieceType)(int)photonEvent.CustomData;
            Debug.Log("Received piece data from master client");
            Debug.Log($"Piece Data: Player: {newPieceType}");

            GameManager.Instance.board.AddNewPiece(PlayerColor.WHITE, newPieceType);
            GameManager.Instance.board.AddNewPiece(PlayerColor.BLACK, newPieceType);
        }
        else if (photonEvent.Code == PLACE_PIECE_EVENT)
        {
            int[] data = (int[])photonEvent.CustomData;
            int index = data[0];
            int player = data[1];
            int row = data[2];
            int col = data[3];
            Debug.Log($"Received piece placement: Piece {index}, Player {player}, Position [{row},{col}]");

            PlayerColor placingPlayer = (PlayerColor)player;
            ChessPiece placedPiece = GameManager.Instance.board.chessPieces[index, (int)placingPlayer];

            if (placedPiece != null)
            {
                HitBox targetBox = GameManager.Instance.board.hitBoxes[row, col];

                if (targetBox != null)
                {
                    ChessPiece existingPiece = targetBox.GetPiece();
                    if (existingPiece != null && existingPiece != placedPiece)
                    {
                        int capturedIndex = existingPiece.index;
                        int capturedPlayer = (int)existingPiece.player;
                        Debug.Log($"Capturing piece: Player {capturedPlayer}, Index {capturedIndex}");
                        Object.Destroy(existingPiece.gameObject);
                        GameManager.Instance.board.chessPieces[capturedIndex, capturedPlayer] = null;
                        GameManager.Instance.PlayAudio("capture");
                    }
                    else
                    {
                        GameManager.Instance.PlayAudio("move-self");
                    }

                    if (placedPiece.transform.parent != null &&
                        placedPiece.transform.parent.GetComponent<HitBox>() != null)
                    {
                        HitBox previousHitBox = placedPiece.transform.parent.GetComponent<HitBox>();
                        previousHitBox.player = PlayerColor.EMPTY;
                    }

                    placedPiece.isOnBoard = true;
                    placedPiece.transform.position = targetBox.transform.position;
                    placedPiece.transform.SetParent(targetBox.transform);
                    targetBox.player = placingPlayer;

                    Debug.Log($"Successfully moved piece {index} for player {placingPlayer} to [{row},{col}]");
                }
            }

            EndTurn();
        }
    }

    public override void Exit()
    {
        GameManager.Instance.DestroyHitBoxes();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    public void EndTurn()
    {
        PlayerColor nextPlayer = (currentPlayer == PlayerColor.WHITE) ? PlayerColor.BLACK : PlayerColor.WHITE;

        var result = GameManager.Instance.board.CheckWin();
        PlayerColor winner = result.winner;
        List<Vector2Int> positions = result.positions;
        Debug.Log($"Winner: {winner}");
        if (winner != PlayerColor.EMPTY)
        {
            Debug.Log($"Game Over! Player {winner} has won!");
            stateMachine.ChangeState(new GameOverState(stateMachine, winner, positions));
        }
        else
        {
            Debug.Log($"Player {nextPlayer}'s turn started");
            GameManager.Instance.board.CheckPieceQueues();
            GameManager.Instance.IncrementTurn();
            stateMachine.ChangeState(new PlayerTurnState(stateMachine, nextPlayer));
        }
    }
}
