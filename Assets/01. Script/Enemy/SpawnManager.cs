using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class MonsterSpawnData
{
    public string monsterName;
    public float spawnInterval;
}

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private MonsterSpawnData[] spawnSettings;
    [SerializeField] private Transform[] spawnPositions;

    private Vector2Int goalGrid;

    private void Start()
    {
        GameObject queen = GameObject.FindGameObjectWithTag("QueenAnt");
        if (queen != null)
        {
            Vector3 worldPos = queen.transform.position;
            goalGrid = new Vector2Int(
                Mathf.FloorToInt(worldPos.x / TileGridManager.Instance.cubeSize),
                Mathf.FloorToInt(worldPos.z / TileGridManager.Instance.cubeSize)
            );
        }

        foreach (var setting in spawnSettings)
        {
            StartCoroutine(SpawnLoop(setting));
        }
    }

    private IEnumerator SpawnLoop(MonsterSpawnData setting)
    {
        while (true)
        {
            yield return new WaitForSeconds(setting.spawnInterval);

            Transform spawnPos = spawnPositions[Random.Range(0, spawnPositions.Length)];
            GameObject go = EnemyPoolManager.Instance.Get(setting.monsterName, spawnPos, goalGrid);
            if (go == null) continue;

            var enemy = go.GetComponent<BaseEnemy>();
            if (enemy == null)
            {
                Debug.LogError($"[SpawnManager] {go.name}에 BaseEnemy 없음.");
                continue;
            }

            EnemyStrategyController.Instance.RegisterEnemy(enemy);
        }
    }

    // 수동 스폰용
    public void SpawnEnemies(int count, int groupId = -1)
    {
        for (int i = 0; i < count; i++)
        {
            var setting = spawnSettings[Random.Range(0, spawnSettings.Length)];
            Transform spawnPos = spawnPositions[Random.Range(0, spawnPositions.Length)];
            GameObject go = EnemyPoolManager.Instance.Get(setting.monsterName, spawnPos, goalGrid);
            if (go == null) continue;

            var enemy = go.GetComponent<BaseEnemy>();
            if (enemy == null) continue;

            go.SetActive(true);
        }
    }
}

