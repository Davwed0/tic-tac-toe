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
    public GameObject gridBoxPrefab;

    public HitBox[,] hitBoxes;
    public GameObject[,] gridBoxes;
    public ChessPiece[,] chessPieces;
    public PieceSlot[] pieceSlots;
    public Score[] scores;

    public Material whiteMaterial, blackMaterial;

    public int winLength = 3; // Default is 3 in a row for traditional tic-tac-toe

    // Queues for pieces when hands are full
    private Queue<PieceType> whitePlayerQueue = new Queue<PieceType>();
    private Queue<PieceType> blackPlayerQueue = new Queue<PieceType>();

    private void Awake()
    {
        hitBoxes = new HitBox[boardSize, boardSize];
        gridBoxes = new GameObject[boardSize, boardSize];
        chessPieces = new ChessPiece[handSize, 2];
        pieceSlots = new PieceSlot[2];
        scores = new Score[2];
    }

    public void InitializeBoard()
    {
        float hitBoxSize = hitBoxPrefab.transform.localScale.x;
        float totalGap = hitBoxSize + hitBoxGap;
        float offset = (boardSize - 1) * totalGap / 2.0f;

        for (int row = 0; row < boardSize; row++)
        {
            for (int column = 0; column < boardSize; column++)
            {
                float xPos = row * totalGap - offset;
                float yPos = column * totalGap - offset;
                GameObject gridBoxObject = Instantiate(gridBoxPrefab, new Vector3(xPos, 0.25f, yPos), Quaternion.identity);
                gridBoxObject.transform.SetParent(transform);
                gridBoxObject.GetComponent<Renderer>().material = ((row + column) % 2 == 0) ? blackMaterial : whiteMaterial;
                gridBoxes[row, column] = gridBoxObject.gameObject;

                GameObject hitBoxObject = Instantiate(hitBoxPrefab, new Vector3(xPos, 0.54f, yPos), Quaternion.identity);
                hitBoxObject.transform.SetParent(transform);

                hitBoxes[row, column] = hitBoxObject.GetComponent<HitBox>();
                hitBoxes[row, column].name = $"HitBox_{row}_{column}";

                hitBoxes[row, column].SetPosition(row, column);
            }
        }
    }

    public void InitializePieces()
    {
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

    public void InitializeSlots()
    {
        float hitBoxSize = hitBoxPrefab.transform.localScale.x;
        float totalGap = hitBoxSize + hitBoxGap;
        float offsetX = (boardSize * totalGap) / 2 + pad;

        for (int i = 0; i < 2; i++)
        {
            float xPosition = i == 0 ? offsetX : -offsetX;
            GameObject slotObject = Instantiate(pieceSlotPrefab, new Vector3(xPosition, 0, 0), Quaternion.identity);
            slotObject.transform.SetParent(transform);

            pieceSlots[i] = slotObject.GetComponent<PieceSlot>();
            pieceSlots[i].player = (PlayerColor)i;

            slotObject.name = $"Slot_{i}";
        }
    }

    public void InitializeScores()
    {
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

            scores[i] = scoreObject.GetComponent<Score>();
            scores[i].player = i;
        }
    }

    public ChessPiece CreatePiece(float xPos, float yPos, PieceType pieceType, int index, PlayerColor player = PlayerColor.WHITE)
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

        if (slotBounds1.Contains(position) || slotBounds2.Contains(position))
        {
            return true;
        }

        return false;
    }

    public (PlayerColor winner, List<Vector2Int> positions) CheckWin()
    {
        PlayerColor[,] boardState = GetBoardState();

        // Check rows
        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col <= boardSize - winLength; col++)
            {
                bool match = true;
                PlayerColor player = boardState[row, col];
                if (player == PlayerColor.EMPTY) continue;

                List<Vector2Int> positions = new List<Vector2Int>();

                for (int k = 0; k < winLength; k++)
                {
                    if (boardState[row, col + k] != player)
                    {
                        match = false;
                        break;
                    }
                    positions.Add(new Vector2Int(row, col + k));
                }

                if (match)
                    return (winner: player, positions: positions);
            }
        }

        // Check columns
        for (int col = 0; col < boardSize; col++)
        {
            for (int row = 0; row <= boardSize - winLength; row++)
            {
                bool match = true;
                PlayerColor player = boardState[row, col];
                if (player == PlayerColor.EMPTY) continue;

                List<Vector2Int> positions = new List<Vector2Int>();

                for (int k = 0; k < winLength; k++)
                {
                    if (boardState[row + k, col] != player)
                    {
                        match = false;
                        break;
                    }
                    positions.Add(new Vector2Int(row + k, col));
                }

                if (match)
                    return (winner: player, positions: positions);
            }
        }

        // Check diagonals (top-left to bottom-right)
        for (int row = 0; row <= boardSize - winLength; row++)
        {
            for (int col = 0; col <= boardSize - winLength; col++)
            {
                bool match = true;
                PlayerColor player = boardState[row, col];
                if (player == PlayerColor.EMPTY) continue;

                List<Vector2Int> positions = new List<Vector2Int>();

                for (int k = 0; k < winLength; k++)
                {
                    if (boardState[row + k, col + k] != player)
                    {
                        match = false;
                        break;
                    }
                    positions.Add(new Vector2Int(row + k, col + k));
                }

                if (match)
                    return (winner: player, positions: positions);
            }
        }

        // Check diagonals (top-right to bottom-left)
        for (int row = 0; row <= boardSize - winLength; row++)
        {
            for (int col = winLength - 1; col < boardSize; col++)
            {
                bool match = true;
                PlayerColor player = boardState[row, col];
                if (player == PlayerColor.EMPTY) continue;

                List<Vector2Int> positions = new List<Vector2Int>();

                for (int k = 0; k < winLength; k++)
                {
                    if (boardState[row + k, col - k] != player)
                    {
                        match = false;
                        break;
                    }
                    positions.Add(new Vector2Int(row + k, col - k));
                }

                if (match)
                    return (winner: player, positions: positions);
            }
        }

        // If no winner, return EMPTY
        return (winner: PlayerColor.EMPTY, positions: null);
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
        // First destroy any pieces on the board
        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                ChessPiece piece = hitBoxes[row, col].GetPiece();
                if (piece != null)
                {
                    Destroy(piece.gameObject);
                }
                hitBoxes[row, col].Reset();
                gridBoxes[row, col].GetComponent<Renderer>().material = ((row + col) % 2 == 0) ? blackMaterial : whiteMaterial;

            }
        }

        for (int hand = 0; hand < handSize; hand++)
        {
            // Destroy any pieces in the hands
            Destroy(chessPieces[hand, 0].gameObject);
            Destroy(chessPieces[hand, 1].gameObject);

            chessPieces[hand, 0] = null;
            chessPieces[hand, 1] = null;
        }

        GameManager.Instance.currentTurn = 0;
        GameManager.Instance.currentPlayer = PlayerColor.WHITE;
        GameManager.Instance.selectedPiece = null;
    }

    // Add a new piece to the player's hand or queue if hand is full
    public void AddNewPiece(PlayerColor player, PieceType piece)
    {
        if (player == PlayerColor.WHITE)
            whitePlayerQueue.Enqueue(piece);
        else
            blackPlayerQueue.Enqueue(piece);
    }

    // Try to add a piece to the player's hand, return true if successful
    public bool AddPieceToHand(PlayerColor player, PieceType pieceType)
    {
        int playerIndex = (int)player;

        // Check if there's an empty slot in the hand
        for (int i = 0; i < handSize; i++)
        {
            if (chessPieces[i, playerIndex] == null)
            {
                // Create the piece in the empty slot
                float hitBoxSize = hitBoxPrefab.transform.localScale.x;
                float totalGap = hitBoxSize + hitBoxGap;
                float offsetX = (boardSize * totalGap) / 2 + pad;
                float offsetY = (handSize * hitBoxSize) / 2;

                float yPos = playerIndex == 0 ?
                    i * hitBoxSize - offsetY :
                    (handSize - i - 1) * hitBoxSize - offsetY;

                float xPos = (playerIndex == 0 ? 1 : -1) * offsetX;
                float yPositionOffset = playerIndex == 0 ? 2 : 0;

                chessPieces[i, playerIndex] = CreatePiece(xPos, yPos + yPositionOffset, pieceType, i, player);
                Debug.Log($"Added a {pieceType} to player {player}'s hand at position {i}");
                return true;
            }
            else
            {
                Debug.Log($"Slot {i} is already occupied for player {player}");
            }
        }

        return false; // Hand is full
    }

    public void CheckPieceQueues()
    {
        if (whitePlayerQueue.Count > 0)
        {
            PieceType nextPiece = whitePlayerQueue.Peek();
            bool added = AddPieceToHand(PlayerColor.WHITE, nextPiece);

            if (added)
            {
                whitePlayerQueue.Dequeue();
                Debug.Log($"Added queued {nextPiece} to White player's hand");
            }
            else
            {
                Debug.Log($"Failed to add queued {nextPiece} to White player's hand, putting it back in queue");
            }
        }
        if (blackPlayerQueue.Count > 0)
        {
            PieceType nextPiece = blackPlayerQueue.Peek();
            bool added = AddPieceToHand(PlayerColor.BLACK, nextPiece);

            if (added)
            {
                blackPlayerQueue.Dequeue();
                Debug.Log($"Added queued {nextPiece} to Black player's hand");
            }
            else
            {
                Debug.Log($"Failed to add queued {nextPiece} to Black player's hand, putting it back in queue");
            }
        }
    }

    // Generate new pieces for both players at end of a full turn
    public PieceType GenerateNewPieces()
    {
        PieceType randomPieceType = (PieceType)Random.Range(0, 6);
        AddNewPiece(PlayerColor.WHITE, randomPieceType);
        AddNewPiece(PlayerColor.BLACK, randomPieceType);
        return randomPieceType;
    }

    public void HighlightWinningPositions(List<Vector2Int> positions)
    {
        foreach (Vector2Int pos in positions)
        {
            Renderer renderer = gridBoxes[pos.x, pos.y].GetComponent<Renderer>();
            Material originalMaterial = renderer.material;
            Color targetColor = Color.red;

            // Start a coroutine to slowly change the color
            StartCoroutine(ColorChangeAnimation(renderer, originalMaterial.color, targetColor, 0.3f));
        }
    }

    private System.Collections.IEnumerator ColorChangeAnimation(Renderer renderer, Color startColor, Color targetColor, float duration)
    {
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            renderer.material.color = Color.Lerp(startColor, targetColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        renderer.material.color = targetColor;
    }
}
