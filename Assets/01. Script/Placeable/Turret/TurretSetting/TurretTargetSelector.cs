using UnityEngine;

[RequireComponent(typeof(TurretBase))]
public class TurretTargetSelector : MonoBehaviour
{
    private TurretBase turret;
    public bool IsEnemyInRange { get; private set; }
    private void Awake()
    {
        turret = GetComponent<TurretBase>();
    }

    // 가장 가까운 적 찾기

    public GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject bestTarget = null;
        float bestScore = float.MaxValue;

        float range = turret.GetRange();
        float rangeSqr = range * range;
        Vector3 origin = transform.position;

        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;

            var health = enemy.GetComponent<EnemyHealth>();
            if (health == null || health.currentHp <= 0 || !enemy.activeInHierarchy)
                continue;

            float distSqr = (enemy.transform.position - origin).sqrMagnitude;
            if (distSqr > rangeSqr) continue;

            float minSq = turret.GetMinAttackRange() * turret.GetMinAttackRange();
           // Debug.Log($"DistSq : {distSqr} || MinSq : {minSq}");

            if (distSqr < bestScore && distSqr > minSq)
            {
                bestScore = distSqr;
                bestTarget = enemy;
            }
        }

        IsEnemyInRange = bestTarget != null;
        return bestTarget;
    }


  
}
