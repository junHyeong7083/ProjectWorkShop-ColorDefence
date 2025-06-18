using UnityEngine;

[DisallowMultipleComponent]
public class EnemyTargetFinder : MonoBehaviour
{
    private BaseEnemy enemy;

    [Header("탐지 대상 레이어")]
    [SerializeField] private LayerMask targetLayer;

    public Transform CurrentTarget { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<BaseEnemy>();
    }
    public bool TryFindTarget(out Transform target)
    {
        target = FindClosestEnemyInAttackRange(enemy.Data.AttackRange);
        return target != null;
    } 
    public Transform FindClosestEnemyInAttackRange(float attackRange)
    {
        Vector3 center = enemy.transform.position;
        Collider[] hits = Physics.OverlapSphere(center, attackRange, targetLayer);

        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<IDamageable>(out _))
            {
                float dist = Vector3.SqrMagnitude(hit.transform.position - center);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = hit.transform;
                }
            }
        }

        return closest;
    }
}
