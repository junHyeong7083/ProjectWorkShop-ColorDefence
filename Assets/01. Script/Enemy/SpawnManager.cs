using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private string[] monsterNames;
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Transform[] spawnPositions; // 스폰 위치들

    private Vector2Int goalGrid;

    private void Start()
    {
        // 목표는 여왕개미 위치, 없으면 중앙
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
                Debug.LogError($"[SpawnManager] {go.name}에 BaseEnemy 컴포넌트 없음.");
                continue;
            }

            // 전략 컨트롤러 등록
            EnemyStrategyController.Instance.RegisterEnemy(enemy);

            // Hold 필요 시 여기서 enemy.Hold() 호출 가능
        }
    }

    private GameObject GetEnemyFromPool()
    {
        string monsterName = monsterNames[Random.Range(0, monsterNames.Length)];
        Transform spawnPos = spawnPositions[Random.Range(0, spawnPositions.Length)];

        return EnemyPoolManager.Instance.Get(monsterName, spawnPos, goalGrid);
    }

    // 외부에서 수동 호출 가능 (전략적 생성 등)
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
