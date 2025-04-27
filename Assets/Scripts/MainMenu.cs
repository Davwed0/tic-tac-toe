using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
using ExitGames.Client.Photon;

public class MainMenu : MonoBehaviourPunCallbacks
{
    public TMP_Text statusText1;
    public TMP_Text statusText2;
    private int selectedBoardSize = 3;
    private int selectedWinLength = 3;
    public TMP_Dropdown boardSizeDropdown;
    public TMP_Dropdown winLengthDropdown;
    private bool isHosting = false;

    private void Start()
    {
        if (boardSizeDropdown != null)
        {
            boardSizeDropdown.onValueChanged.AddListener(SetBoardSize);
        }
        if (winLengthDropdown != null)
        {
            winLengthDropdown.onValueChanged.AddListener(SetWinLength);
        }
    }

    public void SetBoardSize(int dropdownValue)
    {
        switch (dropdownValue)
        {
            case 0:
                selectedBoardSize = 3;
                break;
            case 1:
                selectedBoardSize = 4;
                break;
            case 2:
                selectedBoardSize = 5;
                break;
            default:
                selectedBoardSize = 3;
                break;
        }
        Debug.Log($"Board size set to {selectedBoardSize}x{selectedBoardSize}");
    }

    public void SetWinLength(int dropdownValue)
    {
        switch (dropdownValue)
        {
            case 0:
                selectedWinLength = 3;
                break;
            case 1:
                selectedWinLength = 4;
                break;
            default:
                selectedWinLength = 3;
                break;
        }
        Debug.Log($"Win length set to {selectedWinLength}");
    }

    // Base connection method
    private void ConnectToServer(string statusMessage)
    {
        if (statusText1 != null && statusText2 != null)
        {
            statusText1.text = statusMessage;
            statusText2.text = statusMessage;
        }
        PhotonNetwork.ConnectUsingSettings();
    }

    // Host button should call this method
    public void HostGame()
    {
        isHosting = true;
        ConnectToServer("Connecting to server to host game...");
    }

    // Join button should call this method
    public void JoinGame()
    {
        isHosting = false;
        ConnectToServer("Connecting to server to join game...");
    }

    public override void OnConnectedToMaster()
    {
        if (isHosting)
        {
            if (statusText1 != null && statusText2 != null)
            {
                statusText1.text = "Creating new room...";
                statusText2.text = "Creating new room...";
            }

            ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable();
            customRoomProperties.Add("boardSize", selectedBoardSize);
            customRoomProperties.Add("winLength", selectedWinLength);

            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 2;
            roomOptions.CustomRoomProperties = customRoomProperties;
            roomOptions.CustomRoomPropertiesForLobby = new string[] { "boardSize", "winLength" };

            PhotonNetwork.CreateRoom(null, roomOptions);
        }
        else
        {
            if (statusText1 != null && statusText2 != null)
            {
                statusText1.text = "Connected! Joining random room...";
                statusText2.text = "Connected! Joining random room...";
            }
            PhotonNetwork.JoinRandomRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (statusText1 != null && statusText2 != null)
        {
            statusText1.text = "No rooms available. Try hosting a game instead!";
            statusText2.text = "No rooms available. Try hosting a game instead!";
        }

        // Start a coroutine to retry joining a room every 5 seconds
        StartCoroutine(RetryJoiningRandomRoom());
    }

    private System.Collections.IEnumerator RetryJoiningRandomRoom()
    {
        WaitForSeconds waitTime = new WaitForSeconds(5f);
        while (!PhotonNetwork.IsConnected || PhotonNetwork.CountOfRooms == 0)
        {
            if (statusText1 != null && statusText2 != null)
            {
                statusText1.text = "No rooms found. Retrying in 5 seconds...";
                statusText2.text = "No rooms found. Retrying in 5 seconds...";
            }
            yield return waitTime;

            if (PhotonNetwork.IsConnected)
            {
                if (statusText1 != null && statusText2 != null)
                {
                    statusText1.text = "Trying to join a room...";
                    statusText2.text = "Trying to join a room...";
                }
                PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                ConnectToServer("Reconnecting to server...");
                break;
            }
        }
    }

    public override void OnJoinedRoom()
    {
        if (statusText1 != null && statusText2 != null)
        {
            statusText1.text = "Waiting for another player...";
            statusText2.text = "Waiting for another player...";
        }

        if (!PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("boardSize") && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("winLength"))
        {
            selectedBoardSize = (int)PhotonNetwork.CurrentRoom.CustomProperties["boardSize"];
            Debug.Log($"Joined room with board size: {selectedBoardSize}x{selectedBoardSize}");

            selectedWinLength = (int)PhotonNetwork.CurrentRoom.CustomProperties["winLength"];
            Debug.Log($"Joined room with win length: {selectedWinLength}");
        }

        PlayerPrefs.SetInt("boardSize", selectedBoardSize);
        PlayerPrefs.SetInt("winLength", selectedWinLength);
        PlayerPrefs.Save();

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("GameScene");
            }
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("GameScene");
            }
        }
    }

    public void ExitGame()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
        }
        Application.Quit();
        Debug.Log("Game Exited");
    }
}
