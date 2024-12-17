using UnityEngine;
using System.Collections.Generic;

public class MapObjectGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 50;
    public int mapHeight = 50;
    public float gridCellSize = 1f;  // Size of each grid cell in Unity units
    
    [Header("Generation Control")]
    public bool generateOnStart = true;

    [Header("Prefabs")]
    public GameObject enemyType1Prefab;
    public GameObject enemyType2Prefab;
    public GameObject obstacle1Prefab;
    public GameObject obstacle2Prefab;
    public GameObject exitPrefab;
    public GameObject treasurePrefab;

    [Header("Spawn Settings")]
    [Range(0, 50)]
    public int minEnemyType1 = 5;
    [Range(0, 50)]
    public int maxEnemyType1 = 10;
    [Range(0, 50)]
    public int minEnemyType2 = 3;
    [Range(0, 50)]
    public int maxEnemyType2 = 7;
    [Range(0, 100)]
    public int minObstacles = 10;
    [Range(0, 100)]
    public int maxObstacles = 20;

    private bool[,] occupiedTiles;
    private Transform objectsParent;

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateMap();
        }
    }

    public void GenerateMap()
    {
        if (objectsParent != null)
        {
            Destroy(objectsParent.gameObject);
        }

        InitializeMap();
        PlaceObjects();
    }

    private void InitializeMap()
    {
        occupiedTiles = new bool[mapWidth, mapHeight];
        objectsParent = new GameObject("Generated Objects").transform;

        // Mark border tiles as occupied
        for (int x = 0; x < mapWidth; x++)
        {
            occupiedTiles[x, 0] = true;
            occupiedTiles[x, mapHeight - 1] = true;
        }
        for (int y = 0; y < mapHeight; y++)
        {
            occupiedTiles[0, y] = true;
            occupiedTiles[mapWidth - 1, y] = true;
        }
    }

    private void PlaceObjects()
    {
        // Place exit and treasure first (one of each)
        PlaceObjectRandomly(exitPrefab);
        PlaceObjectRandomly(treasurePrefab);

        // Place random number of enemies
        int enemyType1Count = Random.Range(minEnemyType1, maxEnemyType1 + 1);
        int enemyType2Count = Random.Range(minEnemyType2, maxEnemyType2 + 1);
        
        for (int i = 0; i < enemyType1Count; i++)
            PlaceObjectRandomly(enemyType1Prefab);
        
        for (int i = 0; i < enemyType2Count; i++)
            PlaceObjectRandomly(enemyType2Prefab);

        // Place random number of obstacles
        int totalObstacles = Random.Range(minObstacles, maxObstacles + 1);
        for (int i = 0; i < totalObstacles; i++)
        {
            GameObject obstaclePrefab = Random.value < 0.5f ? obstacle1Prefab : obstacle2Prefab;
            PlaceObjectRandomly(obstaclePrefab);
        }
    }

    private Vector3 GridToWorldPosition(int x, int y)
    {
        // Convert grid coordinates to world position, centered in the grid cell
        float worldX = x * gridCellSize;
        float worldY = y * gridCellSize;
        return new Vector3(worldX, worldY, 0f);
    }

    private void PlaceObjectRandomly(GameObject prefab)
    {
        int maxAttempts = 100;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            // Get random grid position (excluding border)
            int gridX = Random.Range(1, mapWidth - 1);
            int gridY = Random.Range(1, mapHeight - 1);

            if (!occupiedTiles[gridX, gridY])
            {
                // Convert grid position to world position
                Vector3 worldPos = GridToWorldPosition(gridX, gridY);
                
                // Instantiate object aligned to grid
                GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, objectsParent);
                obj.name = $"{prefab.name} ({gridX}, {gridY})";
                
                // Ensure the object is perfectly aligned to the grid
                obj.transform.position = worldPos;
                
                // Mark tile as occupied
                occupiedTiles[gridX, gridY] = true;
                break;
            }

            attempts++;
        }

        if (attempts >= maxAttempts)
            Debug.LogWarning($"Could not place {prefab.name} after {maxAttempts} attempts");
    }
}