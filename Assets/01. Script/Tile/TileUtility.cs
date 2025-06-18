using System.Collections.Generic;

public static class TileUtility
{
    // tileData ����Ʈ�� �޾Ƽ� ���� ���¸� �����ϰų� �����ϴ� �Լ�
    public static void MarkTilesOccupied(List<TileData> tiles, bool occupied)
    {
        foreach (var tile in tiles)
        {
            if (occupied)
            {
               // tile.OccupyingFence = new DummyFence(); // �ӽ� ������ ���� ��Ÿ�� ��ü
            }
            else
            {
                tile.OccupyingFence = null;
            }
        }
    }
}
