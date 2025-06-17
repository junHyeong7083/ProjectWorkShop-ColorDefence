using UnityEngine.AI;
using UnityEngine;

public class AntMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] float moveSpeed = 7f;

    public System.Action OnArrive;  // 도착 시 호출할 콜백

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                if (OnArrive != null)
                {
                    OnArrive.Invoke();
                    OnArrive = null;
                }
            }
        }
    }


    public void MoveTo(Vector3 position)
    {
        if (agent == null || !agent.isOnNavMesh) return;

        if (NavMesh.SamplePosition(position, out var hit, 5f, NavMesh.AllAreas))
        {
            agent.isStopped = false; // 이동 다시 활성화
            agent.speed = moveSpeed;
            agent.SetDestination(hit.position);
        }
    }

    public void Stop()
    {
        if (agent != null)
        {
            agent.ResetPath(); // 확실하게 경로 제거
            agent.velocity = Vector3.zero; // 물리적으로 멈추기
            agent.isStopped = true; // 이동 완전 정지
        }
    }
}
