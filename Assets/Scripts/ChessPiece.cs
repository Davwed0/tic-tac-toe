using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public enum PieceType
{
    PAWN,
    ROOK,
    KNIGHT,
    BISHOP,
    QUEEN,
    KING
}

public enum PlayerColor
{
    WHITE = 0,
    BLACK = 1,
    EMPTY = -1
}

public class ChessPiece : MonoBehaviour
{
    public PieceType pieceType;
    public PlayerColor player;
    public bool isOnBoard;
    public int index;

    public Mesh pawnMesh, rookMesh, knightMesh, bishopMesh, queenMesh, kingMesh;
    public Material whiteMaterial, blackMaterial, hoverMaterial;

    void Update()
    {
        SetPieceModel();
        SetPieceColor();
    }

    private void OnMouseDown()
    {
        // In multiplayer, only allow selection of your own pieces on your turn
        if (PhotonNetwork.IsConnected)
        {
            if (GameManager.Instance.currentPlayer == player &&
                player == NetworkManager.Instance.GetLocalPlayerColor())
            {
                ICommand command = new SelectPieceCommand(this);
                GameManager.Instance.ExecuteCommand(command);
            }
        }
        else
        {
            // Single player logic
            if (GameManager.Instance.currentPlayer == player)
            {
                ICommand command = new SelectPieceCommand(this);
                GameManager.Instance.ExecuteCommand(command);
            }
        }
    }

    public void PlacePiece(HitBox hitBox)
    {
        ICommand command = new PlacePieceCommand(this, hitBox);
        GameManager.Instance.ExecuteCommand(command);
    }

    public List<int[]> ValidMoves()
    {
        int boardSize = GameManager.Instance.board.boardSize;
        PlayerColor[,] boardState = GameManager.Instance.board.GetBoardState();

        HitBox currentHitBox = transform.parent.GetComponent<HitBox>();
        if (currentHitBox == null) return new List<int[]>();
        int[] currentPosition = currentHitBox.GetPosition();

        List<int[]> validMoves = new List<int[]>();

        switch (pieceType)
        {
            case PieceType.PAWN:
                // Pawn logic remains unchanged as they only move one step
                int forwardVector = player == PlayerColor.WHITE ? -1 : 1;
                int[][] directions = { new[] { forwardVector, 1 }, new[] { forwardVector, -1 } };

                foreach (var direction in directions)
                {
                    int newRow = currentPosition[0] + direction[0];
                    int newColumn = currentPosition[1] + direction[1];

                    if (newRow >= 0 && newRow < boardSize && newColumn >= 0 && newColumn < boardSize)
                    {
                        if (boardState[newRow, newColumn] != player && boardState[newRow, newColumn] != PlayerColor.EMPTY)
                        {
                            validMoves.Add(new[] { newRow, newColumn });
                        }
                    }
                }

                int forwardRow = currentPosition[0] + forwardVector;
                if (forwardRow >= 0 && forwardRow < boardSize)
                {
                    if (boardState[forwardRow, currentPosition[1]] == PlayerColor.EMPTY)
                    {
                        validMoves.Add(new[] { forwardRow, currentPosition[1] });
                    }
                }
                break;

            case PieceType.ROOK:
                int[][] rookDirections = { new[] { 1, 0 }, new[] { -1, 0 }, new[] { 0, 1 }, new[] { 0, -1 } };

                foreach (var direction in rookDirections)
                {
                    for (int step = 1; step < boardSize; step++)
                    {
                        int newRow = currentPosition[0] + direction[0] * step;
                        int newColumn = currentPosition[1] + direction[1] * step;

                        if (newRow < 0 || newRow >= boardSize || newColumn < 0 || newColumn >= boardSize)
                            break; // Out of bounds

                        if (boardState[newRow, newColumn] == player)
                            break; // Can't move through or onto own pieces

                        validMoves.Add(new[] { newRow, newColumn });

                        if (boardState[newRow, newColumn] != PlayerColor.EMPTY)
                            break; // Hit opponent piece, can't go further
                    }
                }
                break;

            case PieceType.KNIGHT:
                // Knights can jump, so their logic remains mostly unchanged
                int[][] knightMoves = {
                    new[] { 2, 1 }, new[] { 2, -1 }, new[] { -2, 1 }, new[] { -2, -1 },
                    new[] { 1, 2 }, new[] { 1, -2 }, new[] { -1, 2 }, new[] { -1, -2 }
                };

                foreach (var move in knightMoves)
                {
                    int newRow = currentPosition[0] + move[0];
                    int newColumn = currentPosition[1] + move[1];

                    if (newRow >= 0 && newRow < boardSize && newColumn >= 0 && newColumn < boardSize)
                    {
                        // Only add if square is empty or has an opponent's piece
                        if (boardState[newRow, newColumn] != player)
                        {
                            validMoves.Add(new[] { newRow, newColumn });
                        }
                    }
                }
                break;

            case PieceType.BISHOP:
                int[][] bishopDirections = { new[] { 1, 1 }, new[] { 1, -1 }, new[] { -1, 1 }, new[] { -1, -1 } };

                foreach (var direction in bishopDirections)
                {
                    for (int step = 1; step < boardSize; step++)
                    {
                        int newRow = currentPosition[0] + direction[0] * step;
                        int newColumn = currentPosition[1] + direction[1] * step;

                        if (newRow < 0 || newRow >= boardSize || newColumn < 0 || newColumn >= boardSize)
                            break; // Out of bounds

                        if (boardState[newRow, newColumn] == player)
                            break; // Can't move through or onto own pieces

                        validMoves.Add(new[] { newRow, newColumn });

                        if (boardState[newRow, newColumn] != PlayerColor.EMPTY)
                            break; // Hit opponent piece, can't go further
                    }
                }
                break;

            case PieceType.QUEEN:
                int[][] queenDirections = {
                    new[] { 1, 0 }, new[] { -1, 0 }, new[] { 0, 1 }, new[] { 0, -1 },
                    new[] { 1, 1 }, new[] { 1, -1 }, new[] { -1, 1 }, new[] { -1, -1 }
                };

                foreach (var direction in queenDirections)
                {
                    for (int step = 1; step < boardSize; step++)
                    {
                        int newRow = currentPosition[0] + direction[0] * step;
                        int newColumn = currentPosition[1] + direction[1] * step;

                        if (newRow < 0 || newRow >= boardSize || newColumn < 0 || newColumn >= boardSize)
                            break; // Out of bounds

                        if (boardState[newRow, newColumn] == player)
                            break; // Can't move through or onto own pieces

                        validMoves.Add(new[] { newRow, newColumn });

                        if (boardState[newRow, newColumn] != PlayerColor.EMPTY)
                            break; // Hit opponent piece, can't go further
                    }
                }
                break;

            case PieceType.KING:
                // King only moves one step so no need to check for jumping
                int[][] kingMoves = {
                    new[] { 1, 0 }, new[] { -1, 0 }, new[] { 0, 1 }, new[] { 0, -1 },
                    new[] { 1, 1 }, new[] { 1, -1 }, new[] { -1, 1 }, new[] { -1, -1 }
                };

                foreach (var move in kingMoves)
                {
                    int newRow = currentPosition[0] + move[0];
                    int newColumn = currentPosition[1] + move[1];

                    if (newRow >= 0 && newRow < boardSize && newColumn >= 0 && newColumn < boardSize)
                    {
                        if (boardState[newRow, newColumn] != player)
                        {
                            validMoves.Add(new[] { newRow, newColumn });
                        }
                    }
                }
                break;
        }

        return validMoves;
    }

    private void SetPieceModel()
    {
        Mesh selectedMesh = pieceType switch
        {
            PieceType.PAWN => pawnMesh,
            PieceType.ROOK => rookMesh,
            PieceType.KNIGHT => knightMesh,
            PieceType.BISHOP => bishopMesh,
            PieceType.QUEEN => queenMesh,
            PieceType.KING => kingMesh,
            _ => null
        };

        if (selectedMesh != null)
        {
            GetComponent<MeshFilter>().mesh = selectedMesh;
            GetComponent<MeshCollider>().sharedMesh = selectedMesh;
        }
    }

    private void SetPieceColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material = player switch
        {
            PlayerColor.WHITE => whiteMaterial,
            PlayerColor.BLACK => blackMaterial,
            _ => null
        };

        if (GameManager.Instance.selectedPiece == this)
        {
            renderer.material = hoverMaterial;
        }
    }
}