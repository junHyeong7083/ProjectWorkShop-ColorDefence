using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int initSize = 20;

    private Queue<Bullet> pool = new();

    void Awake()
    {
        Instance = this;
        StartCoroutine(InitializePool());
    }

    IEnumerator InitializePool()
    {
        for (int i = 0; i < initSize; i++)
        {
            Bullet bullet = Instantiate(bulletPrefab, transform);
            bullet.gameObject.SetActive(false);
            pool.Enqueue(bullet);

            // 10개마다 한 프레임씩 분산 처리
            if (i % 10 == 0)
                yield return null;
        }
    }

    public Bullet Get(Vector3 position, Vector3 direction, Action onArrive = null)
    {
        Bullet bullet = pool.Count > 0 ? pool.Dequeue() : Instantiate(bulletPrefab, transform);
        bullet.transform.position = position;
        bullet.transform.rotation = Quaternion.LookRotation(direction);
        bullet.Init(direction, onArrive);
        return bullet;
    }

    public void Return(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
        pool.Enqueue(bullet);
    }
}
