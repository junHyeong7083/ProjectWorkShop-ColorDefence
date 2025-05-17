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

    public float attackRange;
    public float attackRate;
    public int damage;
}
