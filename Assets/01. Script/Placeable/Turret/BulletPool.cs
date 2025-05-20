using UnityEngine;
using System.Collections.Generic;
using System;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [SerializeField] private Bullet bulletPrefab;
    [SerializeField] private int initSize = 20;

    private Queue<Bullet> pool = new Queue<Bullet>();

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < initSize; i++)
        {
            Bullet bullet = Instantiate(bulletPrefab, transform);
            bullet.gameObject.SetActive(false);
            pool.Enqueue(bullet);
        }
    }

    public Bullet Get(Vector3 position, Vector3 direction, System.Action onArrive = null)
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
