using UnityEngine;
using System.Collections;

public interface ITurretShooter
{
    void ShootAtEnemy(GameObject enemy);
    bool IsReloading { get; }
}

[RequireComponent(typeof(TurretBase))]
[RequireComponent(typeof(TurretTargetSelector))]
[RequireComponent(typeof(TurretRotationController))]
public class TurretCombatLoop : MonoBehaviour
{
    private TurretBase turret;
    private TurretTargetSelector selector;
    private TurretRotationController rotator;
    private ITurretShooter shooter;

    private Coroutine attackRoutine;
    private GameObject currentEnemy;

    private void Awake()
    {
        turret = GetComponent<TurretBase>();
        selector = GetComponent<TurretTargetSelector>();
        rotator = GetComponent<TurretRotationController>();
        shooter = GetComponent<ITurretShooter>();
    }

    private void OnEnable()
    {
        StartCombat();
    }

    public void StartCombat()
    {
        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(CombatRoutine());
    }

    private IEnumerator CombatRoutine()
    {
        while (turret.turretData == null)
            yield return null;

        while (true)
        {
            bool didAttack = false;
            yield return StartCoroutine(PerformAction(result => didAttack = result));

            if (didAttack)
                yield return YieldCache.WaitForSeconds(turret.GetAttackRate());
            else
                yield return YieldCache.WaitForSeconds(0.05f);
        }
    }

    private IEnumerator PerformAction(System.Action<bool> callback)
    {
        if (turret.turretData.actionType == TurretActionType.AttackEnemy)
        {
            if (!IsValidEnemy(currentEnemy))
                currentEnemy = selector.FindClosestEnemy();

            if (!IsValidEnemy(currentEnemy))
            {
                ForceDisableLaser(); // 
                currentEnemy = null;
                callback(false);
                yield break;
            }
            if (shooter.IsReloading)
            {
                callback(false);
                yield break;
            }

            yield return rotator.RotateTo(currentEnemy.transform.position);

            if (!IsValidEnemy(currentEnemy))
            {
                ForceDisableLaser(); //
                currentEnemy = null;
                callback(false);
                yield break;
            }

            shooter.ShootAtEnemy(currentEnemy);
            callback(true);
        }
    }

    private bool IsValidEnemy(GameObject enemy)
    {
        if (enemy == null || !enemy.activeInHierarchy)
            return false;

        var health = enemy.GetComponent<EnemyHealth>();
        if (health == null || health.currentHp <= 0)
            return false;

        float range = turret.GetRange();
        float rangeSqr = range * range;

        Vector3 turretPos = transform.position;
        Vector3 enemyPos = enemy.transform.position;

        float distSqr = (enemyPos - turretPos).sqrMagnitude;
        return distSqr <= rangeSqr;
    }


    /// 현재 shooter가 LaserShooter일 경우 강제로 레이저 끄기
    private void ForceDisableLaser()
    {
        if (shooter is LaserShooter laserShooter)
            laserShooter.DisableLaser();
    }


    private void Update()
    {
        if(shooter is GatlingShooter gatlingShooter)
        {
            gatlingShooter.Tick(Time.deltaTime);
        }
    }
}
