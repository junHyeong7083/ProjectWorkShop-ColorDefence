using UnityEngine;

public class GatlingShooter : MonoBehaviour, ITurretShooter
{
    private GatlingTurret gatling;
    private TurretBase turret;

    [SerializeField] private float fireDuration = 3f;   // 공격 유지 시간
    [SerializeField] private float reloadDuration = 2f; // 장전 시간

    private float elapsed;
    private bool isReloading = false;
    public bool IsReloading => isReloading;
    private void Awake()
    {
        gatling = GetComponent<GatlingTurret>();
        turret = GetComponent<TurretBase>();
    }

    private void Update()
    {
        elapsed += Time.deltaTime;

        if (isReloading)
        {
            if (elapsed >= reloadDuration)
            {
                isReloading = false;
                elapsed = 0f;
            }
        }
    }

    public void ShootAtEnemy(GameObject enemy)
    {
        if (isReloading || enemy == null) return;

        Vector3 firePos = gatling.GetFirePoint().position;
        Transform target = enemy.transform;
        int damage = turret.GetDamage();

        BulletPool.Instance.GetGatlingEnemyBullet(firePos, target, damage);

        if (elapsed >= fireDuration)
        {
            isReloading = true;
            elapsed = 0f;
        }
    }

    public void ShootAtTile(Tile tile)
    {
        if (isReloading || tile == null) return;

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

        if (elapsed >= fireDuration)
        {
            isReloading = true;
            elapsed = 0f;
        }
    }
}
