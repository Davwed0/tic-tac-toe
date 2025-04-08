using UnityEngine;
using System.Collections.Generic;

public class Plane : MonoBehaviour
{
    [SerializeField] private GameObject quadPrefab; // The quad prefab to instance
    [SerializeField] private int grassCount = 1000; // Number of grass instances to generate
    [SerializeField] private float noiseScale = 10f; // Scale of the Perlin noise
    [SerializeField][Range(0f, 1f)] private float noiseThreshold = 0.5f; // Noise threshold for placement
    [SerializeField] private int maxAttempts = 100000; // Maximum attempts to prevent infinite loops
    [SerializeField] private Material quadMaterial;

    private List<Matrix4x4> matrices = new List<Matrix4x4>();
    private Mesh quadMesh;
    private bool positionsGenerated = false;
    private Terrain terrain;

    void Start()
    {
        if (quadPrefab == null)
        {
            Debug.LogError("Quad prefab is not assigned!");
            return;
        }

        terrain = GetComponent<Terrain>();
        if (terrain == null)
        {
            Debug.LogError("Terrain component not found!");
            return;
        }

        quadMesh = quadPrefab.transform.GetChild(0).GetComponent<MeshFilter>().sharedMesh;

        GenerateLattice();
        positionsGenerated = true;
    }

    void GenerateLattice()
    {
        // Get terrain size
        Vector3 terrainSize = terrain.terrainData.size;
        float terrainWidth = terrainSize.x;
        float terrainHeight = terrainSize.z;

        int attempts = 0;
        matrices.Clear();

        while (matrices.Count < grassCount && attempts < maxAttempts)
        {
            attempts++;

            // Generate random position within terrain bounds
            float x = Random.Range(0f, terrainWidth);
            float z = Random.Range(0f, terrainHeight);

            // Sample Perlin noise at this position
            float noiseValue = Mathf.PerlinNoise(x / noiseScale, z / noiseScale);

            // Skip if noise value doesn't meet threshold
            if (noiseValue < noiseThreshold)
                continue;

            // Convert to world position and sample terrain height
            Vector3 worldPos = terrain.transform.position + new Vector3(x, 0, z);
            worldPos.y = terrain.SampleHeight(worldPos);

            // Skip if collision detected
            if (GameManager.Instance.board.CheckCollisions(worldPos))
                continue;

            // Create random rotation and scale
            Quaternion rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            float scaleX = Random.Range(0.25f, 0.7f);
            float scaleY = Random.Range(0.3f, 1f);
            Vector3 scale = new Vector3(scaleX, scaleY, scaleX);

            // Adjust position to account for vertical scale
            Vector3 adjustedPosition = new Vector3(worldPos.x, worldPos.y + scaleY / 2f - 5, worldPos.z);
            Matrix4x4 matrix = Matrix4x4.TRS(adjustedPosition, rotation, scale);
            matrices.Add(matrix);
        }

        if (matrices.Count < grassCount)
        {
            Debug.LogWarning($"Only generated {matrices.Count} grass instances. " +
                "Consider increasing maxAttempts or lowering noiseThreshold.");
        }
    }

    void RenderLattice()
    {
        if (matrices.Count > 0)
        {
            const int batchSize = 1023;
            var meshBatch = new RenderParams(quadMaterial);
            for (int i = 0; i < matrices.Count; i += batchSize)
            {
                int count = Mathf.Min(batchSize, matrices.Count - i);
                Graphics.RenderMeshInstanced(meshBatch, quadMesh, 0, matrices.GetRange(i, count).ToArray());
            }
        }
    }

    void Update()
    {
        if (!positionsGenerated || quadMesh == null || quadMaterial == null)
            return;

        RenderLattice();
    }
}