using System.Collections;
using UnityEngine;

public class BeeController : MonoBehaviour,IDamageable
{
    public BeeData beeData;
    private int currentHp;
    [SerializeField] private Animator animator;
    private float lastAttackTime;
    private GameObject targetEnemy;
    private GameObject explicitTarget;

    private void Awake()
    {
        currentHp = beeData.maxHp;
        lastAttackTime = -beeData.attackCooltime;
    }

    private void Update()
    {
        if (currentHp <= 0) return;

        GameObject target = explicitTarget != null ? explicitTarget : FindClosestEnemy();

        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);

            if (explicitTarget != null && dist > beeData.attackRange)
            {
                var agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                if (agent != null)
                    agent.SetDestination(target.transform.position);
            }

            if (dist <= beeData.attackRange)
            {
                if (Time.time >= lastAttackTime + beeData.attackCooltime)
                {
                    Attack();
                    lastAttackTime = Time.time;

                    var enemyHealth = target.GetComponent<BaseEnemy>();
                    if (enemyHealth != null)
                        enemyHealth.TakeDamage(beeData.attackPower);
                }
                else animator.SetBool("IsAttack", false);
            }
            else animator.SetBool("IsAttack", false);
        }
        else
        {
            animator.SetBool("IsAttack", false);
            explicitTarget = null; 
        }
    }


    public void SetAttackTarget(GameObject target)
    {
        explicitTarget = target;
    }

    public GameObject FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    public GameObject FindClosestEnemyInRange()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject closest = null;
        float minDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist && dist <= beeData.attackRange)
            {
                minDist = dist;
                closest = enemy;
            }
        }
        return closest;
    }

    public void TakeDamage(int damage)
    {
        if (currentHp <= 0) return;

        currentHp -= damage;
        animator.SetTrigger("IsHit");

        if (currentHp <= 0)
        {
            animator.SetBool("IsDie",true);
            animator.SetTrigger("IsAttack");
        }
    }

    IEnumerator Die()
    {
        float time = Time.time;
        while(Time.time- time < 0.5f)
        {

            yield return null;
        }

    }


    private void Attack()
    {
        animator.SetBool("IsAttack", true);
    }
}
