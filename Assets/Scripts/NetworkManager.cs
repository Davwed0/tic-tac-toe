using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Configure Photon settings
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        // set player color
        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.playerColor = PlayerColor.WHITE;
        }
        else
        {
            GameManager.Instance.playerColor = PlayerColor.BLACK;
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from Photon: " + cause.ToString());
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public bool IsMasterClient()
    {
        return PhotonNetwork.IsMasterClient;
    }
}
