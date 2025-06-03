using UnityEngine;

[RequireComponent(typeof(CannonTurret))]  // �ݵ�� CannonTurret�� �Բ� �־�� ��
public class CannonShooter : MonoBehaviour, ITurretShooter
{
    private CannonTurret cannon;  // �߻� ��ġ(firePoint)�� ��� ���� ����
    private TurretBase turret;    // �ͷ� ����(������, ������ ��) ���

    [SerializeField] float arcHeight;   // ������ ���� ���� (y������ �󸶳� Ƣ�������)
    [SerializeField] float flightTime;  // ��ź�� ���ư��� �ð� (����� ����)

    public bool IsReloading => false;

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
                "BigExplosion",
                end
            );
        });
    }


    
}
