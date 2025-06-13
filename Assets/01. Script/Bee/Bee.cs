using UnityEngine;

public class BeeController : MonoBehaviour
{
    [SerializeField] BeeData beeData;

    private int currentHp;
    private Animator animator;

    private void Awake()
    {
        currentHp = beeData.maxHp;
        animator = GetComponent<Animator>();
    }


    public void TakeDamage(int damage)
    {
        if (currentHp <= 0) return;

        currentHp -= damage;

        animator.SetTrigger("IsHit");

        if (currentHp <= 0)
        {
            animator.SetTrigger("IsDie");
        }
    }


    public void a

}
