using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private string[] monsterNames; // Slime, Orc 등
    [SerializeField] private Transform[] spawnPoints; // 스폰 위치들
    [SerializeField] private float spawnInterval = 2f;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 1. 랜덤 몬스터 종류 선택
            string monsterName = monsterNames[Random.Range(0, monsterNames.Length)];

            // 2. 랜덤 스폰 위치 선택
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // 3. 풀에서 꺼내서 배치
            GameObject enemy = EnemyPoolManager.Instance.Get(monsterName);
            enemy.transform.position = point.position;
        }
    }
}
