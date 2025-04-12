using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    // Define custom properties keys
    public const string PLAYER_COLOR_PROP = "PlayerColor";
    public const string GAME_STARTED_PROP = "GameStarted";

    // Event codes for custom events
    public const byte PLACE_PIECE_EVENT = 1;
    public const byte GENERATE_PIECES_EVENT = 2;
    public const byte RESET_BOARD_EVENT = 3;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Register for custom Photon events
            PhotonNetwork.NetworkingClient.EventReceived += OnEventReceived;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // Unregister event handler when destroyed
        if (PhotonNetwork.NetworkingClient != null)
            PhotonNetwork.NetworkingClient.EventReceived -= OnEventReceived;
    }

    public void OnEventReceived(EventData photonEvent)
    {
        if (photonEvent.Code == PLACE_PIECE_EVENT)
        {
            // Handle piece placement event
            object[] data = (object[])photonEvent.CustomData;
            HandleRemotePiecePlacement(data);
        }
        else if (photonEvent.Code == GENERATE_PIECES_EVENT)
        {
            // Handle piece generation event
            object[] data = (object[])photonEvent.CustomData;
            HandleRemotePieceGeneration(data);
        }
        else if (photonEvent.Code == RESET_BOARD_EVENT)
        {
            // Handle board reset event
            GameManager.Instance.board.Reset();
        }
    }

    private void HandleRemotePiecePlacement(object[] data)
    {
        if (GameManager.Instance == null || GameManager.Instance.board == null)
            return;

        int pieceIndex = (int)data[0];
        int playerColorInt = (int)data[1];
        int row = (int)data[2];
        int column = (int)data[3];

        PlayerColor playerColor = (PlayerColor)playerColorInt;

        // Only process if we're not the sender
        if (GetLocalPlayerColor() != playerColor)
        {
            ChessPiece piece = GameManager.Instance.board.chessPieces[pieceIndex, playerColorInt];
            if (piece != null)
            {
                HitBox targetHitBox = GameManager.Instance.board.hitBoxes[row, column];

                // Simple placement without command pattern for remote events
                // We're just updating the local state to match the network event

                // Handle capturing
                ChessPiece capturedPiece = targetHitBox.GetPiece();
                if (capturedPiece != null)
                {
                    int capturedIndex = capturedPiece.index;
                    PlayerColor capturedPlayer = capturedPiece.player;
                    GameManager.Instance.board.chessPieces[capturedIndex, (int)capturedPlayer] = null;
                    Destroy(capturedPiece.gameObject);
                    GameManager.Instance.board.CheckPieceQueues();
                }

                // Place the piece
                piece.isOnBoard = true;
                piece.transform.position = targetHitBox.transform.position;
                piece.transform.SetParent(targetHitBox.transform);
                targetHitBox.player = playerColor;

                // Ensure turn updates locally
                GameManager.Instance.currentPlayer = playerColor == PlayerColor.WHITE ?
                    PlayerColor.BLACK : PlayerColor.WHITE;
                GameManager.Instance.currentTurn++;
            }
        }
    }

    private void HandleRemotePieceGeneration(object[] data)
    {
        if (GameManager.Instance == null || GameManager.Instance.board == null)
            return;

        // Generate pieces for both players as received from master client
        for (int i = 0; i < data.Length; i += 2)
        {
            int playerIndex = (int)data[i];
            int pieceType = (int)data[i + 1];

            PlayerColor player = (PlayerColor)playerIndex;
            PieceType type = (PieceType)pieceType;

            GameManager.Instance.board.AddNewPiece(player, type);
        }
    }

    public void SendPlacePieceEvent(ChessPiece piece, HitBox hitBox)
    {
        if (!PhotonNetwork.IsConnected)
            return;

        int pieceIndex = piece.index;
        int playerColorInt = (int)piece.player;
        int[] hitBoxPos = hitBox.GetPosition();

        object[] content = new object[] { pieceIndex, playerColorInt, hitBoxPos[0], hitBoxPos[1] };

        // Send event to all players including ourselves
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(PLACE_PIECE_EVENT, content, raiseEventOptions, sendOptions);
    }

    public void SendGeneratePiecesEvent(PieceType whitePieceType, PieceType blackPieceType)
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.IsMasterClient)
            return;

        object[] content = new object[] {
            (int)PlayerColor.WHITE, (int)whitePieceType,
            (int)PlayerColor.BLACK, (int)blackPieceType
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(GENERATE_PIECES_EVENT, content, raiseEventOptions, sendOptions);
    }

    public void SendResetBoardEvent()
    {
        if (!PhotonNetwork.IsConnected || !PhotonNetwork.IsMasterClient)
            return;

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.All
        };
        SendOptions sendOptions = new SendOptions { Reliability = true };

        PhotonNetwork.RaiseEvent(RESET_BOARD_EVENT, null, raiseEventOptions, sendOptions);
    }

    public void SetPlayerProperties()
    {
        // Set player color based on join order
        if (PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
        {
            // Master client is WHITE, other player is BLACK
            int colorValue = PhotonNetwork.IsMasterClient ? (int)PlayerColor.WHITE : (int)PlayerColor.BLACK;
            PhotonNetwork.LocalPlayer.SetCustomProperties(
                new ExitGames.Client.Photon.Hashtable { { PLAYER_COLOR_PROP, colorValue } }
            );

            Debug.Log($"Set local player color to: {(PlayerColor)colorValue}");
        }
    }

    public PlayerColor GetLocalPlayerColor()
    {
        if (!PhotonNetwork.IsConnected || PhotonNetwork.LocalPlayer == null)
            return PlayerColor.WHITE; // Default in single player

        if (PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue(PLAYER_COLOR_PROP, out object colorObj))
        {
            return (PlayerColor)(int)colorObj;
        }

        // Default based on master client status if not explicitly set
        return PhotonNetwork.IsMasterClient ? PlayerColor.WHITE : PlayerColor.BLACK;
    }

    public bool IsMyTurn()
    {
        return GameManager.Instance.currentPlayer == GetLocalPlayerColor();
    }

    // Synchronize initial game state for late joiners
    public void SyncGameState()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // Set the current game state in room properties
        PhotonNetwork.CurrentRoom.SetCustomProperties(new ExitGames.Client.Photon.Hashtable
        {
            { GAME_STARTED_PROP, true }
        });
    }

    // Handle player disconnection
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"Player {otherPlayer.NickName} left the room");

        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            // We could implement handling for when a player leaves
            // For example, pause the game or show a waiting message
            Debug.Log("Waiting for another player to join...");
        }
    }
}
