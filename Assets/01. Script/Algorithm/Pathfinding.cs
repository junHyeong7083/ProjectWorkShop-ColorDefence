// Pathfinding.cs - BFS �Ÿ��� �߰� ����
using System.Collections.Generic;
using UnityEngine;

public static class Pathfinding
{
    // A* �˰��� ��� -> ���� ��� Ž��
    public static List<Tile> APointFindPath(Tile startTile, Tile goalTile)
    {
        if (startTile == null || goalTile == null)
            return null;

        // Ž���� ��� ���� - �켱���� ť ����, fScore ������������ ����
        var openSet = new SortedSet<TileNode>(new TileNodeComparer());
        // �� Ÿ���� ��� �Դ��� �����ϴ� ��
        var cameFrom = new Dictionary<Tile, Tile>();
        // ���� Ÿ�Ϸκ��� ���� Ÿ�ϱ����� ���� �̵� �Ÿ�
        var gScore = new Dictionary<Tile, float>();
        // fScore = gScore + �޸���ƽ (���� ���� ���)
        var fScore = new Dictionary<Tile, float>();


        // ���� Ÿ���� gScore�� 0
        gScore[startTile] = 0;
        // ���� Ÿ���� fScore�� �޸���ƽ ��갪
        fScore[startTile] = Heuristic(startTile, goalTile);

        // openSet�� ���� ��� �߰�
        openSet.Add(new TileNode(startTile, fScore[startTile]));

        while (openSet.Count > 0)
        {
            // fScore�� ���� ���� ��� ������
            TileNode currentNode = openSet.Min;
            Tile current = currentNode.Tile;

            // ��ǥ�� ���������� ��� �籸�� �� ��ȯ
            if (current == goalTile)
                return ReconstructPath(cameFrom, current);

            // ���� ��带 openSet���� ����
            openSet.Remove(currentNode);

            // ���� Ÿ���� �̿� Ÿ���� �˻�
            foreach (var neighbor in GetNeighbors(current))
            {
                // ����Ÿ���� �����Ǿ������� ����
                if (neighbor.IsOccupied) continue;

                // ������� �Ÿ� + 1
                float tentativeG = gScore[current] + 1f;

                // neighbor�� gScore�� ���ų� �� ª�� ��� �߰� �� ����
                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    // ��� �Դ��� ��� + gScore, fScore ����
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goalTile);

                    // openSet ����(�̹� �����ϸ� ������ �ٽ� �߰�)
                    TileNode neighborNode = new TileNode(neighbor, fScore[neighbor]);
                    openSet.Remove(neighborNode);
                    openSet.Add(neighborNode);
                }
            }
        }
        // ��θ� ã�� �� ���� ��� null 
        return null;
    }

    // ��ǥ Ÿ���� �������� BFS �Ÿ� ���� ����
    public static Dictionary<Tile, int> GenerateDistanceMap(Tile goalTile)
    {
        // �� Ÿ�ϱ����� �Ÿ� ���� ��
        Dictionary<Tile, int> distanceMap = new();
        // BFS Ž���� ť
        Queue<Tile> queue = new();

        if (goalTile == null) return distanceMap;

        // ��ǥ Ÿ���� ���������� ť�� �ְ� �Ÿ� 0���� ����
        queue.Enqueue(goalTile);
        distanceMap[goalTile] = 0;

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();
            int currentDistance = distanceMap[current];

            // ���� Ÿ���� �̿����� ��ȸ
            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor.IsOccupied || distanceMap.ContainsKey(neighbor)) continue;

                distanceMap[neighbor] = currentDistance + 1;
                queue.Enqueue(neighbor);
            }
        }

        // �ϼ��� �Ÿ� �� ��ȯ
        return distanceMap;
    }

    // �� Ÿ�� ���� �޸���ƽ �Ÿ� ���
    static float Heuristic(Tile a, Tile b)
    {
        return Mathf.Abs(a.GridPos.x - b.GridPos.x) + Mathf.Abs(a.GridPos.y - b.GridPos.y);
    }

    // ���� Ÿ���� �����¿� �̿� Ÿ�� ��ȯ
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

    // cameFrom ��ųʸ��� �̿��Ͽ� ��θ� ������
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

    // A*���� ���Ǵ� ��� Ŭ����
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

    // TileNode ���� ���� Ŭ����
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