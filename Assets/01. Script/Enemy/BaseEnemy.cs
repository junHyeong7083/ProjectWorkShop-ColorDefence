using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public interface IDamageable
{
    void TakeDamage(int amount);
}

public enum EnemyState
{
    MOVE, ATTACK, FLEE, DEAD
}

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyTargetFinder))]

public class BaseEnemy : MonoBehaviour, IDamageable
{
    public MonsterData Data;

    private NavMeshAgent navMeshAgent;
    private Vector2Int goalGrid;

    public EnemyState CurrentState { get; private set; } = EnemyState.MOVE;

    private Transform targetTransform;
    private EnemyTargetFinder targetFinder;
    private GameObject hpBarInstance;
    private HpBarUI hpBarUI;
    private Animator animator;

    private int currentHp;
    private float attackTimer;
    private bool isHolding;

    private Transform overrideTarget;  // À¯´Ö °¨ÁöµÇ¸é ÀúÀå
    private bool HasOverrideTarget => overrideTarget != null;

    [SerializeField] private GameObject modelPrefab;
    [SerializeField] private GameObject hpBarPrefab;
    [SerializeField] private float hpBarOffset = 1f;
    public event Action<BaseEnemy> OnDie;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        targetFinder = GetComponent<EnemyTargetFinder>();
        currentHp = Data.MaxHp;
        animator = modelPrefab.GetComponent<Animator>();

        CreateHpBar();
    }

    private void OnEnable()
    {
        Debug.Log($"[BaseEnemy] {name} ½ÃÀÛ À§Ä¡: {transform.position} / isOnNavMesh: {GetComponent<NavMeshAgent>().isOnNavMesh}");
        currentHp = Data.MaxHp;
        CurrentState = EnemyState.MOVE;
        targetTransform = null;
        attackTimer = 0f;
    }

    private void Update()
    {
        if (isHolding) return;
        UpdateFSM();
    }
    public void Resume()
    {
        isHolding = false;
        CurrentState = EnemyState.MOVE;
    }
    private void UpdateFSM()
    {
        if (!HasOverrideTarget && targetFinder.TryFindTarget(out Transform t))
        {
            Debug.Log("À¯´Ö °¨Áö ¡æ ÀÓ½Ã °ø°Ý ÀüÈ¯");
            overrideTarget = t;
            CurrentState = EnemyState.ATTACK;
            return;
        }

        switch (CurrentState)
        {
            case EnemyState.MOVE:
                Move();
                break;
            case EnemyState.ATTACK:
                Attack();
                break;
        }
    }


     void FindPlayerUnit()
    {

    }



    private void Move()
    {
        if (HasOverrideTarget) return;

        Vector2Int currentGoal = TileGridManager.GetGridPositionFromWorld(navMeshAgent.destination);
        if (currentGoal != goalGrid)
        {
            SetGoal(goalGrid);
        }
    }

    private void Attack()
    {
        if (!HasOverrideTarget || !overrideTarget.gameObject.activeSelf)
        {
            Debug.Log("Å¸°Ù »ç¶óÁü ¡æ º¹±Í");
            overrideTarget = null;
            CurrentState = EnemyState.MOVE;
            return;
        }

        float dist = Vector3.Distance(transform.position, overrideTarget.position);
        if (dist > Data.DetectRange)
        {
            Debug.Log("Å¸°Ù ¸Ö¾îÁü ¡æ º¹±Í");
            overrideTarget = null;
            CurrentState = EnemyState.MOVE;
            return;
        }

        navMeshAgent.SetDestination(overrideTarget.position);  // µû¶ó°¡±â

        if (dist <= Data.AttackRange && attackTimer <= 0f)
        {
            var damageable = overrideTarget.GetComponent<IDamageable>();
            damageable?.TakeDamage((int)Data.AttackDamage);
            animator.SetTrigger("IsAttack");
            attackTimer = Data.AttackCoolTime;
        }
        else
        {
            attackTimer -= Time.deltaTime;
        }
    }

    public void SetGoal(Vector2Int gridPos)
    {
        goalGrid = gridPos;
        Vector3 worldPos = TileGridManager.GetWorldPositionFromGrid(gridPos.x, gridPos.y);

        if (navMeshAgent.isOnNavMesh)
            navMeshAgent.SetDestination(worldPos);
        else
            StartCoroutine(DelayedSetDestination(worldPos));
    }

    private IEnumerator DelayedSetDestination(Vector3 pos)
    {
        yield return new WaitForSeconds(0.05f);
        if (navMeshAgent.isOnNavMesh)
            navMeshAgent.SetDestination(pos);
    }

    public void TakeDamage(int dmg)
    {
        currentHp -= dmg;
        animator.SetTrigger("IsHit");
        UpdateHpBar();
        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        OnDie?.Invoke(this);
        animator.SetBool("IsDie", true);
        EnemyPoolManager.Instance.Return(name.Replace("(Clone)", "").Trim(), gameObject);
    }

    public void ResetHP()
    {
        currentHp = Data.MaxHp;
        animator.SetBool("IsDie", false);
        UpdateHpBar();

    }
    public int GetCurrentHp()
    {
        return currentHp;
    }
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
            hpBarUI.SetFill(Mathf.Clamp01((float)currentHp / Data.MaxHp));
    }
}
