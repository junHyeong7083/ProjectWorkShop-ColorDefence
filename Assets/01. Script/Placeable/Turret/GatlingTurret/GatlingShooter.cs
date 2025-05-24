using UnityEngine;

[RequireComponent(typeof(GatlingTurret))]
public class GatlingShooter : MonoBehaviour, ITurretShooter
{
    private GatlingTurret gatling;
    private TurretBase turret;

    private void Awake()
    {
        gatling = GetComponent<GatlingTurret>();
        turret = GetComponent<TurretBase>();
    }

    public void ShootAtEnemy(GameObject enemy)
    {
        Vector3 dir = (enemy.transform.position - gatling.GetFirePoint().position).normalized;
        BulletPool.Instance.GetGatlingEnemyBullet(gatling.GetFirePoint().position, enemy.transform, turret.GetDamage());
    }

    public void ShootAtTile(Tile tile)
    {
        Vector3 dir = (tile.CenterWorldPos - gatling.GetFirePoint().position).normalized;

        BulletPool.Instance.GetGatlingTileBullet(gatling.GetFirePoint().position, dir, () =>
        {
            if (tile.ColorState == TileColorState.Enemy)
            {
                tile.SetColor(TileColorState.Player);
                tile.AnimateBump();
            }

            tile.Release();
            tile.TargetingTurret = null;

            EffectManager.Instance.PlayEffect(
                turret.turretData.turretType,
                turret.turretData.actionType,
                tile.CenterWorldPos
            );
        });
    }
}
