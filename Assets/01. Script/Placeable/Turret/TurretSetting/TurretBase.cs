using UnityEngine;

public abstract class TurretBase : PlaceableBase
{
    public TurretData turretData { get; private set; }
    public int CurrentLevel { get; private set; } = 1;

    [SerializeField] GameObject selectBox;
    
    // 터렛 설치 시 호출
    public override void SetData(ScriptableObject data)
    {
        turretData = data as TurretData;
        CurrentLevel = 1;
    }

    // 업그레이드 시도 (골드 소모)
    public override void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (GameManager.instance.SpendGold(cost))
            CurrentLevel++;
    }

    // 판매 시 골드 회수 및 타일 반환
    public override void Sell()
    {
        GameManager.instance.AddGold(GetSellPrice());
        TileUtility.MarkTilesOccupied(occupiedTiles, false);
        Destroy(this.gameObject);
    }


    public void ShowSelectBox() => selectBox.gameObject.SetActive(true);
    public void HideSelectBox() => selectBox.gameObject.SetActive(false);


    #region 스탯 계산 메서드

    public int GetDamage()
        => turretData.baseDamage + (turretData.damageGrowth * (CurrentLevel - 1));

    public float GetRange()
    {
        float tileRange = turretData.baseAttackRange + (turretData.attackRangeGrowth * (CurrentLevel - 1));
        return tileRange * TileGridManager.Instance.cubeSize; // <- 타일 수 → 월드 거리
    }

    public float GetAttackRate()
    {
        float value = turretData.baseAttackRate - (turretData.attackRateGrowth * (CurrentLevel - 1) * 0.2f);
        switch(turretData.turretType)
        {
            case TurretType.Gatling:
                return Mathf.Max(0.5f, value);
            case TurretType.Laser:
                return 0;
            case TurretType.Cannon:
                return value;
        }
        return value;
    }

    public int GetUpgradeCost() => 100 + (CurrentLevel * 50);

    public int GetSellPrice() => 50 + (CurrentLevel * 25);

    public string GetDescription() => turretData.description;

    #endregion
}
