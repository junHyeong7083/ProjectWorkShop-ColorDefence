using System.Collections.Generic;
using UnityEngine;

// A* 경로 탐색 + BFS 기반 거리맵 생성 기능 제공.
public static class Pathfinding
{
    // A* 알고리즘을 사용하여 시작 타일에서 목표 타일까지의 최적 경로를 계산함.
    public static List<Tile> APointFindPath(Tile startTile, Tile goalTile)
    {
        if (startTile == null || goalTile == null)
            return null;

        // fScore 기반 정렬을 위한 우선순위 큐 역할의 Set
        var openSet = new SortedSet<TileNode>(new TileNodeComparer());

        var cameFrom = new Dictionary<Tile, Tile>(); // 경로 추적용
        var gScore = new Dictionary<Tile, float>();  // 시작지점부터 현재 노드까지의 실제 비용
        var fScore = new Dictionary<Tile, float>();  // gScore + Heuristic = 예상 총 비용

        gScore[startTile] = 0;
        fScore[startTile] = Heuristic(startTile, goalTile);
        openSet.Add(new TileNode(startTile, fScore[startTile]));

        while (openSet.Count > 0)
        {
            // 현재 가장 fScore가 낮은 노드 추출
            TileNode currentNode = openSet.Min;
            Tile current = currentNode.Tile;

            if (current == goalTile)
                return ReconstructPath(cameFrom, current); // 경로 도달 시 재구성

            openSet.Remove(currentNode); // 처리 완료

            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor.IsOccupied) continue; // 점유 타일은 통과 불가

                float tentativeG = gScore[current] + 1f;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    // 더 짧은 경로 발견 시 경로 갱신
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goalTile);

                    // openSet 중복 방지: 제거 후 재추가
                    TileNode neighborNode = new TileNode(neighbor, fScore[neighbor]);
                    openSet.Remove(neighborNode); // 없으면 무시됨
                    openSet.Add(neighborNode);
                }
            }
        }

        // 경로를 찾지 못한 경우
        return null;
    }

   // 목표 타일로부터 각 타일까지의 최단 거리(BFS 기반)를 저장하는 거리 맵 생성
    public static Dictionary<Tile, int> GenerateDistanceMap(Tile goalTile)
    {
        Dictionary<Tile, int> distanceMap = new();
        Queue<Tile> queue = new();

        if (goalTile == null || goalTile.IsOccupied)
            return distanceMap;

        queue.Enqueue(goalTile);
        distanceMap[goalTile] = 0;

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();
            int currentDistance = distanceMap[current];

            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor.IsOccupied || distanceMap.ContainsKey(neighbor)) continue;

                // 이웃의 거리 = 현재 거리 + 1
                distanceMap[neighbor] = currentDistance + 1;
                queue.Enqueue(neighbor);
            }
        }

        return distanceMap;
    }

    // 두 타일 사이의 맨해튼 거리 (휴리스틱) 계산
    static float Heuristic(Tile a, Tile b)
    {
        return Mathf.Abs(a.GridPos.x - b.GridPos.x) + Mathf.Abs(a.GridPos.y - b.GridPos.y);
    }

    // 상하좌우 4방향의 인접 타일을 반환
    public static List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> result = new();

        int x = tile.GridPos.x;
        int z = tile.GridPos.y;
        TileGridManager g = TileGridManager.Instance;

        void Add(int dx, int dz)
        {
            Tile t = g.GetTile(x + dx, z + dz);
            if (t != null)
                result.Add(t);
        }

        Add(0, 1);   // 위
        Add(0, -1);  // 아래
        Add(-1, 0);  // 왼쪽
        Add(1, 0);   // 오른쪽

        return result;
    }

    // cameFrom 맵을 따라 목표에서 시작지까지 경로를 역추적함
    static List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
    {
        List<Tile> path = new();

        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse(); // 거꾸로 추적했으므로 역순 정렬
        return path;
    }

    // A* 내부에서 사용되는 노드 클래스 (타일 + fScore)
    private class TileNode
    {
        public Tile Tile { get; }
        public float FScore { get; }

        public TileNode(Tile tile, float fScore)
        {
            Tile = tile;
            FScore = fScore;
        }

        public override bool Equals(object obj)
        {
            return obj is TileNode other && Tile == other.Tile;
        }

        public override int GetHashCode()
        {
            return Tile.GetHashCode();
        }
    }

    // fScore 기준으로 TileNode를 정렬하기 위한 Comparer
    private class TileNodeComparer : IComparer<TileNode>
    {
        public int Compare(TileNode a, TileNode b)
        {
            int fCompare = a.FScore.CompareTo(b.FScore);
            if (fCompare != 0)
                return fCompare;

            // fScore가 같으면 고유값으로 비교 (중복 방지)
            return a.Tile.GetHashCode().CompareTo(b.Tile.GetHashCode());
        }
    }
}
