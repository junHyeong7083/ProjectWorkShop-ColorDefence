using UnityEngine;

[DisallowMultipleComponent]
public class EnemyTargetFinder : MonoBehaviour
{
    private Enemy enemy;

    [Header("Ž�� ��� ���̾�")]
    [SerializeField] private LayerMask targetLayer;

    public Transform CurrentTarget { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
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
      //      Debug.Log($"[TargetFinder] ���� ���: {hit.name}, �±�: {hit.tag}, ���̾�: {LayerMask.LayerToName(hit.gameObject.layer)}");

            float dist = Vector3.SqrMagnitude(hit.transform.position - transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                target = hit.transform;
            }
        }

        CurrentTarget = target;
        return target != null;
    }


    private void OnDrawGizmosSelected()
    {
        if (enemy == null) enemy = GetComponent<Enemy>();
        if (enemy == null || enemy.Data == null) return;

        Gizmos.color = new Color(1f, 0f, 0f, 1f); // ������ ����
        Gizmos.DrawWireSphere(transform.position, enemy.Data.DetectRange);
    }
}
