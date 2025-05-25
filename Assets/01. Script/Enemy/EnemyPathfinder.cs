using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class EnemyPathfinder : MonoBehaviour
{
    [SerializeField] private MonsterData data;
    public Vector2Int goalGridPosition = new Vector2Int(0, 0); // 외부에서 중앙 등으로 설정

    private Tile currentTile;
    private Queue<Tile> path = new();
    private float speed;

    public Tile CurrentTile => currentTile;
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

    public void InitializePathfinder(Vector3 spawnPos, Dictionary<Tile, int> distanceMap = null)
    {
        transform.position = spawnPos;

        currentTile = TileGridManager.Instance.GetTile(
            Mathf.FloorToInt(spawnPos.x / TileGridManager.Instance.cubeSize),
            Mathf.FloorToInt(spawnPos.z / TileGridManager.Instance.cubeSize)
        );

        if (currentTile == null)
        {
            Debug.LogError(" Invalid currentTile at spawn position: " + spawnPos);
            return;
        }

        transform.position = currentTile.CenterWorldPos + Vector3.up*TileGridManager.Instance.cubeSize;

        path.Clear();

        if (distanceMap != null)
            RecalculatePathFromMap(distanceMap);
        else
            RecalculatePath();
    }

    public void RecalculatePath(Dictionary<Tile, int> distanceMap = null)
    {
        if (distanceMap != null && distanceMap.ContainsKey(currentTile))
        {
            RecalculatePathFromMap(distanceMap);
            return;
        }

        Tile goalTile = TileGridManager.Instance.GetTile(goalGridPosition.x, goalGridPosition.y);
        if (goalTile == null || goalTile.IsOccupied)
        {
            Debug.LogError("Invalid or blocked goal tile: " + goalGridPosition);
            return;
        }

        var newPath = Pathfinding.APointFindPath(currentTile, goalTile);

        if (newPath == null || newPath.Count == 0)
        {
            Debug.LogWarning($"A* 경로 없음");
            path.Clear();
            return;
        }

        path = new Queue<Tile>(newPath);
    }

    public void RecalculatePathFromMap(Dictionary<Tile, int> distanceMap)
    {
        path.Clear();

        if (!distanceMap.ContainsKey(currentTile))
        {
            return;
        }

        Tile current = currentTile;
        List<Tile> result = new();

        while (distanceMap.TryGetValue(current, out int currDist) && currDist > 0)
        {
            Tile next = null;

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

        if (result.Count == 0)
        {
         
            return;
        }

        path = new Queue<Tile>(result);
    }

    public bool WouldHavePathIf(List<Tile> occupiedTiles)
    {
        foreach (var tile in occupiedTiles)
            tile.IsOccupied = true;

        Tile goalTile = TileGridManager.Instance.GetTile(goalGridPosition.x, goalGridPosition.y);
        var result = Pathfinding.APointFindPath(currentTile, goalTile);

        foreach (var tile in occupiedTiles)
            tile.IsOccupied = false;

        return result != null;
    }

    private void Update()
    {
        if (path == null || path.Count == 0) return;

        Tile next = path.Peek();
        Vector3 target = next.CenterWorldPos;
        target.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            var previousTile = currentTile;
            currentTile = next;
            path.Dequeue();

            // 필요시 감염 등 추가
            // if (previousTile != currentTile)
            //     enemy?.InfectTile();
        }
    }
}
