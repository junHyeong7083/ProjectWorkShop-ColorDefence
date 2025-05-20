using UnityEngine;
using System;

public class GatlingBulletTile : MonoBehaviour
{
    public float speed = 20f;
    Vector3 direction;
    float lifeTime = 5f;

    private Action onArrive;

    public void Init(Vector3 dir, Action onArriveCallback)
    {
        direction = dir.normalized;
        gameObject.SetActive(true);
        onArrive = onArriveCallback;
        CancelInvoke();
        Invoke(nameof(ReturnToPool), lifeTime);
    }


    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Tile"))
        {
            onArrive?.Invoke();
            onArrive = null;
            ReturnToPool();
        }
    }

    void ReturnToPool()
    {
        BulletPool.Instance.Return(this);
    }
}
