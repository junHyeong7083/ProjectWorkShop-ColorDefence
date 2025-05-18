using System.Collections;
using UnityEngine;

public abstract class TurretBase : MonoBehaviour
{
    // 터렛의 so 참조
    public TurretData turretData { get; private set; } 
    public int CurrentLevel { get; private set; } = 1;

    // 공격 루프를 돌리기 위한 핸들
    Coroutine attackRoutine;

    // 공격 루프
    IEnumerator StartAttack()
    {
        while (true)
        {
            // 1. 타겟 찾고 공격
            yield return PerformActionRoutine(); 

            // 쿨타임 대기
            yield return new WaitForSeconds(GetAttackRate()); 
        }
    }


    // 터렛 데이터를 설정 -> 공격 루프
    public void SetTurretData(TurretData _turretData)
    {
        turretData = _turretData;
        CurrentLevel = 1;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(StartAttack());
    }

    // 타워 업그레이드
    public void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (GameManager.instance.SpendGold(cost))
            CurrentLevel++;
    }

    // 타워 판매 -> 파괴 
    public void Sell()
    {
        int refund = GetSellPrice();
        GameManager.instance.AddGold(refund);
        Destroy(this.gameObject);
    }

    // 터렛 클릭시 ui출력 && 범위 시각화
    private void OnMouseDown()
    {
        TurretManager.Instance.SelectTurret(this);
        GetComponent<TurretRangeVisualizer>().Show(GetRange());
    }
    private void OnMouseUp()
    {
        GetComponent<TurretRangeVisualizer>().Hide();
    }
   
    // 공격 행동 정의를 위한 추상 코루틴
    protected virtual IEnumerator PerformActionRoutine()
    {
        yield break;
    }

   // protected abstract void PerformAction();


    // 공격 메서드
    protected abstract void AttackEnemy();
    protected abstract void AttackTile();



    // 가장 가까운 적 유닛 탐색
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

    // 가장 오래된 적 타일 탐색( 다른 터렛이 조준 -> 제외 )
    protected virtual Tile FindOldestEnemyTile()
    {
        float range = GetRange();
        Vector3 center = transform.position;

        Tile result = null;
        float oldestTime = Mathf.Infinity;

        var nearbyTiles = TileGridManager.Instance.GetTilesInRange(center, range);
        foreach (var tile in nearbyTiles)
        {
            if (tile.ColorState != TileColorState.Enemy) continue;
            
            // Bumping 애니메이션 재생중 => 이거 안막으니 자꾸 통통튀면면서 시각적으로 맛없어짐
            if (tile.IsBumping) continue;

            // 다른터렛이 타겟팅 && 타겟팅중인게 내가 아니라면 continue
            if (tile.TargetingTurret != null && tile.TargetingTurret != this) continue;

            if(tile.LastChangedTime < oldestTime)
            {
                oldestTime = tile.LastChangedTime;
                result = tile;
            }
        }
        if (result != null)
            result.TargetingTurret = this;


        return result;
    }


    // XZ 평면 기준 거리 계산 커스텀함수
    protected static float DistanceXZ(Vector3 a, Vector3 b)
    {
        Vector2 aXZ = new Vector2(a.x, a.z);
        Vector2 bXZ = new Vector2(b.x, b.z);
        return Vector2.Distance(aXZ, bXZ);
    }


    #region ####----- 반환 메서드 -----####
    public int GetDamage() => turretData.baseDamage + (turretData.damageGrowth * (CurrentLevel - 1));
    public float GetRange() => turretData.baseAttackRange + (turretData.attackRangeGrowth * (CurrentLevel - 1));
    public float GetAttackRate()
    {
        // 우선 테스트용으로 0.2f 만큼만 줄어들게
        float value = turretData.baseAttackRate - (turretData.attackRateGrowth * (CurrentLevel - 1)*0.2f);

        if (turretData.turretType != TurretType.Laser)
            value = Mathf.Max(0.3f, value); // 최소공격속도 0.3초 주기
        return value;
    }

    public int GetUpgradeCost() => 100 + (CurrentLevel * 50);
    public int GetSellPrice() => 50 + (CurrentLevel * 25);
    #endregion
}
