using UnityEngine;

public class TileGridManager : MonoBehaviour
{
    public static TileGridManager Instance;
    public int Width = 50;
    public int Height = 50;
    public GameObject TilePrefab;

    private Tile[,] tiles;
    public float cubeSize;

    void Awake()
    {
        Instance = this;
        tiles = new Tile[Width, Height];
        GenerateGrid();

        // test
        tiles[0, 0].SetColor(TileColorState.Player);
        tiles[10, 10].SetColor(TileColorState.Enemy);
        tiles[20, 20].SetColor(TileColorState.Player);
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
            }
        }
    }

    public Tile GetTile(int x, int z)
    {
        if (x < 0 || x >= Width || z < 0 || z >= Height) return null;
        return tiles[x, z];
    }
}
