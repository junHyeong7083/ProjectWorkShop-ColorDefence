using System.Collections.Generic;

public static class TileUtility
{
    public static void MarkTilesOccupied(List<Tile> _tiles, bool _occupied)
    {
        foreach (var tile in _tiles) if (tile != null) tile.IsOccupied = _occupied;
    }
}


