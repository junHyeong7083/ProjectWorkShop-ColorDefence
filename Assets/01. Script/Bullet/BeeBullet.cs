using System;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class BeeBullet : MonoBehaviour
{
    private Transform target;
    private float t = 0;
    private float travelDuration;
    private float distance;

    private Vector3 startPos;

    private int damage;
    private Action onArrive;

    private float hitThreshold = 0.3f;


    public void Init(Transform targetTransform, int damage, Action onArriveCallback = null)
    {
        target = targetTransform;
        this.damage = damage;   
        onArrive = onArriveCallback;

        startPos = transform.position;
        t = 0f;

        distance = Vector3.Distance(startPos, target.position);
        travelDuration = Mathf.Max(distance / 100, 0.1f);
    }


    private void Update()
    {
        if(target == null)
        {
            ReturnToPool();
            return;
        }

        t += Time.deltaTime / travelDuration;
        t = Mathf.Clamp01(t);

        transform.position = Vector3.Lerp(startPos, target.position, t);


        // µµÂø ÆÇÁ¤
        if (Vector3.Distance(transform.position, target.position) <= hitThreshold || t >= 1f)
        {
            var health = target.GetComponent<BaseEnemy>();
            if (health != null && health.GetCurrentHp() > 0)
                health.TakeDamage(damage);

            onArrive?.Invoke();
            EffectManager.Instance.PlayEffect(
               "Gatling_Enemy_HitFX",
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
