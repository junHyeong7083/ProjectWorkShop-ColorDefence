using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;
using static UnityEngine.GraphicsBuffer;
public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [SerializeField] private GatlingBulletEnemy gatlingEnemyBulletPrefab;
    [SerializeField] private GatlingBulletTile gatlingTileBulletPrefab;
    [SerializeField] private CannonBulletEnemy cannonEnemyBulletPrefab;



    private Queue<GatlingBulletEnemy> gatlingBulletEnemies = new();
    private Queue<GatlingBulletTile> gatlingBulletTiles = new();
    private Queue<CannonBulletEnemy> cannonBulletEnemies = new();

    void Awake()
    {
        Instance = this;
        InitPool(gatlingEnemyBulletPrefab, gatlingBulletEnemies, 20);
        InitPool(gatlingTileBulletPrefab, gatlingBulletTiles, 20);
        InitPool(cannonEnemyBulletPrefab, cannonBulletEnemies, 20); // �߰�
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

        bullet.gameObject.SetActive(true);

        bullet.transform.position = pos;
        bullet.Init(target, damage);
    }

    public void GetGatlingTileBullet(Vector3 pos, Vector3 dir, Action onArrive)
    {
        var bullet = gatlingBulletTiles.Count > 0 ? gatlingBulletTiles.Dequeue() : Instantiate(gatlingTileBulletPrefab, transform);
        bullet.transform.position = pos;
        bullet.transform.rotation = Quaternion.LookRotation(dir);
        bullet.Init(dir, onArrive);
    }
}
