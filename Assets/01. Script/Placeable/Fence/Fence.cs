using System.Collections.Generic;
using UnityEngine;

public class Fence : PlaceableBase, IFogRevealer
{
    public FenceData fenceData { get; private set; }
    public int FogRevealerIndex { get; set; } = -1;

    public float viewRange => 10f;
    public override void SetData(ScriptableObject data)
    {
        fenceData = data as FenceData;

        FogOfWarSystem.Instance?.Register(this);
        FogOfWarSystem.Instance?.RegisterAssetFog(this);
    }

    public override void Sell()
    {
      // GameManager.instance.AddGold(GetSellPrice());

        // ���� ���� ó��
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
    public void RevealFog()
    {
        //  FogOfWarSystem.Instance.RevealArea(this.transform.position, viewRange);
        FogOfWarSystem.Instance.RevealAreaGradient(transform.position, viewRange);
    }

    public override void Upgrade()
    {
        // Fence�� ���׷��̵� ����
    }

    #region ��ȯ �޼���
    public int GetSellPrice() => 50;
    public string GetDescription() => fenceData.description;
    #endregion
}
