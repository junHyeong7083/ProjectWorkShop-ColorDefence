using System.Collections;
using UnityEngine;

public class GatlingTurret : TurretBase
{
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform headToRotate;
    [SerializeField] private float rotateSpeed = 10f;

    private GameObject currentTarget;
    private Tile targetTile;
    
    protected override IEnumerator PerformActionRoutine()
    {
        if (turretData.actionType == TurretActionType.AttackEnemy)
        {
            currentTarget = FindClosestEnemy();
            if (currentTarget == null) yield break;

            yield return RotateToTarget(currentTarget.transform.position);

            AttackEnemy();
        }
        else if (turretData.actionType == TurretActionType.AttackTile)
        {
            targetTile = FindOldestEnemyTile();
            if (targetTile == null) yield break;

            Vector3 targetPos = new Vector3(
                (targetTile.GridPos.x + 0.5f) * TileGridManager.Instance.cubeSize,
                0,
                (targetTile.GridPos.y + 0.5f) * TileGridManager.Instance.cubeSize
            );

            yield return RotateToTarget(targetPos);
            AttackTile();
        }
    }

    protected override void AttackEnemy()
    {
        if (currentTarget == null) return;

        var health = currentTarget.GetComponent<EnemyHealth>();
        if (health == null || health.currentHp <= 0) return;

        Vector3 dir = (currentTarget.transform.position - firePoint.position).normalized;

        // damage에 현재레벨 공격력을 적용시킴
        int damage = GetDamage();
        BulletPool.Instance.GetEnemyBullet(firePoint.position, dir, damage);

    }



    protected override void AttackTile()
    {
        if (targetTile == null) return;

        Tile tileToHit = targetTile;
        Vector3 dir = (targetTile.CenterWorldPos - firePoint.position).normalized;

        BulletPool.Instance.GetTileBullet(firePoint.position, dir, () =>
        {
            if (tileToHit != null && tileToHit.ColorState == TileColorState.Enemy)
            {
                tileToHit.SetColor(TileColorState.Player);
                tileToHit.AnimateBump();
            }

            if (tileToHit != null)
            {
                tileToHit.Release();
                tileToHit.TargetingTurret = null;

                EffectManager.Instance.PlayEffect(
                    TurretType.Gatling,
                    TurretActionType.AttackTile,
                    tileToHit.CenterWorldPos
                );
            }
        });
    }

    private IEnumerator RotateToTarget(Vector3 targetPos)
    {
        Vector3 direction = targetPos - headToRotate.position;
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.001f) yield break;

        Quaternion targetRot = Quaternion.LookRotation(direction);

        while (Quaternion.Angle(headToRotate.rotation, targetRot) > 1f)
        {
            headToRotate.rotation = Quaternion.Slerp(
                headToRotate.rotation,
                targetRot,
                Time.deltaTime * rotateSpeed
            );

            yield return null;
        }
    }

}
