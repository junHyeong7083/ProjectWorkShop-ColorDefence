using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private string[] monsterNames;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Transform[] spawnPositions; // ���� ��ġ��

    private Vector2Int goalGrid;

    private void Start()
    {
        // ��ǥ�� ���հ��� ��ġ, ������ �߾�
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
            goalGrid = TileGridManager.GetCenterGrid();
        }

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
                Debug.LogError($"[SpawnManager] {go.name}�� BaseEnemy ������Ʈ ����.");
                continue;
            }

            // ���� ��Ʈ�ѷ� ���
            EnemyStrategyController.Instance.RegisterEnemy(enemy);

            // Hold �ʿ� �� ���⼭ enemy.Hold() ȣ�� ����
        }
    }

    private GameObject GetEnemyFromPool()
    {
        string monsterName = monsterNames[Random.Range(0, monsterNames.Length)];
        Transform spawnPos = spawnPositions[Random.Range(0, spawnPositions.Length)];

        return EnemyPoolManager.Instance.Get(monsterName, spawnPos, goalGrid);
    }

    // �ܺο��� ���� ȣ�� ���� (������ ���� ��)
    public void SpawnEnemies(int count, int groupId = -1)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject go = GetEnemyFromPool();
            if (go == null) continue;

            BaseEnemy enemy = go.GetComponent<BaseEnemy>();
            if (enemy == null) continue;

          //  enemy.SetGroupId(groupId);
            go.SetActive(true);
        }
    }
}
