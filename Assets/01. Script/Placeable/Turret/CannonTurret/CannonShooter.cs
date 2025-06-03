using UnityEngine;

[RequireComponent(typeof(CannonTurret))]  // 반드시 CannonTurret가 함께 있어야 함
public class CannonShooter : MonoBehaviour, ITurretShooter
{
    private CannonTurret cannon;  // 발사 위치(firePoint)를 얻기 위한 참조
    private TurretBase turret;    // 터렛 정보(데미지, 데이터 등) 사용

    [SerializeField] float arcHeight;   // 베지어 궤적 높이 (y축으로 얼마나 튀어오를지)
    [SerializeField] float flightTime;  // 포탄이 날아가는 시간 (길수록 느림)

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
