using System.Collections.Generic;

public static class Pathfinding
{
    public static List<Tile> FindPath(Tile startTile, Tile goalTile)
    {
        //// 울타리 설치됬을때 다시 VISITED를 초기화해야할듯? => 만약 왔던길을 되돌아가야하는 상황
        //// 좀따 하기


        // BFS 탐색을 위한 큐
        Queue<Tile> queue = new();

        // 각 타일이 어디서 왔는지 기록하는 맵 (역추적용)
        Dictionary<Tile, Tile> cameFrom = new();

        // 이미 방문한 타일을 기록 (중복 방지)
        HashSet<Tile> visited = new();

        // 시작 타일로 초기화
        queue.Enqueue(startTile);
        visited.Add(startTile);

        // BFS 탐색 시작
        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            // 목표에 도달했으면 탐색 종료
            if (current == goalTile) break;

            // 상하좌우 인접 타일을 탐색
            foreach (var neighbor in GetNeighbors(current))
            {
                // 방문하지 않았고, 막히지 않은 타일만 큐에 추가
                if (!visited.Contains(neighbor) && !neighbor.IsOccupied)
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);

                    // 이웃 타일로 가기 위해 어떤 타일에서 왔는지 기록
                    cameFrom[neighbor] = current;
                }
            }
        }

        // 목표까지 도달하지 못했다면 null 반환 (막힘)
        if (!cameFrom.ContainsKey(goalTile))
            return null;

        // 목표부터 시작까지 역추적하여 경로를 만든다
        List<Tile> path = new();
        Tile t = goalTile;

        while (t != startTile)
        {
            path.Add(t);
            t = cameFrom[t];
        }

        // 경로를 반대로 뒤집어야 (시작 → 목표 순서)
        path.Reverse();
        return path;
    }

    /// <summary>
    /// 주어진 타일의 상하좌우 인접 타일을 반환한다.
    /// </summary>
    static List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> result = new();

        // 현재 타일 좌표
        int x = tile.GridPos.x;
        int z = tile.GridPos.y;

        TileGridManager g = TileGridManager.Instance;

        // 내부 함수로 인접 좌표 더하고 타일 존재 시 추가
        void Add(int dx, int dz)
        {
            Tile t = g.GetTile(x + dx, z + dz);
            if (t != null)
                result.Add(t);
        }

        // 상하좌우 4방향
        Add(0, 1);   // 위쪽
        Add(0, -1);  // 아래쪽
        Add(-1, 0);  // 왼쪽
        Add(1, 0);   // 오른쪽

        return result;
    }
}