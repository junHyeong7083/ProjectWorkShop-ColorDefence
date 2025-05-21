using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private string[] monsterNames; // Slime, Orc ��
    [SerializeField] private Transform[] spawnPoints; // ���� ��ġ��
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

            // 1. ���� ���� ���� ����
            string monsterName = monsterNames[Random.Range(0, monsterNames.Length)];

            // 2. ���� ���� ��ġ ����
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // 3. Ǯ���� ������ ��ġ
            GameObject enemy = EnemyPoolManager.Instance.Get(monsterName);
            enemy.transform.position = point.position;
        }
    }
}
