using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class EnemyPathfinder : MonoBehaviour
{
    [SerializeField] private MonsterData data;
    public Vector2Int goalGridPosition = new Vector2Int(0, 0); // 외부에서 중앙 등으로 설정

    // 현재 적이 위치한 타일
    private Tile currentTile;

    // 현재 따라가야할 경로 타일
    private Queue<Tile> path = new();
    private float speed;

    // 외부에서 현재 타일을 읽기 위한 프로퍼티
    public Tile CurrentTile => currentTile;

    // Enemy참조용
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }



    private IEnumerator Start()
    {
        speed = data.Speed;

        // TileGridManager이 준비될때까지 대기
        while (!TileGridManager.Instance || !TileGridManager.Instance.IsInitialized)
            yield return null;

        // 현재 위치를 기준으로 타일 찾고 설정 -> 형은 이거 형 시스템로직맞춰서 수정하면될듯
        currentTile = TileGridManager.Instance.GetTile(
            Mathf.FloorToInt(transform.position.x / TileGridManager.Instance.cubeSize),
            Mathf.FloorToInt(transform.position.z / TileGridManager.Instance.cubeSize)
        );
    }

    // 외부에서 경로 탐색을 초기화할떄(몬스터 소환 시점)
    public void InitializePathfinder(Vector3 spawnPos, Dictionary<Tile, int> distanceMap = null)
    {
        transform.position = spawnPos;

        // 각 위치에 대응하는 타일 계산
        currentTile = TileGridManager.Instance.GetTile(
            Mathf.FloorToInt(spawnPos.x / TileGridManager.Instance.cubeSize),
            Mathf.FloorToInt(spawnPos.z / TileGridManager.Instance.cubeSize)
        );

        if (currentTile == null) return;

        // 포지션 offset
        transform.position = currentTile.CenterWorldPos + Vector3.up*TileGridManager.Instance.cubeSize;

        path.Clear();


        // 사전 계산된 거리 맵이 있다면 그걸 기반으로 경로 재계산 => 내코드는 지금 tileBase라 BFS사용해도 
        // 별 차이없어서 상관없었는데 형은 아마 BFS빼야할지도
        if (distanceMap != null)
            RecalculatePathFromMap(distanceMap);
        else
            RecalculatePath();
    }

    // 현재 위치에서 목표 타일까지 A*로 경로 재탐색
    public void RecalculatePath(Dictionary<Tile, int> distanceMap = null)
    {
        // 거리 맵이 있으면 더 빠른 방식으로 처리 
        if (distanceMap != null && distanceMap.ContainsKey(currentTile))
        {
            RecalculatePathFromMap(distanceMap);
            return;
        }

        // 목표 타일 계산
        Tile goalTile = TileGridManager.Instance.GetTile(goalGridPosition.x, goalGridPosition.y);
        if (goalTile == null || goalTile.IsOccupied) // IsOccupied : 현재 타일이 점령되었는지( 터렛 or 울타리가 설치되었는지 확인용 => 무시해도됨)
            return;

        // A* 경로 탐색
        var newPath = Pathfinding.APointFindPath(currentTile, goalTile);

        if (newPath == null || newPath.Count == 0)
        {
            path.Clear();
            return;
        }

        // 큐에 경로 저장
        path = new Queue<Tile>(newPath);
    }

    // 거리맵(거리기반 역추적)을 활용한 더 빠른 경로 계산 방식
    public void RecalculatePathFromMap(Dictionary<Tile, int> distanceMap)
    {
        path.Clear();

        if (!distanceMap.ContainsKey(currentTile))
        {
            return;
        }

        Tile current = currentTile;
        List<Tile> result = new();

        // 현재 타일에서 거리 맵을 따라 목표 타일로 역추적
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

    // 특정 타일들을 임시로 막았을 때 경로가 살아있는지 미리 검사하는 시뮬레이션
    public bool WouldHavePathIf(List<Tile> occupiedTiles)
    {
        // 임시로 타일 점유 처리
        foreach (var tile in occupiedTiles)
            tile.IsOccupied = true;

        Tile goalTile = TileGridManager.Instance.GetTile(goalGridPosition.x, goalGridPosition.y);
        var result = Pathfinding.APointFindPath(currentTile, goalTile);

        // 점유 상태 복원
        foreach (var tile in occupiedTiles)
            tile.IsOccupied = false;

        return result != null;
    }

    // 경로에 따라 한 칸씩 이동하며, 도착 시 타일 변경
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
