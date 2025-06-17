using System.Collections.Generic;
using UnityEngine;
using System;
public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [SerializeField] private GatlingBulletEnemy gatlingEnemyBulletPrefab;
    [SerializeField] private GatlingBulletTile gatlingTileBulletPrefab;
    [SerializeField] private CannonBulletEnemy cannonEnemyBulletPrefab;
    [SerializeField] private BeeBullet beeBulletPrefab;


    [SerializeField] private int BulletCount;

    private Queue<GatlingBulletEnemy> gatlingBulletEnemies = new();
    private Queue<GatlingBulletTile> gatlingBulletTiles = new();
    private Queue<CannonBulletEnemy> cannonBulletEnemies = new();
    private Queue<BeeBullet> beeBulletEnemies = new();


    void Awake()
    {
        Instance = this;
        InitPool(gatlingEnemyBulletPrefab, gatlingBulletEnemies, BulletCount);
        InitPool(gatlingTileBulletPrefab, gatlingBulletTiles, BulletCount);
        InitPool(cannonEnemyBulletPrefab, cannonBulletEnemies, BulletCount); // Ãß°¡
        InitPool(beeBulletPrefab, beeBulletEnemies, BulletCount);
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

    #region Return Overloading
    public void Return(CannonBulletEnemy bullet)
    {
        bullet.gameObject.SetActive(false);
        cannonBulletEnemies.Enqueue(bullet);
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
    public void Return(BeeBullet bullet)
    {
        bullet.gameObject.SetActive(false);
        beeBulletEnemies.Enqueue(bullet);
    }
    #endregion

    public void GetCannonEnemyBullet(Vector3 start, Vector3 end, float height, float duration, int damage, Action onHit)
    {
        var bullet = cannonBulletEnemies.Count > 0 ? cannonBulletEnemies.Dequeue() : Instantiate(cannonEnemyBulletPrefab, transform);
        bullet.transform.position = start;
        bullet.Init(start, end, height, duration, damage, onHit);
    }



    public void GetGatlingEnemyBullet(Vector3 pos, Transform target, int damage)
    {
        var bullet = gatlingBulletEnemies.Count > 0
       ? gatlingBulletEnemies.Dequeue()
       : Instantiate(gatlingEnemyBulletPrefab, transform);

     //   bullet.gameObject.SetActive(true);

        bullet.transform.position = pos;

        Vector3 dir = (target.position - pos).normalized;
        bullet.transform.rotation = Quaternion.LookRotation(dir);
        bullet.gameObject.SetActive(true);
        bullet.Init(target, damage);
    }

    public void GetGatlingTileBullet(Vector3 pos, Vector3 dir, Action onArrive)
    {
        var bullet = gatlingBulletTiles.Count > 0 
            ? gatlingBulletTiles.Dequeue() 
            : Instantiate(gatlingTileBulletPrefab, transform);
        bullet.transform.position = pos;
        bullet.transform.rotation = Quaternion.LookRotation(dir);
        bullet.Init(dir, onArrive);
    }


    public void GetBeeBullet(Vector3 pos, Transform target, int damage)
    {
        var bullet = beeBulletEnemies.Count > 0
       ? beeBulletEnemies.Dequeue()
       : Instantiate(beeBulletPrefab, transform);

        bullet.transform.position = pos;

        Vector3 dir = (target.position - pos).normalized;
        bullet.transform.rotation = Quaternion.LookRotation(dir);
        bullet.gameObject.SetActive(true);
        bullet.Init(target, damage);
    }
}
