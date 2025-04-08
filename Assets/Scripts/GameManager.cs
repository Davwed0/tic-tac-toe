using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Board board;

    public PlayerColor currentPlayer = PlayerColor.WHITE;
    public int currentTurn = 0;
    public ChessPiece selectedPiece;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            currentPlayer = PlayerColor.WHITE;
            currentTurn = 0;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        board = GameObject.Find("Board").GetComponent<Board>();
        Debug.Log($"Game started with player: {currentPlayer}");
    }

    private void Update()
    {
        if (selectedPiece != null)
        {
            DestroyHitBoxes();
            RenderValidHitBoxes();
        }
        else
        {
            DestroyHitBoxes();
        }
    }

    public void IncrementTurn()
    {
        currentTurn++;
        currentPlayer = currentPlayer == PlayerColor.WHITE ? PlayerColor.BLACK : PlayerColor.WHITE;
    }

    private void RenderValidHitBoxes()
    {
        if (selectedPiece == null) return;
        List<int[]> pieceValidMoves = selectedPiece.ValidMoves();

        GameObject selectedObject = this.selectedPiece.gameObject;
        if (pieceValidMoves == null) return;

        for (int i = 0; i < pieceValidMoves.Count; i++)
        {
            int[] move = pieceValidMoves[i];
            int row = move[0];
            int column = move[1];
            HitBox hitBox = board.hitBoxes[row, column];

            if (hitBox.GetPlayer() != selectedPiece.player || hitBox.GetPlayer() == PlayerColor.EMPTY)
            {
                board.hitBoxes[row, column].Render();
            }
        }

        if (!selectedPiece.isOnBoard)
        {
            for (int row = 0; row < board.boardSize; row++)
            {
                for (int column = 0; column < board.boardSize; column++)
                {
                    if (board.hitBoxes[row, column].GetPlayer() == PlayerColor.EMPTY)
                    {
                        board.hitBoxes[row, column].Render();
                    }
                }
            }
        }
    }

    private void DestroyHitBoxes()
    {
        GameObject[] pieces = GameObject.FindGameObjectsWithTag("Piece");

        for (int row = 0; row < board.boardSize; row++)
        {
            for (int column = 0; column < board.boardSize; column++)
            {
                board.hitBoxes[row, column].Destroy();
            }
        }
    }
}