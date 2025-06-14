using UnityEngine;


public enum EnemyAttackType
{
    NONE,
    FAR,
    NEAR
}




[CreateAssetMenu(menuName = "Game/MonsterData")]
public class MonsterData : ScriptableObject
{
    public EnemyAttackType enemyAttackType = EnemyAttackType.NONE;


    public int Width;
    public int Height;
    
    public int MaxHp;
    public float Speed;
    public float DetectRange;
    public float AttackRange;
    public float AttackDamage;
    public float AttackCoolTime;
    public TileColorState InfectColor = TileColorState.Enemy;
}

