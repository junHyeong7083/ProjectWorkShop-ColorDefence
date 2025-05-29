using UnityEngine;
using UnityEngine.AI;

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
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (OnArrive != null)
            {
                OnArrive.Invoke();
                OnArrive = null; // 한 번만 호출되도록
            }
        }
    }
    public void MoveTo(Vector3 position)
    {
        agent.speed = moveSpeed;
        agent.SetDestination(position);
    }
}