using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class NewGameState : GameState, IOnEventCallback
{
    private const byte SEND_BOARD_EVENT = 1;

    public NewGameState(GameStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        Debug.Log("Entering New State");

        // Register for Photon events
        PhotonNetwork.AddCallbackTarget(this);

        if (NetworkManager.Instance.IsMasterClient())
        {
            Debug.Log("Master client detected. Loading game scene.");
            GameManager.Instance.board.Reset();

            GameManager.Instance.board.GenerateNewPieces();
            // Serialize and send piece data to other clients
            SendBoardData();

            PlayStartSound();

            // Master client can proceed immediately
            stateMachine.ChangeState(new PlayerTurnState(stateMachine, PlayerColor.WHITE));
        }
        else
        {
            Debug.Log("Not master client. Waiting for game to start.");

            PlayStartSound();
        }
    }

    private void SendBoardData()
    {
        List<object> allBoardData = new List<object>();

        for (int playerIdx = 0; playerIdx < 2; playerIdx++)
        {
            for (int handIdx = 0; handIdx < GameManager.Instance.board.handSize; handIdx++)
            {
                ChessPiece piece = GameManager.Instance.board.chessPieces[handIdx, playerIdx];
                if (piece != null)
                {
                    object[] boardData = new object[3];
                    boardData[0] = (int)piece.player;
                    boardData[1] = piece.index;
                    boardData[2] = (int)piece.pieceType;

                    allBoardData.Add(boardData);
                }
            }
        }

        // Send the data through Photon
        object[] data = allBoardData.ToArray();
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others };
        PhotonNetwork.RaiseEvent(SEND_BOARD_EVENT, data, raiseEventOptions, SendOptions.SendReliable);

        Debug.Log("Sent board data to other clients: " + data.Length + " pieces");
        foreach (object boardDataObj in data)
        {
            object[] boardData = (object[])boardDataObj;
            Debug.Log($"Piece Data: Player: {boardData[0]}, Index: {boardData[1]}, Type: {boardData[2]}");
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == SEND_BOARD_EVENT && !NetworkManager.Instance.IsMasterClient())
        {
            Debug.Log("Received board data from master client");
            object[] allBoardData = (object[])photonEvent.CustomData;

            Debug.Log("Received board data: " + allBoardData.Length + " pieces");
            foreach (object pieceDataObj in allBoardData)
            {
                object[] pieceData = (object[])pieceDataObj;
                Debug.Log($"Piece Data: Player: {pieceData[0]}, Index: {pieceData[1]}, Type: {pieceData[2]}");
            }

            foreach (object pieceDataObj in allBoardData)
            {
                object[] pieceData = (object[])pieceDataObj;
                PlayerColor playerColor = (PlayerColor)(int)pieceData[0];
                int handIndex = (int)pieceData[1];
                PieceType pieceType = (PieceType)(int)pieceData[2];

                float hitBoxSize = GameManager.Instance.board.hitBoxPrefab.transform.localScale.x;
                float totalGap = hitBoxSize + GameManager.Instance.board.hitBoxGap;
                float offsetX = GameManager.Instance.board.boardSize * totalGap / 2 + GameManager.Instance.board.pad;
                float offsetY = GameManager.Instance.board.handSize * hitBoxSize / 2;

                int playerIdx = (int)playerColor;
                float yPos = playerIdx == 0 ?
                    handIndex * hitBoxSize - offsetY :
                    (GameManager.Instance.board.handSize - handIndex - 1) * hitBoxSize - offsetY;

                float xPos = (playerIdx == 0 ? 1 : -1) * offsetX;
                float yPositionOffset = playerIdx == 0 ? 2 : 0;

                GameManager.Instance.board.chessPieces[handIndex, playerIdx] =
                    GameManager.Instance.board.CreatePiece(xPos, yPos + yPositionOffset, pieceType, handIndex, playerColor);
            }

            stateMachine.ChangeState(new PlayerTurnState(stateMachine, PlayerColor.WHITE));

            PhotonNetwork.RemoveCallbackTarget(this);
        }
    }

    private void PlayStartSound()
    {
        AudioSource audioSource = Camera.main.GetComponent<AudioSource>();
        if (audioSource != null)
        {
            AudioClip moveSound = Resources.Load<AudioClip>("Audio/game-start");
            if (moveSound != null)
                audioSource.PlayOneShot(moveSound);
        }
    }

    public override void Exit()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }
}
