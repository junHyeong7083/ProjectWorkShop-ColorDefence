using UnityEngine;
using System.Collections.Generic;






public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance { get; private set; }

    [SerializeField] GameObject[] enemyPrefabs;
    [SerializeField] int poolSizePerType = 10;

    private Dictionary<string, Queue<GameObject>> poolDict = new();
    private Dictionary<string, Transform> containerDict = new();

    private Transform rootContainer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        rootContainer = new GameObject("Enemy").transform;

        foreach (var prefab in enemyPrefabs)
        {
            string key = prefab.name;
            poolDict[key] = new Queue<GameObject>();

            GameObject containerGO = new GameObject(key);
            containerGO.transform.SetParent(rootContainer);
            containerDict[key] = containerGO.transform;

            for (int i = 0; i < poolSizePerType; i++)
            {
                GameObject obj = Instantiate(prefab, containerGO.transform);
                obj.SetActive(false);
                poolDict[key].Enqueue(obj);
            }
        }
    }

    public GameObject Get(string prefabName, Vector3 spawnPos, Vector2Int goalGridPos, Dictionary<TileData, int> distanceMap = null)
    {
        if (!poolDict.ContainsKey(prefabName))
            return null;

        var pool = poolDict[prefabName];
        var container = containerDict[prefabName];

        GameObject obj;

        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
        }
        else
        {
            GameObject prefab = GetPrefabByName(prefabName);
            obj = Instantiate(prefab, container);
        }

        obj.SetActive(true);
        var col = obj.GetComponent<Collider>();
        col.enabled = false;
        col.enabled = true;

        var health = obj.GetComponent<EnemyHealth>();
        var pathfinder = obj.GetComponent<EnemyPathfinder>();

        health?.ResetHp();
        if (pathfinder != null)
        {
            pathfinder.goalGridPosition = goalGridPos;
            pathfinder.InitializePathfinder(spawnPos, distanceMap);
        }

        return obj;
    }

    public void Return(string prefabName, GameObject obj)
    {
        obj.SetActive(false);
        poolDict[prefabName].Enqueue(obj);
    }

    private GameObject GetPrefabByName(string name)
    {
        foreach (var prefab in enemyPrefabs)
        {
            if (prefab.name == name)
                return prefab;
        }
        return null;
    }
}
