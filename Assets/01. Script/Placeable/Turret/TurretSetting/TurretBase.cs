using UnityEngine;

public abstract class TurretBase : PlaceableBase, FogRevealFog
{
    public TurretData turretData { get; private set; }
    public int CurrentLevel { get; private set; } = 1;

    public float viewRange = 10f;

    [SerializeField] GameObject selectBox;
    public override void SetData(ScriptableObject data)
    {
        turretData = data as TurretData;
        CurrentLevel = 1;

        FogOfWarSystem.Instance?.Register(this);
    }

    public override void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (GameManager.instance.SpendGold(cost))
            CurrentLevel++;
    }

    public override void Sell()
    {
        GameManager.instance.AddGold(GetSellPrice());

        // 점유 해제: TileData 직접 처리
        foreach (var tile in occupiedTiles)
        {
            if (tile != null)
            {
                tile.TargetingTurret = null;
                tile.IsReserved = false;
            }
        }
        FogOfWarSystem.Instance?.Unregister(this);
        Destroy(this.gameObject);
    }

    public void ShowSelectBox() => selectBox?.SetActive(true);
    public void HideSelectBox() => selectBox?.SetActive(false);

    public void RevealFog()
    {
      //  FogOfWarSystem.Instance.RevealArea(this.transform.position, viewRange);
       FogOfWarSystem.Instance.RevealAreaGradient(transform.position, viewRange);
    }

    #region 스탯 계산 메서드
    public int GetDamage()
        => turretData.baseDamage + (turretData.damageGrowth * (CurrentLevel - 1));

    public float GetRange()
    {
        float tileRange = turretData.baseAttackRange + (turretData.attackRangeGrowth * (CurrentLevel - 1));
        return tileRange * TileGridManager.Instance.cubeSize;
    }

    public float GetAttackRate()
    {
        float value = turretData.baseAttackRate - (turretData.attackRateGrowth * (CurrentLevel - 1) * 0.2f);
        return turretData.turretType switch
        {
            TurretType.Gatling => Mathf.Max(0.01f, value),
            TurretType.Laser => 0,
            TurretType.Cannon => value,
            _ => value
        };
    }

    
    public int GetUpgradeCost() => 100 + (CurrentLevel * 50);
    public int GetSellPrice() => 50 + (CurrentLevel * 25);
    public string GetDescription() => turretData.description;

    #endregion
}
