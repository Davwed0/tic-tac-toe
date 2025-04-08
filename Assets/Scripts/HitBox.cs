using UnityEngine;

public class HitBox : MonoBehaviour
{
    private PlayerColor player = PlayerColor.EMPTY;
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

    public void OnMouseDown()
    {
        if ((GameManager.Instance.selectedPiece != null && player == PlayerColor.EMPTY))
        {
            if (GameManager.Instance.selectedPiece.transform.parent.name != "Board")
            {
                GameManager.Instance.selectedPiece.transform.parent.GetComponent<HitBox>().player = PlayerColor.EMPTY;
            }
            player = GameManager.Instance.currentPlayer;
            GameManager.Instance.selectedPiece.PlacePiece(this);
        }
        else if (this.GetPiece() != null || GameManager.Instance.selectedPiece != null)
        {
            ChessPiece eaten = this.GetPiece();
            GameManager.Instance.selectedPiece.transform.parent.GetComponent<HitBox>().player = PlayerColor.EMPTY;

            GameManager.Instance.board.MovePieceToHand(eaten, eaten.player);
            eaten.GetComponent<Collider>().enabled = false;

            player = GameManager.Instance.currentPlayer;
            GameManager.Instance.selectedPiece.PlacePiece(this);
        }
    }

    public void Render()
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