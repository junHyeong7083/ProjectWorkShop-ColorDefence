using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.RenderGraphModule;



public interface ITurretShooter
{
    void ShootAtEnemy(GameObject enemy);
    void ShootAtTile(Tile tile);
}




[RequireComponent(typeof(TurretBase))]
[RequireComponent(typeof(TurretTargetSelector))]
[RequireComponent(typeof(TurretRotationController))]
public class TurretCombatLoop : MonoBehaviour
{
    private TurretBase turret; // 터렛 정보
    private TurretTargetSelector selector; // 적/타일 탐색 컴포넌트
    private TurretRotationController rotator; // 회전 처리 컴포넌트

    private ITurretShooter shooter;

    private Coroutine attackRoutine;
    private GameObject currentEnemy;
    private Tile currentTile;
    private void Awake()
    {
        turret = GetComponent<TurretBase>();
        selector = GetComponent<TurretTargetSelector>();
        rotator = GetComponent<TurretRotationController>();
        shooter = GetComponent<ITurretShooter>();
    }

    // 컴포넌트 활성화 시 공격 루프 시작
    private void OnEnable()
    {
        StartCombat();
    }

    // 공격 루프 시작 
    public void StartCombat()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(CombatRoutine());
    }

    // 공격 루프 코루틴
    // 1. 데이터 세팅될 때까지 대기
    // 2. 일정 주기로 PerformAction 실행
    private IEnumerator CombatRoutine()
    {
        //Debug.Log("TurretCombatLoop PerformActionRoutine 호출됨");

        while (turret.turretData == null)
            yield return null;
        while (true)
        {
            yield return PerformAction();
            yield return new WaitForSeconds(turret.GetAttackRate());
        }
    }

    // 현재 액션 타입에 따라 타겟 탐색 + 회전 + 공격 수행
    private IEnumerator PerformAction()
    {
        if (turret.turretData.actionType == TurretActionType.AttackEnemy)
        {
            if (!IsValidEnemy(currentEnemy))
                currentEnemy = selector.FindClosestEnemy();

            if (!IsValidEnemy(currentEnemy))
            {
                currentEnemy = null;
                yield break;
            }

           /* float distanceSq = (currentEnemy.transform.position - this.transform.position).sqrMagnitude;
            float range = turret.turretData.baseAttackRange;
            if (distanceSq > range * range)
                yield break;
*/


            yield return rotator.RotateTo(currentEnemy.transform.position);

            // 회전 중 죽었을 수도 있음
            if (!IsValidEnemy(currentEnemy))
            {
                currentEnemy = null;
                yield break;
            }

            shooter.ShootAtEnemy(currentEnemy);
        }
        else if (turret.turretData.actionType == TurretActionType.AttackTile)
        {
            if (currentTile == null || !IsValidTile(currentTile))
            {
                currentTile = selector.FindBestEnemyTile();
                if (currentTile == null) yield break;

                yield return rotator.RotateTo(currentTile.CenterWorldPos);
            }

            shooter.ShootAtTile(currentTile);

            // 공격 완료 시 currentTile 해제
            if (currentTile.TargetingTurret == null)
                currentTile = null;
        }
    }

    private bool IsValidEnemy(GameObject enemy )
    {
        if(enemy == null) return false;

        var health = enemy.GetComponent<EnemyHealth>();
        return health != null && health.currentHp > 0 && enemy.activeInHierarchy;
    }


    private bool IsValidTile(Tile tile)
    {
        return tile != null &&
               tile.ColorState == TileColorState.Enemy &&
               !tile.IsBumping &&
               tile.TargetingTurret == turret;
    }

}
