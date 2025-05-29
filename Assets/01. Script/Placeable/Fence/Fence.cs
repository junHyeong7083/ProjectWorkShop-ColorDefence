using System.Collections.Generic;
using UnityEngine;

public class Fence : PlaceableBase
{
    public FenceData fenceData { get; private set; }

    public override void SetData(ScriptableObject data)
    {
        fenceData = data as FenceData;
    }

    public override void Sell()
    {
        GameManager.instance.AddGold(GetSellPrice());

        // 점유 해제 처리
        foreach (var tile in occupiedTiles)
        {
            if (tile != null)
            {
                tile.TargetingTurret = null;
                tile.OccupyingFence = null; 
                tile.IsReserved = false;
            }
        }

        Destroy(gameObject);
    }
    public override void Upgrade()
    {
        // Fence는 업그레이드 없음
    }

    #region 반환 메서드
    public int GetSellPrice() => 50;
    public string GetDescription() => fenceData.description;
    #endregion
}
