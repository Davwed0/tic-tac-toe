using UnityEngine;
using Photon.Pun;

public class PieceSlot : MonoBehaviour
{
    public Material whiteMaterial, blackMaterial, hoverMaterial;
    public PlayerColor player;

    void Update()
    {
        SetSlotColor();
    }

    private void SetSlotColor()
    {
        GetComponent<Renderer>().material = player switch
        {
            PlayerColor.WHITE => whiteMaterial,
            PlayerColor.BLACK => blackMaterial,
            _ => null
        };

        // In multiplayer, highlight only the current player's slot if it's their turn
        if (PhotonNetwork.IsConnected)
        {
            if (GameManager.Instance.currentPlayer == player &&
                NetworkManager.Instance.GetLocalPlayerColor() == player)
            {
                GetComponent<Renderer>().material = hoverMaterial;
            }
        }
        else
        {
            // Single player behavior
            if (GameManager.Instance.currentPlayer == player)
                GetComponent<Renderer>().material = hoverMaterial;
        }
    }
}
