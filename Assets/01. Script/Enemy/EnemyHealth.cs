using UnityEngine;
using UnityEngine.UI;
using System;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private GameObject hpBarPrefab;
    [SerializeField] private float offset;

    private Enemy enemy;
    private int maxHp;
    public int currentHp;

    private GameObject hpBarInstance;
    private HpBarUI hpBarUI;

    public event Action<Enemy> OnDie;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        maxHp = enemy.Data.MaxHp;
        currentHp = maxHp;

        CreateHpBar();

       // Debug.Log(currentHp);
    }

    private void OnEnable() => ResetHp();
    void CreateHpBar()
    {
        if (hpBarPrefab == null) return;

        hpBarInstance = Instantiate(hpBarPrefab, this.transform);
        hpBarInstance.transform.localPosition = Vector3.up * offset;

        hpBarUI = hpBarInstance.GetComponent<HpBarUI>();

        UpdateHpBar();
    }
    // ���� ����� EnemyPool�� ��ȯ���ִ� �Լ�
    void Death()
    {
        OnDie?.Invoke(enemy);
        EnemyPoolManager.Instance.Return(enemy.name.Replace("(Clone)", "").Trim(), gameObject);
        Debug.Log("Death!!");
    }


    // ������ �پ��� ����
    public void TakeDamage(int _damage)
    {
        currentHp -= _damage;
        UpdateHpBar();
        if (currentHp <= 0) Death();
    }




    // Hp ���º�ȭ => image.fillamount �����ϱ�
    void UpdateHpBar()
    {
        if(hpBarInstance != null) hpBarUI.SetFill(Mathf.Clamp01((float)currentHp / maxHp));
    }


    public void ResetHp()
    {
        currentHp = maxHp;

        UpdateHpBar();
    }


}
