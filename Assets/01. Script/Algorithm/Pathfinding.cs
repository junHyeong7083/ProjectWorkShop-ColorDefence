using System.Collections.Generic;

public static class Pathfinding
{
    public static List<Tile> FindPath(Tile startTile, Tile goalTile)
    {
        //// ��Ÿ�� ��ġ������ �ٽ� VISITED�� �ʱ�ȭ�ؾ��ҵ�? => ���� �Դ����� �ǵ��ư����ϴ� ��Ȳ
        //// ���� �ϱ�


        // BFS Ž���� ���� ť
        Queue<Tile> queue = new();

        // �� Ÿ���� ��� �Դ��� ����ϴ� �� (��������)
        Dictionary<Tile, Tile> cameFrom = new();

        // �̹� �湮�� Ÿ���� ��� (�ߺ� ����)
        HashSet<Tile> visited = new();

        // ���� Ÿ�Ϸ� �ʱ�ȭ
        queue.Enqueue(startTile);
        visited.Add(startTile);

        // BFS Ž�� ����
        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();

            // ��ǥ�� ���������� Ž�� ����
            if (current == goalTile) break;

            // �����¿� ���� Ÿ���� Ž��
            foreach (var neighbor in GetNeighbors(current))
            {
                // �湮���� �ʾҰ�, ������ ���� Ÿ�ϸ� ť�� �߰�
                if (!visited.Contains(neighbor) && !neighbor.IsOccupied)
                {
                    queue.Enqueue(neighbor);
                    visited.Add(neighbor);

                    // �̿� Ÿ�Ϸ� ���� ���� � Ÿ�Ͽ��� �Դ��� ���
                    cameFrom[neighbor] = current;
                }
            }
        }

        // ��ǥ���� �������� ���ߴٸ� null ��ȯ (����)
        if (!cameFrom.ContainsKey(goalTile))
            return null;

        // ��ǥ���� ���۱��� �������Ͽ� ��θ� �����
        List<Tile> path = new();
        Tile t = goalTile;

        while (t != startTile)
        {
            path.Add(t);
            t = cameFrom[t];
        }

        // ��θ� �ݴ�� ������� (���� �� ��ǥ ����)
        path.Reverse();
        return path;
    }

    /// <summary>
    /// �־��� Ÿ���� �����¿� ���� Ÿ���� ��ȯ�Ѵ�.
    /// </summary>
    static List<Tile> GetNeighbors(Tile tile)
    {
        List<Tile> result = new();

        // ���� Ÿ�� ��ǥ
        int x = tile.GridPos.x;
        int z = tile.GridPos.y;

        TileGridManager g = TileGridManager.Instance;

        // ���� �Լ��� ���� ��ǥ ���ϰ� Ÿ�� ���� �� �߰�
        void Add(int dx, int dz)
        {
            Tile t = g.GetTile(x + dx, z + dz);
            if (t != null)
                result.Add(t);
        }

        // �����¿� 4����
        Add(0, 1);   // ����
        Add(0, -1);  // �Ʒ���
        Add(-1, 0);  // ����
        Add(1, 0);   // ������

        return result;
    }
}