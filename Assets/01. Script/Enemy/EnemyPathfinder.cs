using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class EnemyPathfinder : MonoBehaviour
{
    [SerializeField] private MonsterData data;
    public Vector2Int goalGridPosition = new Vector2Int(0, 0);

    private TileData currentTile;
    private Queue<TileData> path = new();
    private float speed;

    public TileData CurrentTile => currentTile;
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private IEnumerator Start()
    {
        speed = data.Speed;

        while (!TileGridManager.Instance || !TileGridManager.Instance.IsInitialized)
            yield return null;

        currentTile = TileGridManager.Instance.GetTile(
            Mathf.FloorToInt(transform.position.x / TileGridManager.Instance.cubeSize),
            Mathf.FloorToInt(transform.position.z / TileGridManager.Instance.cubeSize)
        );
    }

    public void InitializePathfinder(Vector3 spawnPos, Dictionary<TileData, int> distanceMap = null)
    {
        transform.position = spawnPos;

        currentTile = TileGridManager.Instance.GetTile(
            Mathf.FloorToInt(spawnPos.x / TileGridManager.Instance.cubeSize),
            Mathf.FloorToInt(spawnPos.z / TileGridManager.Instance.cubeSize)
        );

        if (currentTile == null)
        {
           // Debug.LogError($"[Pathfinder] currentTile is NULL! spawnPos = {spawnPos}");
            return;
        }

        //Debug.Log($"[Pathfinder] Init at GridPos: {currentTile.GridPos}");

        transform.position = TileGridManager.GetWorldPositionFromGrid(
            currentTile.GridPos.x, currentTile.GridPos.y
        ) + Vector3.up * TileGridManager.Instance.cubeSize;

        path.Clear();

        if (distanceMap != null)
        {
          //  Debug.Log("[Pathfinder] Using DistanceMap");
            RecalculatePathFromMap(distanceMap);
        }
        else
        {
           // Debug.Log("[Pathfinder] Using APointFindPath");
            RecalculatePath();
        }

        //Debug.Log($"[Pathfinder] path count after recalc: {path.Count}");
    }


    public void RecalculatePath(Dictionary<TileData, int> distanceMap = null)
    {
        if (distanceMap != null && distanceMap.ContainsKey(currentTile))
        {
            RecalculatePathFromMap(distanceMap);
            return;
        }

        var goalTile = TileGridManager.Instance.GetTile(goalGridPosition.x, goalGridPosition.y);
        Debug.Log($"goalTile: {goalTile?.GridPos}, IsOccupied: {goalTile?.IsOccupied}");

        if (goalTile == null || goalTile.IsOccupied)
            return;

        var newPath = Pathfinding.APointFindPath(currentTile, goalTile);

        if (newPath == null || newPath.Count == 0)
        {
            path.Clear();
            return;
        }

        path = new Queue<TileData>(newPath);
    }

    public void RecalculatePathFromMap(Dictionary<TileData, int> distanceMap)
    {
        path.Clear();

        if (!distanceMap.ContainsKey(currentTile))
            return;

        TileData current = currentTile;
        List<TileData> result = new();

        while (distanceMap.TryGetValue(current, out int currDist) && currDist > 0)
        {
            TileData next = null;

            foreach (var neighbor in Pathfinding.GetNeighbors(current))
            {
                if (!distanceMap.TryGetValue(neighbor, out int neighborDist)) continue;
                if (neighborDist < currDist)
                {
                    next = neighbor;
                    currDist = neighborDist;
                }
            }

            if (next == null) break;

            result.Add(next);
            current = next;
        }

        if (result.Count > 0)
            path = new Queue<TileData>(result);
    }

    public bool WouldHavePathIf(List<TileData> occupiedTiles)
    {
        foreach (var tile in occupiedTiles)
            tile.OccupyingFence = new DummyFence(); // 임시 점유

        var goalTile = TileGridManager.Instance.GetTile(goalGridPosition.x, goalGridPosition.y);
        var result = Pathfinding.APointFindPath(currentTile, goalTile);

        foreach (var tile in occupiedTiles)
            tile.OccupyingFence = null; // 점유 해제

        return result != null;
    }

    private void Update()
    {
        if (path == null || path.Count == 0) return;

        TileData next = path.Peek();
        Vector3 target = TileGridManager.GetWorldPositionFromGrid(next.GridPos.x, next.GridPos.y);
        target.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            var previousTile = currentTile;
            currentTile = next;
            path.Dequeue();

            // enemy?.InfectTile(); // 필요 시
        }
    }
}

// 임시 점유용 더미 터렛 객체
public class DummyFence : Fence { }
