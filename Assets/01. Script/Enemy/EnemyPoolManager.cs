using UnityEngine;
using System.Collections.Generic;

public class EnemyPoolManager : MonoBehaviour
{
    public static EnemyPoolManager Instance { get; private set; }

    [SerializeField] private GameObject[] enemyPrefabs;
    [SerializeField] private int poolSizePerType = 10;

    private Dictionary<string, Queue<GameObject>> poolDict = new();
    private Dictionary<string, Transform> containerDict = new();
    private Transform rootContainer;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        rootContainer = new GameObject("EnemyPoolRoot").transform;

        foreach (var prefab in enemyPrefabs)
        {
            string key = prefab.name;
            poolDict[key] = new Queue<GameObject>();

            GameObject containerGO = new GameObject($"{key}_Container");
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

    public GameObject Get(string prefabName, Transform spawnPos, Vector2Int goalGridPos)
    {
        if (!poolDict.ContainsKey(prefabName))
        {
            Debug.LogError($"[EnemyPoolManager] {prefabName} 프리팹이 풀에 없습니다.");
            return null;
        }

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

        obj.transform.position = spawnPos.position;
        obj.SetActive(true);

        var col = obj.GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
            col.enabled = true;
        }

        var enemy = obj.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            enemy.ResetHP();
            enemy.SetGoal(goalGridPos);
        }
       // Debug.Log($"[Get] 최종 위치: {obj.transform.position}");
        return obj;

    }

    public void Return(string prefabName, GameObject obj)
    {
        obj.SetActive(false);
        if (poolDict.ContainsKey(prefabName))
        {
            poolDict[prefabName].Enqueue(obj);
        }
        else
        {
         //   Debug.LogWarning($"[EnemyPoolManager] {prefabName} 프리팹 이름이 풀에 없습니다. 즉시 삭제함.");
            Destroy(obj);
        }
    }

    private GameObject GetPrefabByName(string name)
    {
        foreach (var prefab in enemyPrefabs)
        {
            if (prefab.name == name)
                return prefab;
        }

        Debug.LogError($"[EnemyPoolManager] {name} 프리팹을 찾을 수 없습니다.");
        return null;
    }
}
