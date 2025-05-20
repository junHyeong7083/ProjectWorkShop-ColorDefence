using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public abstract class TurretBase : PlaceableBase
{
    // �ͷ��� so ����
    public TurretData turretData { get; private set; } 
    public int CurrentLevel { get; private set; } = 1;

    // ���� ������ ������ ���� �ڵ�
    Coroutine attackRoutine;

    // ���� ����
    IEnumerator StartAttack()
    {
        while (true)
        {
            // 1. Ÿ�� ã�� ����
            yield return PerformActionRoutine(); 

            // ��Ÿ�� ���
            yield return new WaitForSeconds(GetAttackRate()); 
        }
    }


    // �ͷ� �����͸� ���� -> ���� ����
    public override void SetData(ScriptableObject data)
    {
        turretData = data as TurretData;
        CurrentLevel = 1;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(StartAttack());
    }

    // Ÿ�� ���׷��̵�
    public override void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (GameManager.instance.SpendGold(cost))
            CurrentLevel++;
    }

    // Ÿ�� �Ǹ� -> �ı� 
    public override void Sell()
    {
        GameManager.instance.AddGold(GetSellPrice());
        TileUtility.MarkTilesOccupied(occupiedTiles, false);
        Destroy(this.gameObject);
    }

    // �ͷ� Ŭ���� ui��� && ���� �ð�ȭ
    protected override void OnMouseDown()
    {
        base.OnMouseDown();
        GetComponent<TurretRangeVisualizer>().Show(GetRange());
    }
    private void OnMouseUp()
    {
        GetComponent<TurretRangeVisualizer>().Hide();
    }
   
    // ���� �ൿ ���Ǹ� ���� �߻� �ڷ�ƾ
    protected virtual IEnumerator PerformActionRoutine()
    {
        yield break;
    }

   // protected abstract void PerformAction();


    // ���� �޼���
    protected abstract void AttackEnemy();
    protected abstract void AttackTile();



    // ���� ����� �� ���� Ž��
    protected virtual GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDist = Mathf.Infinity;

        foreach (var enemy in enemies)
        {
            float dist = DistanceXZ(transform.position, enemy.transform.position);
            if (dist < minDist && dist <= GetRange())
            {
                closest = enemy;
                minDist = dist;
            }
        }

        return closest;
    }

    // ���� ������ �� Ÿ�� Ž��( �ٸ� �ͷ��� ���� => ���� )
    protected virtual Tile FindOldestEnemyTile()
    {
        float range = GetRange();
        float rangeSqr = range * range;
        Vector3 center = transform.position;

        Tile result = null;
        float bestScore = float.MaxValue;

        var nearbyTiles = TileGridManager.Instance.GetTilesInRange(center, range);
        foreach (var tile in nearbyTiles)
        {
            if (tile.ColorState != TileColorState.Enemy || tile.IsBumping || tile.IsReserved) continue;
            if (tile.TargetingTurret != null && tile.TargetingTurret != this) continue;


            float dx = tile.CenterWorldPos.x - center.x;
            float dz = tile.CenterWorldPos.z - center.z;
            float distSqr = dx * dx + dz * dz;

            if (distSqr > rangeSqr) continue;

            float score = tile.LastChangedTime * 1000f + distSqr;

            if (score < bestScore)
            {
                bestScore = score;
                result = tile;
            }
        }

        if (result != null)
        {
            result.TargetingTurret = this;
            result.Reserve();
        }
            

        return result;
    }


    // XZ ��� ���� �Ÿ� ��� Ŀ�����Լ�
    protected static float DistanceXZ(Vector3 a, Vector3 b)
    {
        Vector2 aXZ = new Vector2(a.x, a.z);
        Vector2 bXZ = new Vector2(b.x, b.z);
        return Vector2.Distance(aXZ, bXZ);
    }


    #region ####----- ��ȯ �޼��� -----####
    public int GetDamage() => turretData.baseDamage + (turretData.damageGrowth * (CurrentLevel - 1));
    public float GetRange() => turretData.baseAttackRange + (turretData.attackRangeGrowth * (CurrentLevel - 1));
    public float GetAttackRate()
    {
        // �켱 �׽�Ʈ������ 0.2f ��ŭ�� �پ���
        float value = turretData.baseAttackRate - (turretData.attackRateGrowth * (CurrentLevel - 1)*0.2f);

        if (turretData.turretType != TurretType.Laser)
        {
            value = Mathf.Max(0.3f, value); // �ּҰ��ݼӵ� 0.3�� �ֱ�

        }
        return value;
    }

    public int GetUpgradeCost() => 100 + (CurrentLevel * 50);
    public int GetSellPrice() => 50 + (CurrentLevel * 25);

    public string GetDescription() => turretData.description;
    #endregion
}
