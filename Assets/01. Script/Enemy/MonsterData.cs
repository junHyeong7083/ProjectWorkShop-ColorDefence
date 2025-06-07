using UnityEngine;

[CreateAssetMenu(menuName = "Game/MonsterData")]
public class MonsterData : ScriptableObject
{
    public int Width;
    public int Height;
    
    public int MaxHp;
    public float Speed;
    public float DetectRange;
    public float AttackDamage;
    public float AttackCoolTime;
    public TileColorState InfectColor = TileColorState.Enemy;
}

