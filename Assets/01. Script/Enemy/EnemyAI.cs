using UnityEngine;

public enum EnemyState
{
    MOVE, // 이동
    ATTACK, // 공격
    FLEE, // 도주
    DEAD // 사망
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
            Debug.LogError("enemyHealth가 초기화되지 않았습니다!");
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
