using System;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;
    Vector3 direction;
    float lifeTime = 5f;

    // 도착시 실행될 콜백 함수
    private Action onArrive;
    public void Init(Vector3 dir, Action onArriveCallback = null)
    {
        direction = dir.normalized;
        gameObject.SetActive(true);
        CancelInvoke();
        onArrive = onArriveCallback;

        Invoke(nameof(ReturnToPool), lifeTime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        if (onArrive != null)
        {
            float groundY = 0f;
            if (transform.position.y <= groundY + 0.1f)
            {
                onArrive?.Invoke();
                onArrive = null;
                ReturnToPool();
            }
        }
    }
    void ReturnToPool()
    {
        BulletPool.Instance.Return(this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            // 데미지 처리 (원하면 확장)
            ReturnToPool();
        }
    }
}
