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
        float detectRange = enemy.Data.DetectRange;
        Collider[] hits = Physics.OverlapSphere(transform.position, detectRange, targetLayer);

     //   Debug.Log($"[TargetFinder] 감지 시도 - 범위: {detectRange}, 탐지된 개수: {hits.Length}");

        target = null;
        float minDist = float.MaxValue;

        foreach (var hit in hits)
        {

            if(hit.TryGetComponent<IDamageable>(out _))
            {
                float dist = Vector3.SqrMagnitude(hit.transform.position - transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    target = hit.transform;
                }
            }
           
        }

        CurrentTarget = target;
        return target != null;
    }
}
