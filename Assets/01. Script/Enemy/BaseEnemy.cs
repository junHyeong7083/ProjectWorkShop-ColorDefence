using System;
using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int amount);
}

[RequireComponent(typeof(EnemyPathfinder))]
[RequireComponent(typeof(EnemyTargetFinder))]
public class BaseEnemy : MonoBehaviour, IDamageable
{
    public MonsterData Data;

    protected EnemyPathfinder pathFinder;
    protected EnemyTargetFinder targetFinder;
    protected Transform targetTransform;
    protected int currentHp;

    public event Action<BaseEnemy> OnDie;

    [SerializeField] private GameObject hpBarPrefab;
    [SerializeField] private float hpBarOffset = 1f;

    private GameObject hpBarInstance;
    private HpBarUI hpBarUI;

    public EnemyState CurrentState { get; protected set; } = EnemyState.MOVE;
    private bool isHolding;
    private Vector2Int lastGoalGrid = new(-1, -1);

    public int GroupId { get; private set; } = -1;
    public void SetGroupId(int id) => GroupId = id;

    public void Hold()
    {
        isHolding = true;
        targetTransform = null;
    }

    public void Resume()
    {
        isHolding = false;
        CurrentState = EnemyState.MOVE;
    }

    protected virtual void Awake()
    {
        pathFinder = GetComponent<EnemyPathfinder>();
        targetFinder = GetComponent<EnemyTargetFinder>();
        currentHp = Data.MaxHp;
        CreateHpBar();
    }

    protected virtual void OnEnable()
    {
        currentHp = Data.MaxHp;
        CurrentState = EnemyState.MOVE;
        targetTransform = null;
    }

    protected virtual void Update()
    {
        if (isHolding) return;
        UpdateFSM();
    }

    protected virtual void UpdateFSM()
    {
        if (targetTransform == null && targetFinder.TryFindTarget(out Transform t))
        {
            targetTransform = t;
            ScoutIntelReport(t);
            SetPathToTarget(t.position);
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

    protected virtual void Move()
    {
        if (targetTransform == null) return;

        Vector3 destination = targetTransform.position;
        Vector2Int goalGrid = WorldToGrid(destination);

        if (goalGrid != lastGoalGrid)
        {
            pathFinder.goalGridPosition = goalGrid;
            pathFinder.RecalculatePath();
            lastGoalGrid = goalGrid;
        }
    }

    protected virtual void Attack()
    {
        if (targetTransform == null) return;
        var damageable = targetTransform.GetComponent<IDamageable>();
        damageable?.TakeDamage((int)Data.AttackDamage);
    }

    public virtual void TakeDamage(int dmg)
    {
        currentHp -= dmg;
        UpdateHpBar();
        if (currentHp <= 0) Die();
    }

    protected virtual void Die()
    {
        OnDie?.Invoke(this);
        EnemyPoolManager.Instance.Return(name.Replace("(Clone)", "").Trim(), gameObject);
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

    private void SetPathToTarget(Vector3 destination)
    {
        Vector2Int goalGrid = WorldToGrid(destination);
        pathFinder.goalGridPosition = goalGrid;
        pathFinder.RecalculatePath();
        lastGoalGrid = goalGrid;
    }

    private Vector2Int WorldToGrid(Vector3 position)
    {
        float size = TileGridManager.Instance.cubeSize;
        return new Vector2Int(
            Mathf.FloorToInt(position.x / size),
            Mathf.FloorToInt(position.z / size)
        );
    }

    private void ScoutIntelReport(Transform t)
    {
        if (t.CompareTag("FastAnt"))
            ScoutIntel.ReportUnit(t);
        else if (t.CompareTag("AttackAnt"))
            ScoutIntel.ReportTurret(t);
    }
}
