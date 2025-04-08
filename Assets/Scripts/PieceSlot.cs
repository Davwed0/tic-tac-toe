using UnityEngine;

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

        if (GameManager.Instance.currentPlayer == player) GetComponent<Renderer>().material = hoverMaterial;
    }
}
