using UnityEngine;


public enum TurretType
{
    Gatling, 
    Cannon,
    Laser
}

public enum TurretActionType
{
    AttackEnemy,
    AttackTile,
    Both
}

[CreateAssetMenu(menuName = "Game/TurretData")]
public class TurretData : ScriptableObject
{
    public TurretActionType actionType;
    public TurretType turretType;

    public float baseAttackRange;
    public float baseAttackRate;
    public int baseDamage;

    public float attackRangeGrowth;
    public float attackRateGrowth;
    public int damageGrowth;
    public int placementCost;

    public int width;
    public int height;

    //---------------������ ���� �Ķ����---------------//
    [Header("������ ����")]
    public float laserDuration;
    public float laserTickInterval;

}
