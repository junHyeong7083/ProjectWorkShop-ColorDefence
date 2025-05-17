using System.Collections;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public TurretData turretData { get; private set; } // 인스펙터에서 연결 (테스트용)
    public int CurrentLevel { get; private set; } = 1;

    Coroutine attackRoutine;

    IEnumerator StartAttack()
    {
        while (true)
        {
            PerformAction();
            yield return new WaitForSeconds(GetAttackRate());
        }
    }



    public void SetTurretData(TurretData _turretData)
    {
        turretData = _turretData;
        CurrentLevel = 1;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(StartAttack());
    }

    public void Upgrade()
    {
        int cost = GetUpgradeCost();
        if (GameManager.instance.SpendGold(cost))
            CurrentLevel++;
    }


    public void Sell()
    {
        int refund = GetSellPrice();
        GameManager.instance.AddGold(refund);
        Destroy(this.gameObject);
    }

    void PerformAction()
    {
        switch (turretData.actionType)
        {
            case TurretActionType.AttackEnemy:
                //  AttackEnemy();
                break;
            case TurretActionType.AttackTile:
                //  PaintTiles();
                break;
            case TurretActionType.Both:
                //  AttackEnemy();
                //  PaintTiles();
                break;
        }
    }

    private void OnMouseDown()
    {
        TurretManager.Instance.SelectTurret(this);
        GetComponent<TurretRangeVisualizer>().Show(GetRange());
    }
    private void OnMouseUp()
    {
        GetComponent<TurretRangeVisualizer>().Hide();
    }


    void AttackEnemy() => Debug.Log($"[{turretData.turretType}] 공격 ({turretData.baseDamage})");
    void PaintTiles() => Debug.Log($"[{turretData.turretType}] 타일 감염");

    public int GetDamage() => turretData.baseDamage + (turretData.damageGrowth * (CurrentLevel - 1));
    public float GetRange() => turretData.baseAttackRange + (turretData.attackRangeGrowth * (CurrentLevel - 1));
    public float GetAttackRate() => turretData.baseAttackRate + (turretData.attackRateGrowth * (CurrentLevel - 1));

    public int GetUpgradeCost() => 100 + (CurrentLevel * 50);
    public int GetSellPrice() => 50 + (CurrentLevel * 25);

}
