using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public abstract class TurretBase : PlaceableBase
{
    public TurretData turretData { get; private set; }
    public int CurrentLevel { get; private set; } = 1;

    Coroutine attackRoutine;

    IEnumerator StartAttack()
    {
        while (true)
        {
            yield return PerformActionRoutine();
            yield return new WaitForSeconds(GetAttackRate());
        }
    }


    // �ͷ� ��ġ �� ȣ��. ScriptableObject ���� �� ���� ���� ����
    public override void SetData(ScriptableObject data)
    {
        turretData = data as TurretData;
        CurrentLevel = 1;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(StartAttack());
    }

    // ���׷��̵� �õ� (��尡 ����� ��츸)
    public override void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (GameManager.instance.SpendGold(cost))
            CurrentLevel++;
    }

    // �Ǹ� �� ��� ȯ�� �� Ÿ�� ���� ���� �� ������Ʈ ����
    public override void Sell()
    {
        GameManager.instance.AddGold(GetSellPrice());
        TileUtility.MarkTilesOccupied(occupiedTiles, false);
        Destroy(this.gameObject);
    }

    // Ŭ�� �� UI ���� ó�� �� ���� �ð�ȭ ǥ��
    protected override void OnMouseDown()
    {
        base.OnMouseDown();
        GetComponent<TurretRangeVisualizer>().Show(GetRange());
    }

    private void OnMouseUp()
    {
        GetComponent<TurretRangeVisualizer>().Hide();
    }

    // ���� ��ƾ�� �ڽĿ��� �������̵��ؾ� ��
    protected virtual IEnumerator PerformActionRoutine() => null;

    protected abstract void AttackEnemy();
    protected abstract void AttackTile();

    // ���� ����� �� ���� Ž��
    protected virtual GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject bestTarget = null;
        float bestScore = float.MaxValue;
        float rangeSqr = GetRange() * GetRange();
        Vector3 origin = transform.position;

        foreach (var enemy in enemies)
        {
            if (!IsValidEnemy(enemy)) continue;

            float distSqr = (enemy.transform.position - origin).sqrMagnitude;
            if (distSqr > rangeSqr) continue;

            if (distSqr < bestScore)
            {
                bestScore = distSqr;
                bestTarget = enemy;
            }
        }

        return bestTarget;
    }

    // ��ȿ�� ������ �˻� 
    protected virtual bool IsValidEnemy(GameObject enemy)
    {
        return enemy != null;
    }

    // ���� ������ Enemy Ÿ�� Ž�� ( ����ġ ���� : EvaluateTileScore�Լ� )
    protected virtual Tile FindOldestEnemyTile()
    {
        float range = GetRange();
        Vector3 center = transform.position;

        Tile bestTile = null;
        float bestScore = float.MaxValue;

        var tilesInRange = TileGridManager.Instance.GetTilesInRange(center, range);
        foreach (var tile in tilesInRange)
        {
            if (!IsValidTargetTile(tile)) continue;

            float score = EvaluateTileScore(tile);
            if (score < bestScore)
            {
                bestScore = score;
                bestTile = tile;
            }
        }

        if (bestTile != null)
        {
            bestTile.TargetingTurret = this;
            bestTile.Reserve();
        }

        return bestTile;
    }


    // ��ȿ�� Enemy Ÿ������ �˻� (�ߺ� Ÿ���� ���� ����)
    protected virtual bool IsValidTargetTile(Tile tile)
    {
        if (tile.ColorState != TileColorState.Enemy) return false;
        if (tile.IsBumping || tile.IsReserved) return false;
        if (tile.TargetingTurret != null && tile.TargetingTurret != this) return false;

        return true;
    }

    // Enemy Ÿ�� �켱���� �� �Լ� (����ġ ���)
    protected float EvaluateTileScore(Tile tile)
    {
        return -(Time.time - tile.LastChangedTime);
    }

    #region ��ȯ �޼���
    public int GetDamage() => turretData.baseDamage + (turretData.damageGrowth * (CurrentLevel - 1));
    public float GetRange() => turretData.baseAttackRange + (turretData.attackRangeGrowth * (CurrentLevel - 1));

    public float GetAttackRate()
    {
        float value = turretData.baseAttackRate - (turretData.attackRateGrowth * (CurrentLevel - 1) * 0.2f);
        return turretData.turretType != TurretType.Laser ? Mathf.Max(0.3f, value) : value;
    }

    public int GetUpgradeCost() => 100 + (CurrentLevel * 50);
    public int GetSellPrice() => 50 + (CurrentLevel * 25);
    public string GetDescription() => turretData.description;
    #endregion
}
