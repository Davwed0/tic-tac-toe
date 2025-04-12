using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class MainMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Button playButton;

    private bool isConnecting = false;

    private void Start()
    {
        // Make sure we can use PhotonNetwork
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public void PlayGame()
    {
        // Start connection process
        isConnecting = true;

        // Update UI
        if (playButton != null)
            playButton.interactable = false;

        if (statusText != null)
            statusText.text = "Connecting to server...";

        // Connect to Photon network
        if (PhotonNetwork.IsConnected)
        {
            // Already connected, join a room
            JoinRandomRoom();
        }
        else
        {
            // Not connected, connect first
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void ExitGame()
    {
        // If connected, disconnect from Photon first
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();

        Application.Quit();
        Debug.Log("Game Exited");
    }

    // Helper method to join a random room
    private void JoinRandomRoom()
    {
        if (statusText != null)
            statusText.text = "Finding a game...";

        PhotonNetwork.JoinRandomRoom();
    }

    #region Photon Callbacks

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");

        if (isConnecting)
        {
            if (statusText != null)
                statusText.text = "Connected! Finding a game...";

            JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed to join random room. Creating new room: " + message);

        if (statusText != null)
            statusText.text = "Creating new game...";

        // Create a new room with max 2 players
        RoomOptions roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            IsVisible = true,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(null, roomOptions); // null creates a random room name
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created new room: " + PhotonNetwork.CurrentRoom.Name);

        if (statusText != null)
            statusText.text = "Room created. Waiting for opponent...";
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined room: " + PhotonNetwork.CurrentRoom.Name +
                  " with " + PhotonNetwork.CurrentRoom.PlayerCount + " players");

        if (statusText != null)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
                statusText.text = "Waiting for opponent to join...";
            else
                statusText.text = "Starting game...";
        }

        // If all players are here, load the game scene
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Only the master client loads the scene to ensure synchronization
                PhotonNetwork.LoadLevel("GameScene");
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player joined: " + newPlayer.NickName);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
        {
            if (statusText != null)
                statusText.text = "Starting game...";

            // Load the game scene when second player joins
            PhotonNetwork.LoadLevel("GameScene");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogWarning("Disconnected from Photon: " + cause);

        // Reset UI state
        isConnecting = false;

        if (playButton != null)
            playButton.interactable = true;

        if (statusText != null)
            statusText.text = "Disconnected: " + cause.ToString();
    }

    #endregion
}
