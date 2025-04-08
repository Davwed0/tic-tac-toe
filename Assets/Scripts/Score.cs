using UnityEngine;

public class Score : MonoBehaviour
{
    public Mesh mesh0, mesh1, mesh2, mesh3, mesh4, mesh5, mesh6, mesh7, mesh8, mesh9;
    public Material whiteMaterial, blackMaterial;
    public int player;

    private Mesh selectedMesh;
    private int score = 0;

    void Start()
    {
        SetScoreModel();
        SetScoreColor();
    }

    void Update()
    {
        SetScoreModel();
        SetScoreColor();
    }

    private void SetScoreModel()
    {
        Mesh selectedMesh = score switch
        {
            0 => mesh0,
            1 => mesh1,
            2 => mesh2,
            3 => mesh3,
            4 => mesh4,
            5 => mesh5,
            6 => mesh6,
            7 => mesh7,
            8 => mesh8,
            9 => mesh9,
            _ => null
        };

        if (selectedMesh != null)
        {
            GetComponent<MeshFilter>().mesh = selectedMesh;
        }
    }

    public void IncrementScore()
    {
        score++;
    }

    public void SetScoreColor()
    {
        GetComponent<Renderer>().material = player switch
        {
            0 => whiteMaterial,
            1 => blackMaterial,
            _ => null
        };
    }
}
