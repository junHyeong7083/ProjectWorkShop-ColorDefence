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

    [SerializeField] private GameObject hpBarPrefab;
    private GameObject hpBarInstance;
    [SerializeField] private float hpBarOffset;
    private HpBarUI hpBarUI;

    [SerializeField] BeeAttackType attackType;

    public event Action<BeeController> OnDie;
    private void Awake()
    {
        currentHp = beeData.maxHp;
        lastAttackTime = -beeData.attackCooltime;

        CreateHpBar();
    }
    GameObject target;


    private void CreateHpBar()
    {
        if (hpBarPrefab == null) return;
        hpBarInstance = Instantiate(hpBarPrefab, transform);
        hpBarInstance.transform.localPosition = Vector3.up * hpBarOffset;
        hpBarUI = hpBarInstance.GetComponent<HpBarUI>();
        UpdateHpBar();
    }



    private void UpdateHpBar()
    {
        if (hpBarUI != null)
            hpBarUI.SetFill(Mathf.Clamp01((float)currentHp / beeData.maxHp));
    }


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
            bool canAttack = shouldAttack && dist <= beeData.attackRange;
            bool isInAttackCooldown = Time.time < lastAttackTime + beeData.attackCooltime;

            if (canAttack)
            {
                var agent = GetComponent<NavMeshAgent>();
                if (agent != null && agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
                    return;

                // ✅ 공격 쿨타임이 끝났을 때만 회전 및 공격 실행
                if (!isInAttackCooldown)
                {
                    Vector3 lookDir = target.transform.position - transform.position;
                    lookDir.y = 0f;
                    if (lookDir != Vector3.zero)
                    {
                        Quaternion targetRot = Quaternion.LookRotation(lookDir);
                        transform.rotation = targetRot;
                    }

                    Attack();
                    lastAttackTime = Time.time;

                    var enemyHealth = target.GetComponent<BaseEnemy>();
                    if (enemyHealth != null)
                        enemyHealth.TakeDamage(beeData.attackPower);
                }
            }
            else
            {
                explicitTarget = null;
            }
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
        animator.SetTrigger("IsHit");
        UpdateHpBar();
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

        Destroy(this.gameObject);
        
        // EnemyPoolManager.Instance.Return(name.Replace("(Clone)", "").Trim(), gameObject);

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
