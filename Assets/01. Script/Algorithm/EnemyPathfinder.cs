// EnemyPathfinder.cs - distanceMap ��� ��� ���� ����ȭ
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EnemyPathfinder : MonoBehaviour
{
    [SerializeField] private MonsterData data;
    [SerializeField] private Vector2Int goalGridPosition = new Vector2Int(0, 0);

    private Tile currentTile;

    // ��θ� �����ϴ� ť
    private Queue<Tile> path = new();
    private float speed;

    public Tile CurrentTile => currentTile;

    IEnumerator Start()
    {
        speed = data.Speed;

        // Ÿ�� �ʱ�ȭ ���� ������ ���
        while (!TileGridManager.Instance || !TileGridManager.Instance.IsInitialized)
            yield return null;

        currentTile = TileGridManager.Instance.GetTile(
            Mathf.FloorToInt(transform.position.x / TileGridManager.Instance.cubeSize),
            Mathf.FloorToInt(transform.position.z / TileGridManager.Instance.cubeSize)
        );

        // ���� ��ΰ��(A*���)
        RecalculatePath();
    }


    // A* �˰����� �̿��� ��� ���
    public void RecalculatePath()
    {
        Tile goalTile = TileGridManager.Instance.GetTile(goalGridPosition.x, goalGridPosition.y);
        var newPath = Pathfinding.APointFindPath(currentTile, goalTile);

        // ��ΰ� ������ path ť�� �����ϰ�, ������ ���
        if (newPath != null)
            path = new Queue<Tile>(newPath);
        else
            path.Clear();
    }

    // ������ ������� distanceMap�� ������� ��� ������
    public void RecalculatePathFromMap(Dictionary<Tile, int> distanceMap)
    {
        path.Clear();

        if (!distanceMap.ContainsKey(currentTile))
            return; // !distanceMap.ContainsKey(currentTile) => BFS�� ������������ => ��ξ��� => return

        Tile current = currentTile;
        List<Tile> result = new();

        while (distanceMap.TryGetValue(current, out int currDist) && currDist > 0)
        {
            Tile next = null;
            // �̿� �߿��� �Ÿ� ���� �� ���� Ÿ���� ����
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

    // Ư�� Ÿ�ϵ��� ���� ���·� �ٲپ��� �� ��ΰ� �����ϴ��� �ùķ��̼�
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
