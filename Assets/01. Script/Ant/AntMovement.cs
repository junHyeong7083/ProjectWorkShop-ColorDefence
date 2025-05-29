using UnityEngine;
using UnityEngine.AI;

public class AntMovement : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField] float moveSpeed = 7f;
    public System.Action OnArrive;  // ���� �� ȣ���� �ݹ�
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
                OnArrive = null; // �� ���� ȣ��ǵ���
            }
        }
    }
    public void MoveTo(Vector3 position)
    {
        agent.speed = moveSpeed;
        agent.SetDestination(position);
    }
}