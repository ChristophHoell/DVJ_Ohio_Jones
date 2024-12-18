using UnityEngine;
using System.Collections.Generic;

public class MapObjectGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 50;
    public int mapHeight = 50;
    public float gridCellSize = 1f;
    
    [Header("Generation Control")]
    public bool generateOnStart = true;

    [Header("Prefabs")]
    public GameObject enemyType1Prefab;
    public GameObject enemyType2Prefab;
    public GameObject obstacle1Prefab;
    public GameObject obstacle2Prefab;
    public GameObject obstacle3Prefab;
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
    [Range(0, 300)]
    public int minObstacles = 10;
    [Range(0, 300)]
    public int maxObstacles = 20;

    private bool[,] occupiedTiles;
    private bool[,] pathTiles;
    private Transform objectsParent;
    private Vector2Int exitPosition;
    private Vector2Int treasurePosition;
    private const int MAX_GENERATION_ATTEMPTS = 3;
    private Vector2Int PLAYER_START = new Vector2Int(1, 1);

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateValidMap();
        }
    }

    public void GenerateValidMap()
    {
        int attempts = 0;
        bool validMapGenerated = false;

        while (!validMapGenerated && attempts < MAX_GENERATION_ATTEMPTS)
        {
            GenerateMap();
            validMapGenerated = ValidateMap();
            
            if (!validMapGenerated)
            {
                Debug.Log($"Generated invalid map (attempt {attempts + 1}/{MAX_GENERATION_ATTEMPTS}), regenerating...");
                if (objectsParent != null)
                {
                    Destroy(objectsParent.gameObject);
                }
            }
            attempts++;
        }

        if (!validMapGenerated)
        {
            Debug.LogError("Failed to generate a valid map after maximum attempts!");
        }
    }

    private void GenerateMap()
    {
        InitializeMap();
        PlaceKeyObjectsAndCreatePaths();
        PlaceRemainingObjects();
    }

    private void InitializeMap()
    {
        occupiedTiles = new bool[mapWidth, mapHeight];
        pathTiles = new bool[mapWidth, mapHeight];
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

        // Mark player start position
        occupiedTiles[PLAYER_START.x, PLAYER_START.y] = true;
        pathTiles[PLAYER_START.x, PLAYER_START.y] = true;
    }

    private void PlaceKeyObjectsAndCreatePaths()
    {
        exitPosition = PlaceObjectWithinDistance(exitPrefab, PLAYER_START, mapWidth/2, mapWidth - 2);
        CreateMeanderingPath(PLAYER_START, exitPosition);

        treasurePosition = PlaceObjectWithinDistance(treasurePrefab, PLAYER_START, mapWidth/2, mapWidth - 2);
        CreateMeanderingPath(PLAYER_START, treasurePosition);
    }

    private void CreateMeanderingPath(Vector2Int start, Vector2Int end)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Vector2Int current = start;
        
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(1, 0),
            new Vector2Int(0, -1),
            new Vector2Int(-1, 0)
        };

        while (current != end)
        {
            path.Add(current);
            pathTiles[current.x, current.y] = true;

            // Calculate direction to target
            Vector2Int dirToTarget = new Vector2Int(
                Mathf.Clamp(end.x - current.x, -1, 1),
                Mathf.Clamp(end.y - current.y, -1, 1)
            );

            // 70% chance to move in a random direction, 30% chance to move towards target
            Vector2Int nextDir;
            if (Random.value < 0.7f)
            {
                // Choose random direction
                nextDir = directions[Random.Range(0, directions.Length)];
            }
            else
            {
                // Move towards target
                nextDir = dirToTarget;
            }

            Vector2Int next = current + nextDir;

            // If the next position is invalid or occupied, try a different random direction
            int attempts = 0;
            while ((!IsValidPosition(next) || occupiedTiles[next.x, next.y]) && attempts < directions.Length)
            {
                nextDir = directions[Random.Range(0, directions.Length)];
                next = current + nextDir;
                attempts++;
            }

            // If we couldn't find a valid move, try moving directly towards target
            if (attempts >= directions.Length)
            {
                next = current + dirToTarget;
                if (!IsValidPosition(next) || occupiedTiles[next.x, next.y])
                {
                    // If we're stuck, break to prevent infinite loop
                    break;
                }
            }

            current = next;
            MarkPathArea(current);
        }
        
        // Make sure to mark the end position
        pathTiles[end.x, end.y] = true;
    }

    private void MarkPathArea(Vector2Int center)
    {
        // Only mark cardinal directions (not diagonals)
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 0),   // Center
            new Vector2Int(0, 1),   // Up
            new Vector2Int(0, -1),  // Down
            new Vector2Int(1, 0),   // Right
            new Vector2Int(-1, 0)   // Left
        };

        foreach (Vector2Int dir in directions)
        {
            Vector2Int tile = center + dir;
            if (IsValidPosition(tile))
            {
                pathTiles[tile.x, tile.y] = true;
            }
        }
    }

    private Vector2Int PlaceObjectWithinDistance(GameObject prefab, Vector2Int origin, int minDistance, int maxDistance)
    {
        List<Vector2Int> validPositions = new List<Vector2Int>();

        for (int x = 1; x < mapWidth - 1; x++)
        {
            for (int y = 1; y < mapHeight - 1; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                float distance = Vector2Int.Distance(origin, pos);
                if (distance >= minDistance && distance <= maxDistance && !occupiedTiles[x, y])
                {
                    validPositions.Add(pos);
                }
            }
        }

        if (validPositions.Count > 0)
        {
            Vector2Int chosen = validPositions[Random.Range(0, validPositions.Count)];
            PlaceObject(prefab, chosen);
            return chosen;
        }

        Debug.LogWarning($"Could not place {prefab.name} within distance constraints");
        return Vector2Int.zero;
    }

    private void PlaceRemainingObjects()
    {
        PlaceEnemies();
        PlaceObstacles();
    }

    private void PlaceEnemies()
    {
        int enemyType1Count = Random.Range(minEnemyType1, maxEnemyType1 + 1);
        int enemyType2Count = Random.Range(minEnemyType2, maxEnemyType2 + 1);

        for (int i = 0; i < enemyType1Count; i++)
            PlaceObjectInSafeLocation(enemyType1Prefab, 15);

        for (int i = 0; i < enemyType2Count; i++)
            PlaceObjectInSafeLocation(enemyType2Prefab, 15);
    }

    private void PlaceObstacles()
    {
        int totalObstacles = Random.Range(minObstacles, maxObstacles + 1);
        for (int i = 0; i < totalObstacles; i++)
        {
            GameObject obstaclePrefab = SelectObstaclePrefab();
            PlaceObjectInSafeLocation(obstaclePrefab, 1);
        }
    }

    private GameObject SelectObstaclePrefab()
    {
        float randomValue = Random.value;
        if (randomValue < 0.33f) return obstacle1Prefab;
        if (randomValue < 0.66f) return obstacle2Prefab;
        return obstacle3Prefab;
    }

    private void PlaceObjectInSafeLocation(GameObject prefab, int min)
    {
        int maxAttempts = 200;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            int x = Random.Range(min, mapWidth - 1);
            int y = Random.Range(min, mapHeight - 1);
            Vector2Int position = new Vector2Int(x, y);

            if (!occupiedTiles[x, y] && !pathTiles[x, y])
            {
                PlaceObject(prefab, position);
                break;
            }
            attempts++;
        }
    }

    private void PlaceObject(GameObject prefab, Vector2Int position)
    {
        Vector3 worldPos = GridToWorldPosition(position.x, position.y);
        GameObject obj = Instantiate(prefab, worldPos, Quaternion.identity, objectsParent);
        obj.name = $"{prefab.name} ({position.x}, {position.y})";
        occupiedTiles[position.x, position.y] = true;
    }

    private bool ValidateMap()
    {
        bool[,] mapCopy = new bool[mapWidth, mapHeight];
        
        // Only copy obstacles and boundaries, not paths
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                // Only consider a tile blocked if it's occupied AND not part of a path
                mapCopy[x, y] = occupiedTiles[x, y] && !pathTiles[x, y];
            }
        }

        // Ensure the exit and treasure positions are not marked as blocked
        mapCopy[exitPosition.x, exitPosition.y] = false;
        mapCopy[treasurePosition.x, treasurePosition.y] = false;

        bool canReachExit = CanReachDestination(PLAYER_START, exitPosition, mapCopy);
        if (!canReachExit)
        {
            Debug.Log("Cannot reach exit!");
            return false;
        }

        bool canReachTreasure = CanReachDestination(PLAYER_START, treasurePosition, mapCopy);
        if (!canReachTreasure)
        {
            Debug.Log("Cannot reach treasure!");
            return false;
        }
        
        return true;
    }

    private bool CanReachDestination(Vector2Int start, Vector2Int destination, bool[,] mapCopy)
    {
        if (start == destination) return true;
        
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        
        queue.Enqueue(start);
        visited.Add(start);

        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            if (current == destination) return true;

            foreach (Vector2Int dir in directions)
            {
                Vector2Int next = current + dir;
                if (IsValidPosition(next) && !mapCopy[next.x, next.y] && !visited.Contains(next))
                {
                    queue.Enqueue(next);
                    visited.Add(next);
                }
            }
        }

        return false;
    }

    private bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < mapWidth && 
               pos.y >= 0 && pos.y < mapHeight;
    }

    private Vector3 GridToWorldPosition(int x, int y)
    {
        float worldX = x * gridCellSize;
        float worldY = y * gridCellSize;
        return new Vector3(worldX, worldY, 0f);
    }
}