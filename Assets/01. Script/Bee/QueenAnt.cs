using UnityEngine;

public class QueenAnt : MonoBehaviour,IDamageable
{
    [SerializeField] private GameObject hpBarPrefab;
    private GameObject hpBarInstance;
    [SerializeField] private float hpBarOffset;
    private HpBarUI hpBarUI;

    [SerializeField] private int MaxHp;
    private int currentHp;


    [SerializeField] Animator animator;
    private void Awake()
    {
        currentHp = MaxHp;
        CreateHpBar();
    }

    private void OnEnable()
    {
        animator.SetBool("ShuffleEnd", true);
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
            hpBarUI.SetFill(Mathf.Clamp01((float)currentHp /MaxHp));
    }

    public void TakeDamage(int damage)
    {
        if (currentHp <= 0) return;

        currentHp -= damage;
        UpdateHpBar();

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        animator.SetBool("IsDie", true);
        //Destroy(gameObject); // 혹은 비활성화만 해도 됨
    }


}
