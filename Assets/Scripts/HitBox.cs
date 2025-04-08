using UnityEngine;

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