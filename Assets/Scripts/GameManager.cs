using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Board board;
    public GameStateMachine stateMachine;
    public PlayerColor playerColor = NetworkManager.Instance.IsMasterClient() ? PlayerColor.WHITE : PlayerColor.BLACK;

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

            // Initialize state machine
            stateMachine = new GameStateMachine();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        board = GameObject.Find("Board").GetComponent<Board>();
        stateMachine.Initialize(new InitState(stateMachine));
    }

    private void Update()
    {
        stateMachine.Update();
    }

    public void IncrementTurn()
    {
        currentTurn++;
        currentPlayer = currentPlayer == PlayerColor.WHITE ? PlayerColor.BLACK : PlayerColor.WHITE;
    }

    public void RenderValidMoves()
    {
        DestroyHitBoxes();

        if (selectedPiece == null) return;
        List<int[]> pieceValidMoves = selectedPiece.ValidMoves();

        if (pieceValidMoves == null) return;

        for (int i = 0; i < pieceValidMoves.Count; i++)
        {
            int[] move = pieceValidMoves[i];
            int row = move[0];
            int column = move[1];

            // Make sure indices are valid for the board
            if (row >= 0 && row < board.boardSize && column >= 0 && column < board.boardSize)
            {
                HitBox hitBox = board.hitBoxes[row, column];
                if (hitBox.GetPlayer() != selectedPiece.player)
                {
                    // Check if there's a piece to capture
                    bool isCapture = hitBox.GetPiece() != null && hitBox.GetPlayer() != PlayerColor.EMPTY;

                    // Don't allow capturing before round 3 (turn 6)
                    if (isCapture && currentTurn < 6)
                        continue;

                    board.hitBoxes[row, column].Render(isCapture);
                }
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
                        board.hitBoxes[row, column].Render(false);
                    }
                }
            }
        }
    }

    public void DestroyHitBoxes()
    {
        for (int row = 0; row < board.boardSize; row++)
        {
            for (int column = 0; column < board.boardSize; column++)
            {
                board.hitBoxes[row, column].Destroy();
            }
        }
    }

    public IEnumerator StartNewGame(float delay)
    {
        yield return new WaitForSeconds(delay);
        board.Reset();
        this.PlayAudio("game-start");

        stateMachine.ChangeState(new PlayerTurnState(stateMachine, PlayerColor.WHITE));
    }

    public void PlayAudio(string audioName)
    {
        AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            AudioClip moveSound = Resources.Load<AudioClip>($"Audio/{audioName}");
            if (moveSound != null)
                audioSource.PlayOneShot(moveSound);
        }
    }
}