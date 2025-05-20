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

    void Awake()
    {
        Instance = this;
        tiles = new Tile[Width, Height];
        StartCoroutine( GenerateGrid());

       /* // test
        tiles[0, 0].SetColor(TileColorState.Player);
        tiles[10, 10].SetColor(TileColorState.Enemy);
        tiles[20, 20].SetColor(TileColorState.Player);*/
    }

    IEnumerator GenerateGrid()
    {
        GameObject mapParent = new GameObject("Map");

        int createdCnt = 0;
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

                // 프레임 나눠서 처리
                createdCnt++;
                if (createdCnt % 50 == 0)
                    yield return null;
            }
        }
        IsInitialized = true;
    }

    public Tile GetTile(int x, int z)
    {
        if (x < 0 || x >= Width || z < 0 || z >= Height) return null;
        return tiles[x, z];
    }

    public Tile GetTileFast(int x, int z)
    {
        tileMap.TryGetValue(new Vector2Int(x, z), out var tile);
        return tile;
    }


    public bool CanPlaceTurret(int startX, int startZ, int width, int height)
    {
        for(int x = 0; x < width; x++)
        {
            for(int z = 0; z < height; z++)
            {
                Tile tile = GetTile(startX + x, startZ +z);

                // tile 이 점유x ||  타일의 현재색이 enemy =>  return false;
                if (tile == null || tile.IsOccupied || tile.ColorState == TileColorState.Enemy)
                    return false;
            }
        }
        return true;
    }

    public List<Tile> GetTilesInRange(Vector3 worldPos, float range)
    {
        List<Tile> result = new();

        float halfRange = range;
        int minX = Mathf.FloorToInt((worldPos.x - halfRange) / cubeSize);
        int maxX = Mathf.FloorToInt((worldPos.x + halfRange) / cubeSize);
        int minZ = Mathf.FloorToInt((worldPos.z - halfRange) / cubeSize);
        int maxZ = Mathf.FloorToInt((worldPos.z + halfRange) / cubeSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                Tile tile = GetTile(x, z); // 배열 접근
                if (tile == null) continue;

                Vector3 tilePos = new Vector3((x + 0.5f) * cubeSize, 0, (z + 0.5f) * cubeSize);
                Vector3 diff = tilePos - worldPos;


                if (diff.x * diff.x + diff.z * diff.z > range * range) continue;

                result.Add(tile);
            }
        }

        return result;
    }
}

