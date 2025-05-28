using System.Collections.Generic;
using UnityEngine;

// A* ��� Ž�� + BFS ��� �Ÿ��� ���� ��� ����.
public static class Pathfinding
{
    // A* �˰����� ����Ͽ� ���� Ÿ�Ͽ��� ��ǥ Ÿ�ϱ����� ���� ��θ� �����.
    public static List<Tile> APointFindPath(Tile startTile, Tile goalTile)
    {
        if (startTile == null || goalTile == null)
            return null;

        // fScore ��� ������ ���� �켱���� ť ������ Set
        var openSet = new SortedSet<TileNode>(new TileNodeComparer());

        var cameFrom = new Dictionary<Tile, Tile>(); // ��� ������
        var gScore = new Dictionary<Tile, float>();  // ������������ ���� �������� ���� ���
        var fScore = new Dictionary<Tile, float>();  // gScore + Heuristic = ���� �� ���

        gScore[startTile] = 0;
        fScore[startTile] = Heuristic(startTile, goalTile);
        openSet.Add(new TileNode(startTile, fScore[startTile]));

        while (openSet.Count > 0)
        {
            // ���� ���� fScore�� ���� ��� ����
            TileNode currentNode = openSet.Min;
            Tile current = currentNode.Tile;

            if (current == goalTile)
                return ReconstructPath(cameFrom, current); // ��� ���� �� �籸��

            openSet.Remove(currentNode); // ó�� �Ϸ�

            foreach (var neighbor in GetNeighbors(current))
            {
                if (neighbor.IsOccupied) continue; // ���� Ÿ���� ��� �Ұ�

                float tentativeG = gScore[current] + 1f;

                if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                {
                    // �� ª�� ��� �߰� �� ��� ����
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, goalTile);

                    // openSet �ߺ� ����: ���� �� ���߰�
                    TileNode neighborNode = new TileNode(neighbor, fScore[neighbor]);
                    openSet.Remove(neighborNode); // ������ ���õ�
                    openSet.Add(neighborNode);
                }
            }
        }

        // ��θ� ã�� ���� ���
        return null;
    }

   // ��ǥ Ÿ�Ϸκ��� �� Ÿ�ϱ����� �ִ� �Ÿ�(BFS ���)�� �����ϴ� �Ÿ� �� ����
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

                // �̿��� �Ÿ� = ���� �Ÿ� + 1
                distanceMap[neighbor] = currentDistance + 1;
                queue.Enqueue(neighbor);
            }
        }

        return distanceMap;
    }

    // �� Ÿ�� ������ ����ư �Ÿ� (�޸���ƽ) ���
    static float Heuristic(Tile a, Tile b)
    {
        return Mathf.Abs(a.GridPos.x - b.GridPos.x) + Mathf.Abs(a.GridPos.y - b.GridPos.y);
    }

    // �����¿� 4������ ���� Ÿ���� ��ȯ
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

        Add(0, 1);   // ��
        Add(0, -1);  // �Ʒ�
        Add(-1, 0);  // ����
        Add(1, 0);   // ������

        return result;
    }

    // cameFrom ���� ���� ��ǥ���� ���������� ��θ� ��������
    static List<Tile> ReconstructPath(Dictionary<Tile, Tile> cameFrom, Tile current)
    {
        List<Tile> path = new();

        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Reverse(); // �Ųٷ� ���������Ƿ� ���� ����
        return path;
    }

    // A* ���ο��� ���Ǵ� ��� Ŭ���� (Ÿ�� + fScore)
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

    // fScore �������� TileNode�� �����ϱ� ���� Comparer
    private class TileNodeComparer : IComparer<TileNode>
    {
        public int Compare(TileNode a, TileNode b)
        {
            int fCompare = a.FScore.CompareTo(b.FScore);
            if (fCompare != 0)
                return fCompare;

            // fScore�� ������ ���������� �� (�ߺ� ����)
            return a.Tile.GetHashCode().CompareTo(b.Tile.GetHashCode());
        }
    }
}
