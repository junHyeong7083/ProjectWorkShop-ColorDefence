// EnemyPathfinder.cs - distanceMap 기반 경로 추적 최적화
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyPathfinder : MonoBehaviour
{
    [SerializeField] private MonsterData data;
    [SerializeField] private Vector2Int goalGridPosition = new Vector2Int(0, 0);

    private Tile currentTile;

    // 경로를 저장하는 큐
    private Queue<Tile> path = new();
    private float speed;

    public Tile CurrentTile => currentTile;

    IEnumerator Start()
    {
        speed = data.Speed;

        // 타일 초기화 끝날 때까지 대기
        while (!TileGridManager.Instance || !TileGridManager.Instance.IsInitialized)
            yield return null;

        currentTile = TileGridManager.Instance.GetTile(
            Mathf.FloorToInt(transform.position.x / TileGridManager.Instance.cubeSize),
            Mathf.FloorToInt(transform.position.z / TileGridManager.Instance.cubeSize)
        );

        // 최초 경로계산(A*방식)
        RecalculatePath();
    }


    // A* 알고리즘을 이용해 경로 계산
    public void RecalculatePath()
    {
        Tile goalTile = TileGridManager.Instance.GetTile(goalGridPosition.x, goalGridPosition.y);
        var newPath = Pathfinding.APointFindPath(currentTile, goalTile);

        // 경로가 있으면 path 큐에 저장하고, 없으면 비움
        if (newPath != null)
            path = new Queue<Tile>(newPath);
        else
            path.Clear();
    }

    // 사전에 만들어진 distanceMap을 기반으로 경로 역추적
    public void RecalculatePathFromMap(Dictionary<Tile, int> distanceMap)
    {
        path.Clear();

        if (!distanceMap.ContainsKey(currentTile))
            return; // !distanceMap.ContainsKey(currentTile) => BFS가 도달하지못함 => 경로없음 => return

        Tile current = currentTile;
        List<Tile> result = new();

        while (distanceMap.TryGetValue(current, out int currDist) && currDist > 0)
        {
            Tile next = null;
            // 이웃 중에서 거리 값이 더 작은 타일을 선택
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

        path = new Queue<Tile>(result);
    }

    // 특정 타일들을 점유 상태로 바꾸었을 때 경로가 존재하는지 시뮬레이션
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

    void Update()
    {
        if (path == null || path.Count == 0) return;

        Tile next = path.Peek();
        Vector3 target = next.CenterWorldPos;
        target.y = transform.position.y;

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            currentTile = next;
            path.Dequeue();
        }
    }
}
