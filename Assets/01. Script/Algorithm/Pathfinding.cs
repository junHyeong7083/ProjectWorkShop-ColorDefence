using System.Collections.Generic;
using UnityEngine;

// A* 경로 탐색 + BFS 기반 거리맵 생성 기능 제공.
public static class Pathfinding
{
    public static List<TileData> APointFindPath(TileData startTile, TileData goalTile)
    {
        if (startTile == null || goalTile == null)
            return null;

        var openSet = new SortedSet<TileNode>(new TileNodeComparer());
        var cameFrom = new Dictionary<TileData, TileData>();
        var gScore = new Dictionary<TileData, float>();
        var fScore = new Dictionary<TileData, float>();

        gScore[startTile] = 0;
        fScore[startTile] = Heuristic(startTile, goalTile);
        openSet.Add(new TileNode(startTile, fScore[startTile]));

        while (openSet.Count > 0)
        {
            TileNode currentNode = openSet.Min;
            TileData current = currentNode.Tile;

            if (current == goalTile)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(currentNode);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor.IsOccupied) continue;

                float tentativeG = gScore[current] + 1f;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goalTile);

                    TileNode neighborNode = new TileNode(neighbor, fScore[neighbor]);
                    openSet.Remove(neighborNode);
                    openSet.Add(neighborNode);
                }
            }
        }

        return null;
    }

    public static Dictionary<TileData, int> GenerateDistanceMap(TileData goalTile)
    {
        Dictionary<TileData, int> distanceMap = new();
        Queue<TileData> queue = new();

        if (goalTile == null || goalTile.IsOccupied)
            return distanceMap;

        queue.Enqueue(goalTile);
        distanceMap[goalTile] = 0;

        while (queue.Count > 0)
        {
            TileData current = queue.Dequeue();
            int currentDistance = distanceMap[current];

            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor.IsOccupied || distanceMap.ContainsKey(neighbor)) continue;

                distanceMap[neighbor] = currentDistance + 1;
                queue.Enqueue(neighbor);
            }
        }

        return distanceMap;
    }

    static float Heuristic(TileData a, TileData b)
    {
        return Mathf.Abs(a.GridPos.x - b.GridPos.x) + Mathf.Abs(a.GridPos.y - b.GridPos.y);
    }

    public static List<TileData> GetNeighbors(TileData tile)
    {
        List<TileData> result = new();
        int x = tile.GridPos.x;
        int z = tile.GridPos.y;

        TileGridManager g = TileGridManager.Instance;

        void Add(int dx, int dz)
        {
            var neighbor = g.GetTile(x + dx, z + dz);
            if (neighbor != null)
                result.Add(neighbor);
        }

        Add(0, 1);
        Add(0, -1);
        Add(-1, 0);
        Add(1, 0);

        return result;
    }

    static List<TileData> ReconstructPath(Dictionary<TileData, TileData> cameFrom, TileData current)
    {
        List<TileData> path = new();

        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse();
        return path;
    }

    private class TileNode
    {
        public TileData Tile { get; }
        public float FScore { get; }

        public TileNode(TileData tile, float fScore)
        {
            Tile = tile;
            FScore = fScore;
        }

        public override bool Equals(object obj) =>
            obj is TileNode other && Tile == other.Tile;

        public override int GetHashCode() =>
            Tile.GetHashCode();
    }

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

