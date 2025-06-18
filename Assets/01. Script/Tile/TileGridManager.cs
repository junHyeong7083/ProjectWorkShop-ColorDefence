using System.Collections.Generic;
using UnityEngine;

/*
 None : 중립
 Player : 여왕개미 중심으로 확산된 플레이어 점령지
 Enemy : 적이 침범해서 점령된 지역
 */
public enum TileColorState { None, Player, Enemy }

public class TileData
{
    public Vector2Int GridPos; // A*를 위해 좌표 정보가 꼭 필요함


    public TileColorState ColorState = TileColorState.None;
    public TurretBase TargetingTurret;
    public Fence OccupyingFence; // 울타리 점유용 필드 추가

    // 터렛 또는 울타리가 점유 중일 때 true 반환
    public bool IsOccupied => TargetingTurret != null || OccupyingFence != null;

    public bool IsReserved = false;

    public float LastChangedTime = -999f;

    public int EnemyPresenceCount = 0;
    public void Reserve() => IsReserved = true;
    public void Release() => IsReserved = false;
}

public class TileGridManager : MonoBehaviour
{
    public static TileGridManager Instance;

    public int Width;
    public int Height;
    public float cubeSize;

    public TileData[,] tiles;

    public bool IsInitialized { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
        tiles = new TileData[Width, Height];

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                tiles[x, z] = new TileData
                {
                    GridPos = new Vector2Int(x, z)
                };
            }
        }

   //     TileCounter.Instance.Init(Width * Height);
        IsInitialized = true;
    }

    public TileData GetTile(int x, int z)
    {
        if (x < 0 || x >= Width || z < 0 || z >= Height)
            return null;

        return tiles[x, z];
    }

    public bool CanPlaceTurret(int startX, int startZ, int width, int height)
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                var tile = GetTile(startX + x, startZ + z);
                if (tile == null || tile.IsOccupied || tile.ColorState == TileColorState.Enemy)
                    return false;
            }
        }
        /*foreach (var pathfinder in FindObjectsByType<EnemyPathfinder>(FindObjectsSortMode.None))
        {
            if (pathfinder.CurrentTile == )
            {
                Debug.Log($"[설치불가] 몬스터가 타일 ({tile.GridPos}) 위에 있음");
                return false;
            }
        }*/
        return true;
    }
    public static Vector2Int GetGridPositionFromWorld(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x / Instance.cubeSize);
        int z = Mathf.FloorToInt(worldPos.z / Instance.cubeSize);
        return new Vector2Int(x, z);
    }

    public List<Vector2Int> GetTilesInRange(Vector3 worldPos, float range)
    {
        List<Vector2Int> result = new();

        int minX = Mathf.FloorToInt((worldPos.x - range) / cubeSize);
        int maxX = Mathf.FloorToInt((worldPos.x + range) / cubeSize);
        int minZ = Mathf.FloorToInt((worldPos.z - range) / cubeSize);
        int maxZ = Mathf.FloorToInt((worldPos.z + range) / cubeSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                if (!IsInBounds(x, z)) continue;

                Vector3 tileCenter = GetWorldPositionFromGrid(x, z) + new Vector3(cubeSize * 0.5f, 0, cubeSize * 0.5f);
                if ((tileCenter - worldPos).sqrMagnitude <= range * range)
                    result.Add(new Vector2Int(x, z));
            }
        }

        return result;
    }

    public bool IsInBounds(int x, int z) => x >= 0 && x < Width && z >= 0 && z < Height;

    public static Vector3 GetWorldPositionFromGrid(int x, int z)
    {
        return new Vector3(x * Instance.cubeSize, 0, z * Instance.cubeSize);
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
            GetWorldPositionFromGrid(midX, 0),
            GetWorldPositionFromGrid(midX, Instance.Height - 1),
            GetWorldPositionFromGrid(0, midZ),
            GetWorldPositionFromGrid(Instance.Width - 1, midZ),
        };
    }


#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!Application.isPlaying || tiles == null) return;

        for (int x = 0; x < Width; x++)
        {
            for (int z = 0; z < Height; z++)
            {
                var tile = tiles[x, z];
                if (tile.ColorState == TileColorState.Player)
                {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawWireCube(GetWorldPositionFromGrid(x, z) + Vector3.one * 0.5f, Vector3.one);
                }
            }
        }
    }
#endif

}
