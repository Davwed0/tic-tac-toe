using UnityEngine;
using Photon.Pun;

public class HitBox : MonoBehaviour
{
    public PlayerColor player = PlayerColor.EMPTY;
    private int row, column;
    private GameObject tile;

    private void Start()
    {
        GetComponent<Collider>().enabled = false;
        tile = transform.GetChild(0).gameObject;
        tile.GetComponent<Renderer>().enabled = false;
    }

    public PlayerColor GetPlayer()
    {
        return player;
    }

    public void SetPlayer(PlayerColor newPlayer)
    {
        player = newPlayer;
    }

    public void OnMouseDown()
    {
        // In multiplayer, only allow placement during your turn
        if (PhotonNetwork.IsConnected && !NetworkManager.Instance.IsMyTurn())
            return;

        ChessPiece selectedPiece = GameManager.Instance.selectedPiece;

        if (selectedPiece != null)
        {
            // Create and execute the command
            ICommand command = new PlacePieceCommand(selectedPiece, this);
            GameManager.Instance.ExecuteCommand(command);
        }
    }

    public void Render(bool isCapture = false)
    {
        GetComponent<Collider>().enabled = true;
        tile.GetComponent<Renderer>().enabled = true;

        if (player != PlayerColor.EMPTY)
        {
            tile.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
        }
    }

    public void Destroy()
    {
        GetComponent<Collider>().enabled = false;
        tile.GetComponent<Renderer>().enabled = false;
    }

    public ChessPiece GetPiece()
    {
        return transform.childCount > 1 ? transform.GetChild(1).GetComponent<ChessPiece>() : null;
    }

    public void SetPosition(int row, int column)
    {
        this.row = row;
        this.column = column;
    }

    public int[] GetPosition()
    {
        return new int[] { row, column };
    }

    public void Reset()
    {
        player = PlayerColor.EMPTY;
    }
}