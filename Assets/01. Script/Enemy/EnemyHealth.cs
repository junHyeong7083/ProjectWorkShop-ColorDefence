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

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        maxHp = enemy.Data.MaxHp;
        currentHp = maxHp;

        CreateHpBar();

       // Debug.Log(currentHp);
    }

    void CreateHpBar()
    {
        if (hpBarPrefab == null) return;

        hpBarInstance = Instantiate(hpBarPrefab, this.transform);
        hpBarInstance.transform.localPosition = Vector3.up * offset;

        hpBarUI = hpBarInstance.GetComponent<HpBarUI>();

        UpdateHpBar();
    }




    // ������ �پ��� ����
    public void TakeDamage(int _damage)
    {
        currentHp -= _damage;
        UpdateHpBar();
        if (currentHp <= 0) Death();
    }

    // ���� ����� EnemyPool�� ��ȯ���ִ� �Լ�
    void Death() => EnemyPoolManager.Instance.Return(enemy.name.Replace("(Clone)", "").Trim(), gameObject);



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
