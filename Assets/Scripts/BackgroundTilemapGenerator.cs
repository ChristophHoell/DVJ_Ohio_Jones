using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class BackgroundTilemapGenerator : MonoBehaviour
{
    [System.Serializable]
    public class TileWithDistribution
    {
        public TileBase tile;
        [HideInInspector] public float weight;
        [HideInInspector] public float minNoiseThreshold;
        [HideInInspector] public float maxNoiseThreshold;
        [HideInInspector] public int desiredCoverage; // Approximate percentage of map to cover
    }

    [Header("Tilemap Settings")]
    public Tilemap targetTilemap;
    public string sortingLayerName = "Background";
    public int sortingOrder = 0;
    
    [Header("Map Dimensions")]
    public int mapWidth = 50;
    public int mapHeight = 50;

    [Header("Tiles")]
    public TileWithDistribution[] tiles;

    [Header("Distribution Settings")]
    [Range(0.1f, 50f)]
    public float noiseScale = 20f;
    [Range(0f, 1f)]
    public float randomness = 0.3f;
    [Range(0f, 1f)]
    public float clusteringFactor = 0.5f; // Higher values create more distinct regions

    private float noiseOffsetX;
    private float noiseOffsetY;
    private Dictionary<TileBase, int> tileCounts = new Dictionary<TileBase, int>();

    private void Start()
    {
        RandomizeNoiseOffset();
        AutoCalculateDistribution();
        GenerateBackground();
    }

    private void AutoCalculateDistribution()
    {
        if (tiles.Length == 0) return;

        // Calculate base coverage ensuring total is less than 100%
        int baseCoverage = Mathf.FloorToInt(95f / tiles.Length);
        float remainingCoverage = 95f - (baseCoverage * tiles.Length);
        
        // Distribute weights and thresholds
        float thresholdStep = 1f / tiles.Length;
        
        for (int i = 0; i < tiles.Length; i++)
        {
            // Randomize the coverage slightly around the base value
            tiles[i].desiredCoverage = baseCoverage + 
                (i == tiles.Length - 1 ? Mathf.FloorToInt(remainingCoverage) : 0);
            
            // Assign weight based on desired coverage
            tiles[i].weight = tiles[i].desiredCoverage / 100f;

            // Create overlapping noise thresholds for more natural transitions
            tiles[i].minNoiseThreshold = Mathf.Max(0f, (i * thresholdStep) - (thresholdStep * clusteringFactor));
            tiles[i].maxNoiseThreshold = Mathf.Min(1f, ((i + 1) * thresholdStep) + (thresholdStep * clusteringFactor));
        }
    }

    public void GenerateBackground()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("No tilemap assigned!");
            return;
        }

        // Setup tilemap
        targetTilemap.GetComponent<TilemapRenderer>().sortingLayerName = sortingLayerName;
        targetTilemap.GetComponent<TilemapRenderer>().sortingOrder = sortingOrder;

        // Clear existing tiles and counts
        targetTilemap.ClearAllTiles();
        tileCounts.Clear();
        foreach (var tileInfo in tiles)
        {
            tileCounts[tileInfo.tile] = 0;
        }

        int totalTiles = mapWidth * mapHeight;
        List<Vector3Int> unfilledPositions = new List<Vector3Int>();

        // First pass: Place tiles based on noise and weights
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3Int tilePosition = new Vector3Int(x, y, 0);
                
                float noiseValue = GetNoiseValueForPosition(x, y);
                TileBase selectedTile = SelectTileForPosition(noiseValue);
                
                if (selectedTile != null)
                {
                    targetTilemap.SetTile(tilePosition, selectedTile);
                    tileCounts[selectedTile]++;
                }
                else
                {
                    unfilledPositions.Add(tilePosition);
                }
            }
        }

        // Second pass: Adjust distribution to meet desired coverage
        foreach (var tileInfo in tiles)
        {
            int desiredCount = Mathf.RoundToInt((tileInfo.desiredCoverage / 100f) * totalTiles);
            int currentCount = tileCounts[tileInfo.tile];
            
            if (currentCount < desiredCount && unfilledPositions.Count > 0)
            {
                int needToAdd = desiredCount - currentCount;
                needToAdd = Mathf.Min(needToAdd, unfilledPositions.Count);

                for (int i = 0; i < needToAdd; i++)
                {
                    int randomIndex = Random.Range(0, unfilledPositions.Count);
                    targetTilemap.SetTile(unfilledPositions[randomIndex], tileInfo.tile);
                    unfilledPositions.RemoveAt(randomIndex);
                }
            }
        }
    }

    private float GetNoiseValueForPosition(int x, int y)
    {
        float noiseValue = Mathf.PerlinNoise(
            (x + noiseOffsetX) / noiseScale, 
            (y + noiseOffsetY) / noiseScale
        );
        
        return Mathf.Lerp(noiseValue, Random.value, randomness);
    }

    private TileBase SelectTileForPosition(float noiseValue)
    {
        List<TileWithDistribution> validTiles = new List<TileWithDistribution>();
        float totalWeight = 0f;

        // Find all tiles valid for this noise value
        foreach (var tileInfo in tiles)
        {
            if (noiseValue >= tileInfo.minNoiseThreshold && 
                noiseValue <= tileInfo.maxNoiseThreshold)
            {
                validTiles.Add(tileInfo);
                totalWeight += tileInfo.weight;
            }
        }

        if (validTiles.Count == 0) return null;

        // Select a tile based on weights
        float random = Random.value * totalWeight;
        float currentTotal = 0f;

        foreach (var tileInfo in validTiles)
        {
            currentTotal += tileInfo.weight;
            if (random <= currentTotal)
            {
                return tileInfo.tile;
            }
        }

        return validTiles[validTiles.Count - 1].tile;
    }

    public void RandomizeNoiseOffset()
    {
        noiseOffsetX = Random.Range(-10000f, 10000f);
        noiseOffsetY = Random.Range(-10000f, 10000f);
    }

    public void RegenerateWithNewDistribution()
    {
        RandomizeNoiseOffset();
        AutoCalculateDistribution();
        GenerateBackground();
    }

#if UNITY_EDITOR
    [ContextMenu("Debug Distribution")]
    private void DebugDistribution()
    {
        foreach (var tileInfo in tiles)
        {
            Debug.Log($"Tile: {tileInfo.tile.name}" +
                      $"\nWeight: {tileInfo.weight}" +
                      $"\nNoise Range: {tileInfo.minNoiseThreshold} to {tileInfo.maxNoiseThreshold}" +
                      $"\nDesired Coverage: {tileInfo.desiredCoverage}%");
        }
    }
#endif
}