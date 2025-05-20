using UnityEngine;
using System.Collections.Generic;
public class Fence : PlaceableBase
{
    public FenceData fenceData { get; private set; }

    public override void SetData(ScriptableObject data) => fenceData = data as FenceData;
    public override void Sell()
    {
        GameManager.instance.AddGold(GetSellPrice());
        TileUtility.MarkTilesOccupied(occupiedTiles, false);
        Destroy(gameObject);
    }
    public override void Upgrade()
    {
        // ��Ÿ�� ���׷��̵� ����
    }

    #region ####----- ��ȯ �޼��� -----####
    public int GetSellPrice() => 50;
    public string GetDescription() => fenceData.description;
    #endregion
}
