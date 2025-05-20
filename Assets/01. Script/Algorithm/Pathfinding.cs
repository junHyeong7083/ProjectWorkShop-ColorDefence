// Pathfinding.cs - BFS 거리맵 추가 포함
using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    // A* 알고리즘 사용 -> 최적 경로 탐색
    public static List<Tile> APointFindPath(Tile startTile, Tile goalTile)
    {
        if (startTile == null || goalTile == null)
            return null;

        // 탐색할 노드 집합 - 우선순위 큐 역할, fScore 오름차순으로 정렬
        var openSet = new SortedSet<TileNode>(new TileNodeComparer());
        // 각 타일이 어디서 왔는지 추적하는 맵
        var cameFrom = new Dictionary<Tile, Tile>();
        // 시작 타일로부터 현재 타일까지의 실제 이동 거리
        var gScore = new Dictionary<Tile, float>();
        // fScore = gScore + 휴리스틱 (예상 최종 비용)
        var fScore = new Dictionary<Tile, float>();


        // 시작 타일의 gScore는 0
        gScore[startTile] = 0;
        // 시작 타일의 fScore는 휴리스틱 계산값
        fScore[startTile] = Heuristic(startTile, goalTile);

        // openSet에 시작 노드 추가
        openSet.Add(new TileNode(startTile, fScore[startTile]));

        while (openSet.Count > 0)
        {
            // fScore가 가장 낮은 노드 꺼내기
            TileNode currentNode = openSet.Min;
            Tile current = currentNode.Tile;

            // 목표에 도달했으면 경로 재구성 후 반환
            if (current == goalTile)
                return ReconstructPath(cameFrom, current);

            // 현재 노드를 openSet에서 제거
            openSet.Remove(currentNode);

            // 현재 타일의 이웃 타일을 검사
            foreach (var neighbor in GetNeighbors(current))
            {
                // 현재타일이 점유되어있으면 무시
                if (neighbor.IsOccupied) continue;

                // 현재까지 거리 + 1
                float tentativeG = gScore[current] + 1f;

                // neighbor의 gScore가 없거나 더 짧은 경로 발견 시 갱신
                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    // 어디서 왔는지 기록 + gScore, fScore 갱신
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goalTile);

                    // openSet 갱신(이미 존재하면 제거후 다시 추가)
                    TileNode neighborNode = new TileNode(neighbor, fScore[neighbor]);
                    openSet.Remove(neighborNode);
                    openSet.Add(neighborNode);
                }
            }
        }
        // 경로를 찾을 수 없는 경우 null 
        return null;
    }

    // 목표 타일을 기준으로 BFS 거리 맵을 생성
    public static Dictionary<Tile, int> GenerateDistanceMap(Tile goalTile)
    {
        // 각 타일까지의 거리 저장 맵
        Dictionary<Tile, int> distanceMap = new();
        // BFS 탐색용 큐
        Queue<Tile> queue = new();

        if (goalTile == null) return distanceMap;

        // 목표 타일을 시작점으로 큐에 넣고 거리 0으로 설정
        queue.Enqueue(goalTile);
        distanceMap[goalTile] = 0;

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();
            int currentDistance = distanceMap[current];

            // 현재 타일의 이웃들을 순회
            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor.IsOccupied || distanceMap.ContainsKey(neighbor)) continue;

                distanceMap[neighbor] = currentDistance + 1;
                queue.Enqueue(neighbor);
            }
        }

        // 완성된 거리 맵 반환
        return distanceMap;
    }

    // 두 타일 간의 휴리스틱 거리 계산
    static float Heuristic(Tile a, Tile b)
    {
        return Mathf.Abs(a.GridPos.x - b.GridPos.x) + Mathf.Abs(a.GridPos.y - b.GridPos.y);
    }

    // 현재 타일의 상하좌우 이웃 타일 반환
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

        Add(0, 1);
        Add(0, -1);
        Add(-1, 0);
        Add(1, 0);

        return result;
    }

    // cameFrom 딕셔너리를 이용하여 경로를 역추적
    static List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
    {
        List<Tile> path = new();

        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }

    // A*에서 사용되는 노드 클래스
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

    // TileNode 정렬 기준 클래스
    private class TileNodeComparer : IComparer<TileNode>
    {
        public int Compare(TileNode a, TileNode b)
        {
            int fCompare = a.FScore.CompareTo(b.FScore);
            if (fCompare != 0)
                return fCompare;

            return a.Tile.GetHashCode().CompareTo(b.Tile.GetHashCode());
        }
    }
}