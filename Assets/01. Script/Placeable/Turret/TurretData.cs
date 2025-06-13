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

    public int Star;
    public float TurretHP;

    public float baseAttackRange;
    public float baseAttackRate;
    public int baseDamage;
    public int minAttackRange;


    public float attackRangeGrowth;
    public float attackRateGrowth;
    public int damageGrowth;
    public int placementCost;

    public int width;
    public int height;

    //---------------레이저 전용 파라미터---------------//
    [Header("레이저 전용")]
    public float laserDuration;
    public float laserTickInterval;

    public string description;

}
