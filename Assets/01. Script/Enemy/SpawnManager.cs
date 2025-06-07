using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private string[] monsterNames;
    [SerializeField] private float spawnInterval = 2f;

    private Vector3[] spawnPositions;
    private Vector2Int goalGrid;
    private List<BaseEnemy> idleEnemies = new();
    private void Start()
    {
        spawnPositions = TileGridManager.GetSpawnPositions();

        GameObject queen = GameObject.FindGameObjectWithTag("QueenAnt");
        if (queen != null)
        {
            Vector3 worldPos = queen.transform.position;
            goalGrid = new Vector2Int(
                Mathf.FloorToInt(worldPos.x / TileGridManager.Instance.cubeSize),
                Mathf.FloorToInt(worldPos.z / TileGridManager.Instance.cubeSize)
            );
        }
        else
        {
            Debug.LogError("QueenAnt not found! Defaulting to center.");
            goalGrid = TileGridManager.GetCenterGrid(); // fallback
        }

        goalGrid = TileGridManager.GetCenterGrid();

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            GameObject go = GetEnemyFromPool();
            if (go == null) continue;

            BaseEnemy enemy = go.GetComponent<BaseEnemy>();
            if (enemy == null)
            {
                Debug.LogError($"[SpawnManager] {go.name} �� BaseEnemy ������Ʈ�� �����ϴ�.");
                continue;
            }
            // ���� ��Ʈ�ѷ����� ����
            EnemyStrategyController.Instance.RegisterEnemy(enemy);

            // Hold ���·� ��� (���ʿ� ��Ȱ��ȭ��)
            enemy.Hold();
        }
    }

    private GameObject GetEnemyFromPool()
    {
        string monsterName = monsterNames[Random.Range(0, monsterNames.Length)];
        Vector3 spawnPos = spawnPositions[Random.Range(0, spawnPositions.Length)];
        return EnemyPoolManager.Instance.Get(monsterName, spawnPos, goalGrid); // �� goalGrid�� ��򰡿� ����ž� ��
    }
    public void SpawnEnemies(int count, int groupId = -1)
    {
        for (int i = 0; i < count; i++)
        {
            var enemy = GetEnemyFromPool();
            if (enemy == null) continue;

            enemy.GetComponent<BaseEnemy>().SetGroupId(groupId);
            enemy.SetActive(true);
        }
    }


}
