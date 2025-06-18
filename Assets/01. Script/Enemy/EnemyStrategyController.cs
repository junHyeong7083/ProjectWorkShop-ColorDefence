using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// EnemyStrategyType.cs
public enum EnemyStrategyType
{
    SCOUT, // ����
    RUSH,  // ����
    HOLD   // ���
}

public class EnemyStrategyController : MonoBehaviour
{
    public static EnemyStrategyController Instance { get; private set; }

    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private float strategyInterval = 10f;
    [SerializeField] private float scoutTimeout = 5f;  // ���� �ð� �ʰ� ����

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
        // ���� ���� Ÿ�̹�
        if (Time.time - lastStrategyChangeTime > strategyInterval)
        {
            DecideNextStrategy();
        }

        // SCOUT ���¿��� timeout üũ
        if (scoutInProgress && Time.time - scoutStartTime > scoutTimeout)
        {
            // ������ �ƹ��͵� �������� ����
            if (ScoutIntel.lastSeenUnitCount == 0 && !ScoutIntel.turretSpotted)
            {
             //   Debug.Log("[EnemyStrategy] ���� ����: ���� ����");
                scoutFailCount++;
                scoutInProgress = false;

                if (scoutFailCount >= 2)
                {
                //    Debug.Log("[EnemyStrategy] ���� ���� ���� �� ���� RUSH");
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
                // ���� ����
                scoutFailCount = 0;
                scoutInProgress = false;
            }
        }
    }

    private void DecideNextStrategy()
    {
        lastStrategyChangeTime = Time.time;
        currentStrategy = EvaluateStrategy();
       // Debug.Log($"[EnemyStrategy] ���� ���� ����: {currentStrategy}");
        ApplyStrategy(currentStrategy);
    }

    private EnemyStrategyType EvaluateStrategy()
    {
        int troopCount = pooledEnemies.Count;
        bool hasSeenUnits = ScoutIntel.lastSeenUnitCount > 0;
        bool hasSeenTurret = ScoutIntel.turretSpotted;

      //  Debug.Log($"[Intel] ���� ��: {troopCount}, ���� Ž��: {ScoutIntel.lastSeenUnitCount}, �ͷ� �߰�: {ScoutIntel.turretSpotted}");

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
             //   Debug.Log("[EnemyStrategy] HOLD - ���� ���� ��");
                break;
        }

       // Debug.Log($"[EnemyStrategy] ���� ����: {strategy}");
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

            // ��ġ, ��ǥ �л�
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
