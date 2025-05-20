using System.Collections.Generic;
using UnityEngine;
using System;
public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [SerializeField] private GatlingBulletEnemy enemyBulletPrefab;
    [SerializeField] private GatlingBulletTile tileBulletPrefab;

    private Queue<GatlingBulletEnemy> gatlingBulletEnemies = new();
    private Queue<GatlingBulletTile> gatlingBulletTiles = new();

    void Awake()
    {
        Instance = this;
        InitPool(enemyBulletPrefab, gatlingBulletEnemies, 20);
        InitPool(tileBulletPrefab, gatlingBulletTiles, 20);
    }

    void InitPool<T>(T prefab, Queue<T> pool, int count) where T : MonoBehaviour
    {
        for (int i = 0; i < count; i++)
        {
            T obj = Instantiate(prefab, transform);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public void Return(GatlingBulletEnemy bullet)
    {
        bullet.gameObject.SetActive(false);
        gatlingBulletEnemies.Enqueue(bullet);
    }

    public void Return(GatlingBulletTile bullet)
    {
        bullet.gameObject.SetActive(false);
        gatlingBulletTiles.Enqueue(bullet);
    }

    public void GetEnemyBullet(Vector3 pos, Vector3 dir)
    {
        var bullet = gatlingBulletEnemies.Count > 0 ? gatlingBulletEnemies.Dequeue() : Instantiate(enemyBulletPrefab, transform);
        bullet.transform.position = pos;
        bullet.transform.rotation = Quaternion.LookRotation(dir);
        bullet.Init(dir);
    }

    public void GetTileBullet(Vector3 pos, Vector3 dir, Action onArrive)
    {
        var bullet = gatlingBulletTiles.Count > 0 ? gatlingBulletTiles.Dequeue() : Instantiate(tileBulletPrefab, transform);
        bullet.transform.position = pos;
        bullet.transform.rotation = Quaternion.LookRotation(dir);
        bullet.Init(dir, onArrive);
    }
}
