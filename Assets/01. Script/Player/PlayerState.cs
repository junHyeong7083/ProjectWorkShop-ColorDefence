using UnityEngine;

[CreateAssetMenu( menuName = "Game/PlayerState")]
public class PlayerState : ScriptableObject
{
    public float Damage;
    public float AttackRange;
    public float AttackRate;
    public float Hp;
    public float SpawnSpeed;

    public float Speed;
}
