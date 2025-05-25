using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileGridManager : MonoBehaviour
{
    public static TileGridManager Instance;

    public int Width;
    public int Height;
    public GameObject TilePrefab;

    private Tile[,] tiles;
    public float cubeSize;

    public bool IsInitialized { get; private set; } = false;
    private Dictionary<Vector2Int, Tile> tileMap = new();

    private void Awake()
    {
        Instance = this;
        tiles = new Tile[Width, Height];
        GenerateGrid();
    }

    void GenerateGrid()
    {
        GameObject mapParent = new GameObject("Map");

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                Vector3 worldPos = new Vector3(x * cubeSize, 0, z * cubeSize);
                GameObject go = Instantiate(TilePrefab, worldPos, Quaternion.identity, mapParent.transform);
                Tile tile = go.GetComponent<Tile>();
                tile.Init(x, z);

                tiles[x, z] = tile;
                tileMap[new Vector2Int(x, z)] = tile;
            }
        }

        TileCounter.Instance.Init(Width * Height);
        IsInitialized = true;
    }

    public Tile GetTile(int x, int z)
    {
        if (x < 0 || x >= Width || z < 0 || z >= Height)
            return null;

        return tiles[x, z];
    }

    public Tile GetTileFast(int x, int z)
    {
        tileMap.TryGetValue(new Vector2Int(x, z), out var tile);
        return tile;
    }

    public bool CanPlaceTurret(int startX, int startZ, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Tile tile = GetTile(startX + x, startZ + z);
                if (tile == null || tile.IsOccupied || tile.ColorState == TileColorState.Enemy)
                    return false;
            }
        }
        return true;
    }

    public List<Tile> GetTilesInRange(Vector3 worldPos, float range)
    {
        List<Tile> result = new();

        int minX = Mathf.FloorToInt((worldPos.x - range) / cubeSize);
        int maxX = Mathf.FloorToInt((worldPos.x + range) / cubeSize);
        int minZ = Mathf.FloorToInt((worldPos.z - range) / cubeSize);
        int maxZ = Mathf.FloorToInt((worldPos.z + range) / cubeSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                Tile tile = GetTile(x, z);
                if (tile == null) continue;

                Vector3 tilePos = new Vector3((x + 0.5f) * cubeSize, 0, (z + 0.5f) * cubeSize);
                Vector3 diff = tilePos - worldPos;

                if (diff.sqrMagnitude > range * range) continue;

                result.Add(tile);
            }
        }

        return result;
    }

    public static Vector3 GetWorldPositionFromGrid(int x, int z)
    {
        float worldX = x * Instance.cubeSize;
        float worldZ = z * Instance.cubeSize;
        Debug.Log("cubesize : " + TileGridManager.Instance.cubeSize);
        return new Vector3(worldX, TileGridManager.Instance.cubeSize, worldZ);
    }

    public static Vector2Int GetCenterGrid()
    {
        int cx = Mathf.Clamp(Instance.Width / 2, 0, Instance.Width - 1);
        int cz = Mathf.Clamp(Instance.Height / 2, 0, Instance.Height - 1);

        return new Vector2Int(cx, cz);
    }

    public static Vector3[] GetSpawnPositions()
    {
        int midX = Mathf.Clamp(Instance.Width / 2, 0, Instance.Width - 1);
        int midZ = Mathf.Clamp(Instance.Height / 2, 0, Instance.Height - 1);

        return new Vector3[]
        {
            GetWorldPositionFromGrid(midX, 0),                     // ÇÏ´Ü Áß¾Ó
            GetWorldPositionFromGrid(midX, Instance.Height - 1),   // »ó´Ü Áß¾Ó
            GetWorldPositionFromGrid(0, midZ),                     // ÁÂÃø Áß¾Ó
            GetWorldPositionFromGrid(Instance.Width - 1, midZ),    // ¿ìÃø Áß¾Ó
        };
    }
}
