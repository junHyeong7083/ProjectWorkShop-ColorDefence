using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum BeeAttackType
{
    NEAR,
    FAR
}


public class BeeController : MonoBehaviour,IDamageable
{
    public BeeData beeData;
    private int currentHp;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform firePos;
    private float lastAttackTime;
    private GameObject targetEnemy;
    private GameObject explicitTarget;

    [SerializeField] BeeAttackType attackType;

    public event Action<BeeController> OnDie;
    private void Awake()
    {
        currentHp = beeData.maxHp;
        lastAttackTime = -beeData.attackCooltime;
    }
    GameObject target;
    private void Update()
    {
        if (currentHp <= 0) return;

        target = explicitTarget != null ? explicitTarget : FindClosestEnemy();

        if (target != null)
        {
            float dist = Vector3.Distance(transform.position, target.transform.position);

            if (explicitTarget != null && dist > beeData.attackRange)
            {
                var agent = GetComponent<NavMeshAgent>();
                if (agent != null)
                    agent.SetDestination(target.transform.position);
            }

            bool shouldAttack = explicitTarget != null || isInCombatMode;
            if (shouldAttack && dist <= beeData.attackRange)
            {
                var agent = GetComponent<NavMeshAgent>();

                // ?? 공격 전에 반드시 이동 멈춤
                if (agent != null && agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
                {
                    // 아직 도착 안했으면 공격 금지
                    return;
                }

                // 도착 후 공격 가능
                Vector3 lookDir = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z) - transform.position;
                if (lookDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
                }

                if (Time.time >= lastAttackTime + beeData.attackCooltime)
                {
                    Attack();
                    lastAttackTime = Time.time;

                    var enemyHealth = target.GetComponent<BaseEnemy>();
                    if (enemyHealth != null)
                        enemyHealth.TakeDamage(beeData.attackPower);
                }
            }
        }
        else
        {
            explicitTarget = null;
        }
    }

    private bool isInCombatMode = false;  // A-click 시 true, 우클릭이면 false

    public void SetCombatMode(bool enabled)
    {
        isInCombatMode = enabled;
    }
    public void SetAttackTarget(GameObject target)
    {
        explicitTarget = target;
    }

    // 적을 찾기
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
      //  animator.SetTrigger("IsHit");

        if (currentHp <= 0)
        {
            animator.SetBool("IsDie",true);
            StartCoroutine(Die());
        }
    }

    IEnumerator Die()
    {
        float time = Time.time;
        while(Time.time- time < 0.5f)
        {
            yield return null;
        }
        OnDie?.Invoke(this);
        animator.SetBool("IsDie", true);
        EnemyPoolManager.Instance.Return(name.Replace("(Clone)", "").Trim(), gameObject);

    }

    private void Attack()
    {
        switch(attackType)
        {
            case BeeAttackType.FAR:
                animator.SetTrigger("IsFarAttack");

                float dist = Vector3.Distance(transform.position, target.transform.position);
                float ratio = Mathf.Clamp01(dist / beeData.attackRange);
                float delay = Mathf.Lerp(0.5f, 0.05f, ratio);
                StartCoroutine(DelayedFire(delay));
               // BulletPool.Instance.GetBeeBullet(firePos.position, target.transform, beeData.attackPower);
                break;

            case BeeAttackType.NEAR:
                animator.SetTrigger("IsNearAttack");
                break;
        }
    }

    private IEnumerator DelayedFire(float delay)
    {
        Debug.Log($"delay : {delay} ");
        yield return YieldCache.WaitForSeconds(delay);

        if (target != null) // 타겟이 여전히 존재할 경우만 발사
        {
            BulletPool.Instance.GetBeeBullet(firePos.position, target.transform, beeData.attackPower);
        }
    }
}
