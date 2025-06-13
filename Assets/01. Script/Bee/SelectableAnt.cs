using UnityEngine;

public class SelectableAnt : MonoBehaviour
{

    [SerializeField] Transform fbxTransform;
    [SerializeField] GameObject selectRing;
    [SerializeField] GameObject attackRangeRing;

    BeeController beeController;
    float attackRange;
    public bool IsSelected { get; private set; }
  
    private void Awake()
    {
        beeController = GetComponent<BeeController>();
        selectRing.gameObject.SetActive(false);
        attackRangeRing.gameObject.SetActive(false);


        attackRange = beeController.beeData.attackRange;
        attackRangeRing.transform.localScale = new Vector3(attackRange, attackRange, attackRange);
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        selectRing.gameObject.SetActive(IsSelected);

        if (!IsSelected && attackRangeRing != null)
        {
            attackRangeRing.SetActive(false); // ¼±ÅÃ ÇØÁ¦ ½Ã ÀÚµ¿ ¼û±è
        }
    }


    private void Update()
    {
        if (!IsSelected) return;

        bool show = Input.GetKey(KeyCode.A);
        attackRangeRing.gameObject.SetActive(show);

        
    }
}
