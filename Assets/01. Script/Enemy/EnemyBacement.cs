using UnityEngine;

public class EnemyBacement : MonoBehaviour
{
    [SerializeField] private GameObject hpBarPrefab;
    private GameObject hpBarInstance;
    [SerializeField] private float hpBarOffset;
    private HpBarUI hpBarUI;

    [SerializeField] private int MaxHp;
    private int currentHp;

    private void Awake()
    {
        currentHp = MaxHp;

        CreateHpBar();
    }

    private void CreateHpBar()
    {
        if (hpBarPrefab == null) return;
        hpBarInstance = Instantiate(hpBarPrefab, transform);
        hpBarInstance.transform.localPosition = Vector3.up * hpBarOffset;
        hpBarUI = hpBarInstance.GetComponent<HpBarUI>();
        UpdateHpBar();
    }
    /*


        public void TakeDamage(int damage)
        {
            if (currentHp <= 0) return;

            currentHp -= damage;
            UpdateHpBar();

            if (currentHp <= 0)
            {
                Die();
            }
        }*/

    private void UpdateHpBar()
    {
        if (hpBarUI != null)
            hpBarUI.SetFill(Mathf.Clamp01((float)currentHp / MaxHp));
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            // 승리

        }
    }



    private void Die()
    {
        Destroy(gameObject); // 혹은 비활성화만 해도 됨
    }
}
