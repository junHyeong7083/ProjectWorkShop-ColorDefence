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
        if (enemy == null) return;

        Vector3 firePos = gatling.GetFirePoint().position;
        Transform target = enemy.transform;
        int damage = turret.GetDamage();

        BulletPool.Instance.GetGatlingEnemyBullet(firePos, target, damage);
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
