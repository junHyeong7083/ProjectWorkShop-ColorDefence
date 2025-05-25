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
    private TurretBase turret; // �ͷ� ����
    private TurretTargetSelector selector; // ��/Ÿ�� Ž�� ������Ʈ
    private TurretRotationController rotator; // ȸ�� ó�� ������Ʈ

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

    // ������Ʈ Ȱ��ȭ �� ���� ���� ����
    private void OnEnable()
    {
        StartCombat();
    }

    // ���� ���� ���� 
    public void StartCombat()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(CombatRoutine());
    }

    // ���� ���� �ڷ�ƾ
    // 1. ������ ���õ� ������ ���
    // 2. ���� �ֱ�� PerformAction ����
    private IEnumerator CombatRoutine()
    {
        //Debug.Log("TurretCombatLoop PerformActionRoutine ȣ���");

        while (turret.turretData == null)
            yield return null;
        while (true)
        {
            yield return PerformAction();
            yield return new WaitForSeconds(turret.GetAttackRate());
        }
    }

    // ���� �׼� Ÿ�Կ� ���� Ÿ�� Ž�� + ȸ�� + ���� ����
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

            // ȸ�� �� �׾��� ���� ����
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

            // ���� �Ϸ� �� currentTile ����
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
