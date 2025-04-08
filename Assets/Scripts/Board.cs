using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public int boardSize;
    public int handSize;
    public float hitBoxGap;
    public int pad = 3;

    public GameObject hitBoxPrefab;
    public GameObject piecePrefab;
    public GameObject pieceSlotPrefab;
    public GameObject scorePrefab;
    public GameObject boardMesh;

    public HitBox[,] hitBoxes;
    public ChessPiece[,] chessPieces;
    public PieceSlot[] pieceSlots;
    public Score[] scores;

    private void Awake()
    {
        InitializeBoard();
        InitializePieces();
        InitializeSlots();
        InitializeScores();
    }

    private void InitializeBoard()
    {
        hitBoxes = new HitBox[boardSize, boardSize];
        float hitBoxSize = hitBoxPrefab.transform.localScale.x;
        float totalGap = hitBoxSize + hitBoxGap;
        float offset = (boardSize - 1) * totalGap / 2.0f;

        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                float xPos = row * totalGap - offset;
                float yPos = column * totalGap - offset;
                GameObject hitBoxObject = Instantiate(hitBoxPrefab, new Vector3(xPos, 0.35f, yPos), Quaternion.identity);
                hitBoxObject.transform.SetParent(transform);
                hitBoxObject.layer = LayerMask.NameToLayer("Outlined");
                hitBoxObject.transform.GetChild(0).gameObject.layer = LayerMask.NameToLayer("Outlined");

                hitBoxes[row, column] = hitBoxObject.GetComponent<HitBox>();
                hitBoxes[row, column].name = $"HitBox_{row}_{column}";

                hitBoxes[row, column].SetPosition(row, column);
            }
        }
    }

    private void InitializePieces()
    {
        chessPieces = new ChessPiece[handSize, 2];
        float hitBoxSize = hitBoxPrefab.transform.localScale.x;
        float totalGap = hitBoxSize + hitBoxGap;
        float offsetX = (boardSize * totalGap) / 2 + pad;
        float offsetY = (handSize * hitBoxSize) / 2;

        for (int i = 0; i < handSize; i++)
        {
            float yPosW = i * hitBoxSize - offsetY;
            float yPosB = (handSize - i - 1) * hitBoxSize - offsetY;
            PieceType randomPieceType = (PieceType)Random.Range(0, 6);

            chessPieces[i, 0] = CreatePiece(offsetX, yPosW + 2, randomPieceType, i, PlayerColor.WHITE);
            chessPieces[i, 1] = CreatePiece(-offsetX, yPosB, randomPieceType, i, PlayerColor.BLACK);
        }
    }

    private void InitializeSlots()
    {
        pieceSlots = new PieceSlot[2];
        float hitBoxSize = hitBoxPrefab.transform.localScale.x;
        float totalGap = hitBoxSize + hitBoxGap;
        float offsetX = (boardSize * totalGap) / 2 + pad;

        for (int i = 0; i < 2; i++)
        {
            float xPosition = i == 0 ? offsetX : -offsetX;
            GameObject slotObject = Instantiate(pieceSlotPrefab, new Vector3(xPosition, 0, 0), Quaternion.identity);
            slotObject.transform.localScale = new Vector3(hitBoxSize, 0.25f, ((handSize + 1) * totalGap));
            slotObject.transform.SetParent(transform);

            pieceSlots[i] = slotObject.GetComponent<PieceSlot>();
            pieceSlots[i].player = (PlayerColor)i;

            slotObject.name = $"Slot_{i}";
        }
    }

    private void InitializeScores()
    {
        scores = new Score[2];
        float hitBoxSize = hitBoxPrefab.transform.localScale.x;
        float totalGap = hitBoxSize + hitBoxGap;
        float offsetX = (boardSize * totalGap) / 2 + pad;
        float offsetY = (handSize + 1) * hitBoxSize / 2;

        for (int i = 0; i < 2; i++)
        {
            float xPosition = i == 0 ? offsetX : -offsetX;
            float yPosition = i == 0 ? -offsetY + 1 : offsetY - 1;
            float yRotation = i == 0 ? 90 : -90;
            GameObject scoreObject = Instantiate(scorePrefab, new Vector3(xPosition, 0.5f, yPosition), Quaternion.identity);
            scoreObject.transform.position = new Vector3(xPosition, 0.2f, yPosition);
            scoreObject.transform.localScale = new Vector3(hitBoxSize, hitBoxSize, hitBoxSize);
            scoreObject.transform.Rotate(new Vector3(0, yRotation, 0));
            scoreObject.transform.SetParent(transform);
            scoreObject.name = $"Score_{i}";
            scoreObject.layer = LayerMask.NameToLayer("Outlined");

            scores[i] = scoreObject.GetComponent<Score>();
            scores[i].player = i;
        }
    }

    private ChessPiece CreatePiece(float xPos, float yPos, PieceType pieceType, int index, PlayerColor player = PlayerColor.WHITE)
    {
        GameObject chessPieceObject = Instantiate(piecePrefab, new Vector3(xPos, 0, yPos), Quaternion.identity);
        if (player == PlayerColor.BLACK)
        {
            chessPieceObject.transform.Rotate(new Vector3(0, 90, 0));
        }
        else
        {
            chessPieceObject.transform.Rotate(new Vector3(0, -90, 0));
        }
        chessPieceObject.transform.SetParent(transform);
        chessPieceObject.layer = LayerMask.NameToLayer("Outlined");
        ChessPiece chessPiece = chessPieceObject.GetComponent<ChessPiece>();
        chessPiece.pieceType = pieceType;
        chessPiece.index = index;
        chessPiece.player = player;
        chessPieceObject.name = (player == PlayerColor.WHITE ? "W_" : "B_") + index;

        return chessPiece;
    }

    public void MovePieceToHand(ChessPiece chessPiece, PlayerColor player)
    {
        float hitBoxSize = hitBoxPrefab.transform.localScale.x;
        float xPos = ((boardSize * (hitBoxSize + hitBoxGap)) / 2 + pad) * (player == PlayerColor.WHITE ? 1 : -1);
        float index = (player == PlayerColor.WHITE ? chessPiece.index : (handSize - chessPiece.index - 1));
        float yPos = index * hitBoxSize - (handSize * hitBoxSize) / 2 + (player == PlayerColor.WHITE ? 2 : 0);

        chessPiece.transform.position = new Vector3(xPos, 0, yPos);
        chessPiece.transform.SetParent(transform);
        chessPiece.isOnBoard = false;
    }

    public bool CheckCollisions(Vector3 position)
    {
        Bounds slotBounds1 = pieceSlots[0].GetComponent<Collider>().bounds;
        Bounds slotBounds2 = pieceSlots[1].GetComponent<Collider>().bounds;
        slotBounds1.Expand(new Vector3(0.5f, 100f, 0.5f));
        slotBounds2.Expand(new Vector3(0.5f, 100f, 0.5f));

        Bounds boardMeshBounds = boardMesh.transform.GetChild(0).gameObject.GetComponent<Collider>().bounds;
        boardMeshBounds.Expand(new Vector3(0.5f, 100f, 0.5f));

        if (boardMeshBounds.Contains(position) || slotBounds1.Contains(position) || slotBounds2.Contains(position))
        {
            return true;
        }

        return false;
    }

    public PlayerColor CheckWin()
    {
        PlayerColor[,] boardState = GetBoardState();

        // print current turn then the board state
        Debug.Log($"Current Turn: {GameManager.Instance.currentTurn}");
        for (int row = 0; row < boardSize; row++)
        {
            string rowString = "";
            for (int col = 0; col < boardSize; col++)
            {
                rowString += (int)boardState[row, col] + " ";
            }
            Debug.Log(rowString);
        }

        // Check rows for a winner
        for (int row = 0; row < 3; row++)
        {
            if (boardState[row, 0] != PlayerColor.EMPTY && boardState[row, 0] == boardState[row, 1] && boardState[row, 1] == boardState[row, 2])
            {
                return boardState[row, 0]; // Return the winning player (WHITE or BLACK)
            }
        }

        // Check columns for a winner
        for (int col = 0; col < 3; col++)
        {
            if (boardState[0, col] != PlayerColor.EMPTY && boardState[0, col] == boardState[1, col] && boardState[1, col] == boardState[2, col])
            {
                return boardState[0, col]; // Return the winning player (WHITE or BLACK)
            }
        }

        // Check first diagonal (top-left to bottom-right)
        if (boardState[0, 0] != PlayerColor.EMPTY && boardState[0, 0] == boardState[1, 1] && boardState[1, 1] == boardState[2, 2])
        {
            return boardState[0, 0]; // Return the winning player (WHITE or BLACK)
        }

        // Check second diagonal (top-right to bottom-left)
        if (boardState[0, 2] != PlayerColor.EMPTY && boardState[0, 2] == boardState[1, 1] && boardState[1, 1] == boardState[2, 0])
        {
            return boardState[0, 2]; // Return the winning player (WHITE or BLACK)
        }

        // If no winner, return EMPTY
        return PlayerColor.EMPTY;
    }

    public PlayerColor[,] GetBoardState()
    {
        PlayerColor[,] boardState = new PlayerColor[boardSize, boardSize];

        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                boardState[row, col] = hitBoxes[row, col].GetPlayer();
            }
        }

        return boardState;
    }

    public void Reset()
    {
        for (int i = 0; i < handSize; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                chessPieces[i, j].isOnBoard = false;
                MovePieceToHand(chessPieces[i, j], (PlayerColor)j);
            }
        }

        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                hitBoxes[row, col].Reset();
            }
        }

        // rerandomize pieces
        for (int i = 0; i < handSize; i++)
        {
            PieceType randomPieceType = (PieceType)Random.Range(0, 6);
            chessPieces[i, 0].pieceType = randomPieceType;
            chessPieces[i, 1].pieceType = randomPieceType;
        }

        GameManager.Instance.currentTurn = 0;
        GameManager.Instance.currentPlayer = PlayerColor.WHITE;
        GameManager.Instance.selectedPiece = null;
    }
}
