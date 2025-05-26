using UnityEngine;

public abstract class TurretBase : PlaceableBase
{
    public TurretData turretData { get; private set; }
    public int CurrentLevel { get; private set; } = 1;

    [SerializeField] GameObject selectBox;
    
    // �ͷ� ��ġ �� ȣ��
    public override void SetData(ScriptableObject data)
    {
        turretData = data as TurretData;
        CurrentLevel = 1;
    }

    // ���׷��̵� �õ� (��� �Ҹ�)
    public override void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (GameManager.instance.SpendGold(cost))
            CurrentLevel++;
    }

    // �Ǹ� �� ��� ȸ�� �� Ÿ�� ��ȯ
    public override void Sell()
    {
        GameManager.instance.AddGold(GetSellPrice());
        TileUtility.MarkTilesOccupied(occupiedTiles, false);
        Destroy(this.gameObject);
    }


    public void ShowSelectBox() => selectBox.gameObject.SetActive(true);
    public void HideSelectBox() => selectBox.gameObject.SetActive(false);


    #region ���� ��� �޼���

    public int GetDamage()
        => turretData.baseDamage + (turretData.damageGrowth * (CurrentLevel - 1));

    public float GetRange()
    {
        float tileRange = turretData.baseAttackRange + (turretData.attackRangeGrowth * (CurrentLevel - 1));
        return tileRange * TileGridManager.Instance.cubeSize; // <- Ÿ�� �� �� ���� �Ÿ�
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
