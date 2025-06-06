using UnityEngine;

[DisallowMultipleComponent]
public class GatlingShooter : MonoBehaviour, ITurretShooter
{
    private GatlingTurret gatling;
    private TurretBase turret;
    private TurretTargetSelector targetSelector;
    private GatlingAttackBar attackBarUI;

    [Header("타이밍")]
    [SerializeField] private float fireDuration = 3f;
    [SerializeField] private float reloadDuration = 2f;

    private float fillAmount = 1f;
    private float reloadElapsed = 0f;
    private float lastFillAmount = -1f;

    private bool isReloading = false;
    public bool IsReloading => isReloading;
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        gatling = GetComponent<GatlingTurret>();
        turret = GetComponent<TurretBase>();
        targetSelector = GetComponent<TurretTargetSelector>();
        attackBarUI = GetComponent<GatlingAttackBar>();

        if (attackBarUI != null)
        {
            attackBarUI.SpawnBar(transform);
            attackBarUI.SetFillAmount(fillAmount);
            lastFillAmount = fillAmount;
        }
    }

    /// <summary>
    /// CombatLoop에서 매 프레임 호출할 Tick 함수
    /// </summary>
    public void Tick(float deltaTime)
    {
        if (isReloading)
        {
            animator.SetBool("IsAttack", false);
            reloadElapsed += deltaTime;
            fillAmount = reloadElapsed / reloadDuration;

            if (fillAmount >= 1f)
            {
                fillAmount = 1f;
                isReloading = false;
                reloadElapsed = 0f;
            }
        }
        else
        {
            if (!targetSelector.IsEnemyInRange)
            {
                animator.SetBool("IsAttack", false);
                fillAmount += deltaTime / reloadDuration;
                fillAmount = Mathf.Min(fillAmount, 1f);
            }
        }

        UpdateBarUI();
    }

    /// <summary>
    /// 변화가 있을 때만 FillAmount UI 갱신
    /// </summary>
    private void UpdateBarUI()
    {
        if (attackBarUI == null) return;

        if (Mathf.Abs(fillAmount - lastFillAmount) > 0.01f)
        {
            attackBarUI.SetFillAmount(fillAmount);
            lastFillAmount = fillAmount;
        }
    }

    public void ShootAtEnemy(GameObject enemy)
    {
        if (isReloading || enemy == null) return;

        Vector3 firePos = gatling.GetFirePoint().position;
        Transform target = enemy.transform;
        int damage = turret.GetDamage();

        BulletPool.Instance.GetGatlingEnemyBullet(firePos, target, damage);

        ConsumeAmmo();
    }

   
    private void ConsumeAmmo()
    {
        animator.SetBool("IsAttack", true);
        float attackRate = gatling.GetAttackRate(); // 캐싱
        fillAmount -= attackRate / fireDuration;

        if (fillAmount <= 0f)
        {
            fillAmount = 0f;
            isReloading = true;
            reloadElapsed = 0f;
        }

        UpdateBarUI();
    }
}
