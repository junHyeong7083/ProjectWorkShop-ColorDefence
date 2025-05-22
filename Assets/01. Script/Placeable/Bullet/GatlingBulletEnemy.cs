using UnityEngine;
using System;
public class GatlingBulletEnemy : MonoBehaviour
{
    public float speed = 20f;
    Vector3 direction;
    float lifeTime = 5f;

    int damage;

    bool hasHit = false;
    // 도착시 실행될 콜백 함수
    private Action onArrive;

    public void Init(Vector3 dir,int _damage ,Action onArriveCallback = null)
    {
        direction = dir.normalized;
        damage = _damage;

        gameObject.SetActive(true);
        CancelInvoke();
        onArrive = onArriveCallback;

        Invoke(nameof(ReturnToPool), lifeTime);
    }

    private void OnEnable() => hasHit = false;
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
    void ReturnToPool()
    {
        BulletPool.Instance.Return(this);
    }
    void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        if (other.CompareTag("Enemy"))
        {
            hasHit = true;

            onArrive?.Invoke();
            onArrive = null;
            // 데미지 처리 (원하면 확장)

            other.GetComponent<EnemyHealth>()?.TakeDamage(damage);


            EffectManager.Instance.PlayEffect(TurretType.Gatling, TurretActionType.AttackEnemy, this.transform.position);

            ReturnToPool();
        }
    }
}
