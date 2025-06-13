using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class EnemyPathfinder : MonoBehaviour
{
    public Vector2Int goalGridPosition = new Vector2Int(0, 0);

    private TileData currentTile;
    private Queue<TileData> path = new();
    private float speed;

    public TileData CurrentTile => currentTile;
    private BaseEnemy baseEnemy;

    private void Awake() => baseEnemy = GetComponent<BaseEnemy>();

    private IEnumerator Start()
    {
        speed = baseEnemy.Data.Speed;

        while (!TileGridManager.Instance || !TileGridManager.Instance.IsInitialized)
            yield return null;

        currentTile = TileGridManager.Instance.GetTile(
            Mathf.FloorToInt(transform.position.x / TileGridManager.Instance.cubeSize),
            Mathf.FloorToInt(transform.position.z / TileGridManager.Instance.cubeSize)
        );
    }

    public void InitializePathfinder(Transform spawnPos, Dictionary<TileData, int> distanceMap = null)
    {
        transform.position = spawnPos.position;

        currentTile = TileGridManager.Instance.GetTile(
            Mathf.FloorToInt(spawnPos.position.x / TileGridManager.Instance.cubeSize),
            Mathf.FloorToInt(spawnPos.position.z / TileGridManager.Instance.cubeSize)
        );

        if (currentTile == null)
        {
            return;
        }


        transform.position = TileGridManager.GetWorldPositionFromGrid(
            currentTile.GridPos.x, currentTile.GridPos.y
        ) + Vector3.up * TileGridManager.Instance.cubeSize;

        path.Clear();

        if (distanceMap != null)
        {
            RecalculatePathFromMap(distanceMap);
        }
        else
        {
            RecalculatePath();
        }

        currentTile.EnemyPresenceCount++;
        currentTile.ColorState = TileColorState.Enemy;
    }


    public void RecalculatePath(Dictionary<TileData, int> distanceMap = null)
    {
        if (distanceMap != null && distanceMap.ContainsKey(currentTile))
        {
            RecalculatePathFromMap(distanceMap);
            return;
        }

        var goalTile = TileGridManager.Instance.GetTile(goalGridPosition.x, goalGridPosition.y);

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
        if (occupiedTiles.Contains(currentTile))
        {
            Debug.LogWarning("[PATH] currentTile이 시뮬레이션 점유 타일에 포함되어 있음");
        }

        foreach (var tile in occupiedTiles)
            tile.OccupyingFence = new DummyFence(); // 임시 점유

        var goalTile = TileGridManager.Instance.GetTile(goalGridPosition.x, goalGridPosition.y);
        var result = Pathfinding.APointFindPath(currentTile, goalTile);

        foreach (var tile in occupiedTiles)
            tile.OccupyingFence = null; // 점유 해제

        return result != null;
    }

  /*  private void InfectCurrentTile()
    {
        if (currentTile == null) return;

       // if (currentTile.ColorState != enemy.Data.InfectColor)
       // {
       //     currentTile.ColorState = enemy.Data.InfectColor;
       //   //  Debug.Log($"Tile at {currentTile.GridPos} infected by {enemy.name}");
       // }
    }*/

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
            if (previousTile != null)
            {
                previousTile.EnemyPresenceCount--;
                if (previousTile.EnemyPresenceCount <= 0)
                {
                    previousTile.EnemyPresenceCount = 0;
                    if (previousTile.ColorState == TileColorState.Enemy)
                        previousTile.ColorState = TileColorState.None;
                }
            }

            //  현재 타일 몬스터 수 증가
            if (currentTile != null)
            {
                currentTile.EnemyPresenceCount++;
                currentTile.ColorState = TileColorState.Enemy;
            }
        }
    }
}

// 임시 점유용 더미 터렛 객체
public class DummyFence : Fence { }
