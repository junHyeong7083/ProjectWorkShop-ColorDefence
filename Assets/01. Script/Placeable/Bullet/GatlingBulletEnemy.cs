using UnityEngine;
using System;
public class GatlingBulletEnemy : MonoBehaviour
{
    public float speed = 20f;
    Vector3 direction;
    float lifeTime = 5f;

    int damage;

    bool hasHit = false;
    // ������ ����� �ݹ� �Լ�
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
            // ������ ó�� (���ϸ� Ȯ��)

            other.GetComponent<EnemyHealth>()?.TakeDamage(damage);


            EffectManager.Instance.PlayEffect(TurretType.Gatling, TurretActionType.AttackEnemy, this.transform.position);

            ReturnToPool();
        }
    }
}
