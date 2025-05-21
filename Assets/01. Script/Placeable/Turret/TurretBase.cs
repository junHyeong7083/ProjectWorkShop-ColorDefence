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


    // 터렛 설치 시 호출. ScriptableObject 설정 및 공격 루프 시작
    public override void SetData(ScriptableObject data)
    {
        turretData = data as TurretData;
        CurrentLevel = 1;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(StartAttack());
    }

    // 업그레이드 시도 (골드가 충분할 경우만)
    public override void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (GameManager.instance.SpendGold(cost))
            CurrentLevel++;
    }

    // 판매 시 골드 환급 및 타일 점유 해제 후 오브젝트 삭제
    public override void Sell()
    {
        GameManager.instance.AddGold(GetSellPrice());
        TileUtility.MarkTilesOccupied(occupiedTiles, false);
        Destroy(this.gameObject);
    }

    // 클릭 시 UI 선택 처리 및 범위 시각화 표시
    protected override void OnMouseDown()
    {
        base.OnMouseDown();
        GetComponent<TurretRangeVisualizer>().Show(GetRange());
    }

    private void OnMouseUp()
    {
        GetComponent<TurretRangeVisualizer>().Hide();
    }

    // 공격 루틴은 자식에서 오버라이드해야 함
    protected virtual IEnumerator PerformActionRoutine() => null;

    protected abstract void AttackEnemy();
    protected abstract void AttackTile();

    // 가장 가까운 적 유닛 탐색
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

    // 유효한 적인지 검사 
    protected virtual bool IsValidEnemy(GameObject enemy)
    {
        return enemy != null;
    }

    // 가장 오래된 Enemy 타일 탐색 ( 가중치 기준 : EvaluateTileScore함수 )
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


    // 유효한 Enemy 타일인지 검사 (중복 타겟팅 방지 포함)
    protected virtual bool IsValidTargetTile(Tile tile)
    {
        if (tile.ColorState != TileColorState.Enemy) return false;
        if (tile.IsBumping || tile.IsReserved) return false;
        if (tile.TargetingTurret != null && tile.TargetingTurret != this) return false;

        return true;
    }

    // Enemy 타일 우선순위 평가 함수 (가중치 기반)
    protected float EvaluateTileScore(Tile tile)
    {
        return -(Time.time - tile.LastChangedTime);
    }

    #region 반환 메서드
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
