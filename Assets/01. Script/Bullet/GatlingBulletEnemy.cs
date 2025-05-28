using UnityEngine;
using System;

public class GatlingBulletEnemy : MonoBehaviour
{
    private Transform target;
    private float t = 0f;
    private float travelDuration;
    private float distance;
    private Vector3 startPos;

    private int damage;
    private Action onArrive;

    private float hitThreshold = 0.3f;

    public void Init(Transform targetTransform, int _damage, Action onArriveCallback = null)
    {
        target = targetTransform;
        damage = _damage;
        onArrive = onArriveCallback;

        startPos = transform.position;
        t = 0f;

        // �Ÿ� ������� ���� �ð� ����
        distance = Vector3.Distance(startPos, target.position);
        travelDuration = Mathf.Max(distance / 100f, 0.1f);  // �� �� ��ġ�� �Ѿ� �ӵ� ��ü ����
    }

    void Update()
    {
        if (target == null)
        {
            ReturnToPool();
            return;
        }

        t += Time.deltaTime / travelDuration;
        t = Mathf.Clamp01(t);

        transform.position = Vector3.Lerp(startPos, target.position, t);

        // ���� ����
        if (Vector3.Distance(transform.position, target.position) <= hitThreshold || t >= 1f)
        {
            var health = target.GetComponent<EnemyHealth>();
            if (health != null && health.currentHp > 0)
                health.TakeDamage(damage);

            onArrive?.Invoke();
            EffectManager.Instance.PlayEffect(
                TurretType.Gatling,
                TurretActionType.AttackEnemy,
                transform.position
            );

            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        gameObject.SetActive(false);
        BulletPool.Instance.Return(this);
        onArrive = null;
    }
}

