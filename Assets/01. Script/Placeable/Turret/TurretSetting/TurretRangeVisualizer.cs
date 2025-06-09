using UnityEngine;

public class TurretRangeVisualizer : MonoBehaviour
{
    [SerializeField]
    GameObject rangePrefab;
    GameObject rangeInstance;
    private void Start()
    {
        rangePrefab.SetActive(false);
    }


    // ��Ÿ� �ð�ȭ ǥ��
    // - range ũ�⿡ ���� ������ ����
    // - ���� ������ Instantiate�ؼ� ����
    public void Show(float range)
    {
        if (rangeInstance == null)
        {
            rangeInstance = Instantiate(rangePrefab);
            rangeInstance.transform.SetParent(transform);
            rangeInstance.transform.localPosition = Vector3.zero + Vector3.up*2f;
            rangeInstance.transform.localRotation = Quaternion.Euler(90, 0, 0);
        }
        Debug.Log("Range : " + range);

        // ���� ��Ÿ� ����� ��Ÿ� *ť�긦 ���� ť����ǥ�� ���ǹǷ� �ٽ� /cubesize�� ���� �ð���ũ�� ǥ��
        float tileBaseRange = range / TileGridManager.Instance.cubeSize - TileGridManager.Instance.cubeSize*0.5f;
        rangeInstance.transform.localScale = new Vector3(tileBaseRange , tileBaseRange, tileBaseRange);
        rangeInstance.SetActive(true);
    }

    // ��Ÿ� �ð�ȭ ����
    public void Hide()
    {
        if (rangeInstance != null)
            rangeInstance.SetActive(false);
    }
   
}
