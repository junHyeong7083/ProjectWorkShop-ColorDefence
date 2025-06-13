using UnityEngine;

[CreateAssetMenu(menuName = "Game/BeeData")]
public class BeeData : ScriptableObject
{
    public int maxHp;
    public float moveSpeed;
    public int attackPower;
    public float attackCooltime;

    public float attackRange;
}

