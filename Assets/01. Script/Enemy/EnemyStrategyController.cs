using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EnemyStrategyType.cs
public enum EnemyStrategyType
{
    SCOUT, // 정찰
    RUSH,  // 러쉬
    HOLD   // 대기
}

public class EnemyStrategyController : MonoBehaviour
{
    public static EnemyStrategyController Instance { get; private set; }

    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private float strategyInterval = 10f;
    [SerializeField] private float scoutTimeout = 5f;  // 정찰 시간 초과 기준

    private float lastStrategyChangeTime;
    private float scoutStartTime;
    private bool scoutInProgress = false;
    private int scoutFailCount = 0;

    private EnemyStrategyType currentStrategy = EnemyStrategyType.HOLD;

    private List<BaseEnemy> pooledEnemies = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // 전략 재평가 타이밍
        if (Time.time - lastStrategyChangeTime > strategyInterval)
        {
            DecideNextStrategy();
        }

        // SCOUT 상태에서 timeout 체크
        if (scoutInProgress && Time.time - scoutStartTime > scoutTimeout)
        {
            // 아직도 아무것도 못봤으면 실패
            if (ScoutIntel.lastSeenUnitCount == 0 && !ScoutIntel.turretSpotted)
            {
             //   Debug.Log("[EnemyStrategy] 정찰 실패: 정보 없음");
                scoutFailCount++;
                scoutInProgress = false;

                if (scoutFailCount >= 2)
                {
                //    Debug.Log("[EnemyStrategy] 정찰 연속 실패 → 강제 RUSH");
                    ApplyStrategy(EnemyStrategyType.RUSH);
                    scoutFailCount = 0;
                }
                else
                {
                    ApplyStrategy(EnemyStrategyType.HOLD);
                }
            }
            else
            {
                // 정찰 성공
                scoutFailCount = 0;
                scoutInProgress = false;
            }
        }
    }

    private void DecideNextStrategy()
    {
        lastStrategyChangeTime = Time.time;
        currentStrategy = EvaluateStrategy();
       // Debug.Log($"[EnemyStrategy] 현재 전략 상태: {currentStrategy}");
        ApplyStrategy(currentStrategy);
    }

    private EnemyStrategyType EvaluateStrategy()
    {
        int troopCount = pooledEnemies.Count;
        bool hasSeenUnits = ScoutIntel.lastSeenUnitCount > 0;
        bool hasSeenTurret = ScoutIntel.turretSpotted;

      //  Debug.Log($"[Intel] 병력 수: {troopCount}, 유닛 탐지: {ScoutIntel.lastSeenUnitCount}, 터렛 발견: {ScoutIntel.turretSpotted}");

        if (!hasSeenUnits && !hasSeenTurret)
            return EnemyStrategyType.SCOUT;

        if (troopCount >= 5 && (hasSeenUnits || hasSeenTurret))
            return EnemyStrategyType.RUSH;

        return EnemyStrategyType.HOLD;
    }

    private void ApplyStrategy(EnemyStrategyType strategy)
    {
        ScoutIntel.Clear();

        switch (strategy)
        {
            case EnemyStrategyType.SCOUT:
                scoutInProgress = true;
                scoutStartTime = Time.time;
                StartCoroutine(DeployEnemiesAsync(2));
                break;

            case EnemyStrategyType.RUSH:
                scoutInProgress = false;
                StartCoroutine(DeployEnemiesAsync(pooledEnemies.Count));
                break;

            case EnemyStrategyType.HOLD:
                scoutInProgress = false;
             //   Debug.Log("[EnemyStrategy] HOLD - 병력 수집 중");
                break;
        }

       // Debug.Log($"[EnemyStrategy] 전략 적용: {strategy}");
    }

    public void RegisterEnemy(BaseEnemy enemy)
    {
        pooledEnemies.Add(enemy);
        enemy.gameObject.SetActive(false);
    }

    private IEnumerator DeployEnemiesAsync(int count)
    {
        count = Mathf.Min(count, pooledEnemies.Count);
        for (int i = 0; i < count; i++)
        {
            var enemy = pooledEnemies[0];
            pooledEnemies.RemoveAt(0);

            // 위치, 목표 분산
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f),
                0,
                UnityEngine.Random.Range(-0.5f, 0.5f)
            );
            enemy.transform.position += offset;

           /* var pathfinder = enemy.GetComponent<EnemyPathfinder>();
            if (pathfinder != null)
            {
                Vector2Int originalGoal = pathfinder.goalGridPosition;
                pathfinder.goalGridPosition = originalGoal + new Vector2Int(
                    Random.Range(-1, 2), Random.Range(-1, 2)
                );
            }*/

            enemy.gameObject.SetActive(true);
            enemy.Resume();

            yield return YieldCache.WaitForSeconds(0.1f);
        }
    }
}
