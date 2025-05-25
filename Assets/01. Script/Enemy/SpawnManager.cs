using UnityEngine;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private string[] monsterNames; // ex: "Slime", "Orc"
    [SerializeField] private float spawnInterval = 2f;

    private Vector3[] spawnPositions;
    private Vector2Int goalGrid;

    private void Start()
    {
        // 1. 스폰 위치 미리 계산
        spawnPositions = TileGridManager.GetSpawnPositions();
        foreach(var spawn in spawnPositions)
        {
            Debug.Log($"spawnPosX = {spawn.x} spawnPosY = {spawn.y}  spawnPosZ = {spawn.z }  ");
        }
        // 2. 목표 타일 그리드 좌표 저장
        goalGrid = TileGridManager.GetCenterGrid();
        Debug.Log(goalGrid);

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // 1. 랜덤 몬스터 종류
            string monsterName = monsterNames[Random.Range(0, monsterNames.Length)];

            // 2. 랜덤 스폰 위치
            Vector3 spawnPos = spawnPositions[Random.Range(0, spawnPositions.Length)];

            // 3. 생성
            GameObject enemy = EnemyPoolManager.Instance.Get(monsterName, spawnPos);
            enemy.transform.position = spawnPos;

            // 4. 길찾기 세팅
            var pathfinder = enemy.GetComponent<EnemyPathfinder>();
            if (pathfinder != null)
            {
                pathfinder.goalGridPosition = goalGrid;
                pathfinder.InitializePathfinder(spawnPos);
            }
        }
    }
}
