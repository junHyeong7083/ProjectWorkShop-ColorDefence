using UnityEngine;

public enum EnemyState
{
    MOVE, // �̵�
    ATTACK, // ����
    FLEE, // ����
    DEAD // ���
}


public class EnemyAI : MonoBehaviour
{
    public EnemyState currentState = EnemyState.MOVE;

    private Enemy enemy;
    private EnemyHealth enemyHealth;
    private Transform targetTransform;


    private void OnEnable()
    {

        if (enemyHealth == null)
            enemyHealth = GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.ResetHp();
            enemyHealth.OnDie += OnDie;
        }
        else
        {
            Debug.LogError("enemyHealth�� �ʱ�ȭ���� �ʾҽ��ϴ�!");
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
            enemyHealth.OnDie -= OnDie;
    }

    void EnemyFSM(EnemyState enemyState)
    {
        switch (enemyState)
        {

            case EnemyState.MOVE:
                Movement();
                break;

            case EnemyState.ATTACK:
                Attack();
                break;
        }

    }


    void LookForTarget()
    {

    }

    void Movement()
    {

    }

    void Attack()
    {

    }

    void OnDie(Enemy _enemy) => currentState = EnemyState.DEAD;



}
