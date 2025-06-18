using System.Collections.Generic;

public static class TileUtility
{
    // tileData 리스트를 받아서 점유 상태를 설정하거나 해제하는 함수
    public static void MarkTilesOccupied(List<TileData> tiles, bool occupied)
    {
        foreach (var tile in tiles)
        {
            if (occupied)
            {
               // tile.OccupyingFence = new DummyFence(); // 임시 점유용 더미 울타리 객체
            }
            else
            {
                tile.OccupyingFence = null;
            }
        }
    }
}
