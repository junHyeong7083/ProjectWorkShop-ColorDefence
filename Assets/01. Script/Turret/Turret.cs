using System.Collections;
using UnityEngine;

public class Turret : MonoBehaviour
{
    public TurretData turretData { get; private set; } // �ν����Ϳ��� ���� (�׽�Ʈ��)
    int currentLevel = 1;

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
        currentLevel = 1;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(StartAttack());
    }

    public void Upgrade()
    {
        currentLevel++;
    }


    void PerformAction()
    {
        switch (turretData.actionType)
        {
            case TurretActionType.AttackEnemy:
                AttackEnemy();
                break;
            case TurretActionType.AttackTile:
                PaintTiles();
                break;
            case TurretActionType.Both:
                AttackEnemy();
                PaintTiles();
                break;
        }
    }

    void AttackEnemy() => Debug.Log($"[{turretData.turretType}] ���� ({turretData.baseDamage})");
    void PaintTiles() => Debug.Log($"[{turretData.turretType}] Ÿ�� ����");

    int GetDamage() => turretData.baseDamage + (turretData.damageGrowth * (currentLevel - 1));
    float GetRange() => turretData.baseAttackRange + (turretData.attackRangeGrowth * (currentLevel - 1)); 
    float GetAttackRate() => turretData.baseAttackRate + (turretData.attackRateGrowth * (currentLevel - 1));


}
