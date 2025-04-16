using UnityEngine;

public class HitBox : MonoBehaviour
{
    public PlayerColor player = PlayerColor.EMPTY;
    private int row, column;
    private GameObject tile;
    public bool isValid { get; set; } = false;

    // Add public properties to access row and column
    public int Row { get { return this.row; } }
    public int Col { get { return this.column; } }

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
            PlayerTurnState state = GameManager.Instance.stateMachine.CurrentState as PlayerTurnState;
            if (state != null)
            {
                state.PlacePiece(selectedPiece, this);
            }
        }
    }

    public void Render(bool isCapture = false)
    {
        GetComponent<Collider>().enabled = true;
        tile.GetComponent<Renderer>().enabled = true;
        isValid = true;

        if (player != PlayerColor.EMPTY)
        {
            tile.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
        }
    }

    public void Destroy()
    {
        GetComponent<Collider>().enabled = false;
        tile.GetComponent<Renderer>().enabled = false;
        isValid = false;
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
        isValid = false;
    }
}