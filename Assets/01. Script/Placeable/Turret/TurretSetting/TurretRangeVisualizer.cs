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


    // 사거리 시각화 표시
    // - range 크기에 따라 스케일 조정
    // - 원이 없으면 Instantiate해서 생성
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

        // 현재 사거리 계산이 사거리 *큐브를 통해 큐브좌표로 계산되므로 다시 /cubesize를 통해 시각적크기 표현
        float tileBaseRange = range / TileGridManager.Instance.cubeSize - TileGridManager.Instance.cubeSize*0.5f;
        rangeInstance.transform.localScale = new Vector3(tileBaseRange , tileBaseRange, tileBaseRange);
        rangeInstance.SetActive(true);
    }

    // 사거리 시각화 숨김
    public void Hide()
    {
        if (rangeInstance != null)
            rangeInstance.SetActive(false);
    }
   
}
