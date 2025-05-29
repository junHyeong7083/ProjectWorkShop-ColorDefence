using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private string[] monsterNames;
    [SerializeField] private float spawnInterval = 2f;

    private Vector3[] spawnPositions;
    private Vector2Int goalGrid;

    private void Start()
    {
        spawnPositions = TileGridManager.GetSpawnPositions();
        goalGrid = TileGridManager.GetCenterGrid();

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            string monsterName = monsterNames[Random.Range(0, monsterNames.Length)];
            Vector3 spawnPos = spawnPositions[Random.Range(0, spawnPositions.Length)];

            GameObject enemy = EnemyPoolManager.Instance.Get(monsterName, spawnPos, goalGrid);
            enemy.transform.position = spawnPos;
        }
    }
}
