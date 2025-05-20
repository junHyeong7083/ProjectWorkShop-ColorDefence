using UnityEngine;
using System.Collections.Generic;
public class Fence : MonoBehaviour
{
    [HideInInspector]
    public FenceData fenceData{ get; private set; }
    public List<Tile> occupiedTiles = new();

    public void SetFenceData(FenceData _data) => fenceData = _data;
    public void Sell()
    {
        int refund = GetSellPrice();
        GameManager.instance.AddGold(refund);

        TileUtility.MarkTilesOccupied(occupiedTiles, false);
        Destroy(gameObject);
    }

    #region ####----- 반환 메서드 -----####
    public int GetSellPrice() => 50;

    #endregion
}
