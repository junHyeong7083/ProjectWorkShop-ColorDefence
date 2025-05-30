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

            string monsterName = monsterNames[Random.Range(0, monsterNames.Length)];
            Vector3 spawnPos = spawnPositions[Random.Range(0, spawnPositions.Length)];
          //  Debug.Log($"spawnPos : {spawnPos}");
            GameObject enemy = EnemyPoolManager.Instance.Get(monsterName, spawnPos, goalGrid);
            enemy.transform.position = spawnPos + Vector3.up*2f;
        }
    }
}
