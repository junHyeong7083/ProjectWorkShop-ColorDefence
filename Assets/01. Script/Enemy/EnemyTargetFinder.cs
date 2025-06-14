using UnityEngine;

[DisallowMultipleComponent]
public class EnemyTargetFinder : MonoBehaviour
{
    private BaseEnemy enemy;

    [Header("Ž�� ��� ���̾�")]
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

     //   Debug.Log($"[TargetFinder] ���� �õ� - ����: {detectRange}, Ž���� ����: {hits.Length}");

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
