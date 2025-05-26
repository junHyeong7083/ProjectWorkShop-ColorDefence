using UnityEngine;

[RequireComponent(typeof(CannonTurret))]  // �ݵ�� CannonTurret�� �Բ� �־�� ��
public class CannonShooter : MonoBehaviour, ITurretShooter
{
    private CannonTurret cannon;  // �߻� ��ġ(firePoint)�� ��� ���� ����
    private TurretBase turret;    // �ͷ� ����(������, ������ ��) ���

    [SerializeField] float arcHeight;   // ������ ���� ���� (y������ �󸶳� Ƣ�������)
    [SerializeField] float flightTime;  // ��ź�� ���ư��� �ð� (����� ����)

    private void Awake()
    {
        cannon = GetComponent<CannonTurret>();
        turret = GetComponent<TurretBase>();
    }

    public void ShootAtEnemy(GameObject enemy)
    {
        if (enemy == null) return;

        Vector3 start = cannon.GetFirePoint().position;
        Vector3 end = enemy.transform.position;
        int damage = turret.GetDamage();

        BulletPool.Instance.GetCannonEnemyBullet(start, end, arcHeight, flightTime, damage, () =>
        {
            CameraShakeManager.Instance.RequestShake(0.6f);
            EffectManager.Instance.PlayEffect(
                turret.turretData.turretType,
                turret.turretData.actionType,
                end
            );
        });
    }


    /// <summary>
    /// Ÿ�Ͽ� ��ź �߻��ϴ� ���� (�̻�� ����, Gatling�� ���� �ڵ尡 �ּ�ó���Ǿ� ����)
    /// ���� �ʿ� �� ĳ�� ���� Ÿ�� ���ݵ� ���� ����
    /// </summary>
    public void ShootAtTile(Tile tile)
    {
        /*
        Vector3 dir = (tile.CenterWorldPos - cannon.GetFirePoint().position).normalized;

        BulletPool.Instance.GetGatlingTileBullet(cannon.GetFirePoint().position, dir, () =>
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
        */
    }
}
