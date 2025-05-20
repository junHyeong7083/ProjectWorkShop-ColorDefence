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

        Vector3 dir = (currentTarget.transform.position - firePoint.position).normalized;
        BulletPool.Instance.GetEnemyBullet(firePoint.position, dir);

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

    /*  protected override void PaintTiles() 
      {
          float range = GetRange();
          float tileSize = TileGridManager.Instance.cubeSize;
          Vector3 center = transform.position;

          int minX = Mathf.FloorToInt((center.x - range) / tileSize);
          int maxX = Mathf.FloorToInt((center.x + range) / tileSize);

          int minZ = Mathf.FloorToInt((center.z - range) / tileSize);
          int maxZ = Mathf.FloorToInt((center.z + range) / tileSize);

          for (int x = minX; x <= maxX; x++)
          {
              for (int z = minZ; z <= maxZ; z++)
              {
                  Tile tile = TileGridManager.Instance.GetTile(x, z);
                  if (tile == null) continue;

                  Vector3 tilePos = new Vector3(x * tileSize, 0, z * tileSize);
                  float dist = Vector3.Distance(new Vector3(center.x, 0, center.z), tilePos);
                  if (dist > range) continue;

                  if (tile.ColorState == TileColorState.Enemy)
                      tile.SetColor(TileColorState.Player);
              }
          }
      }
  */

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
