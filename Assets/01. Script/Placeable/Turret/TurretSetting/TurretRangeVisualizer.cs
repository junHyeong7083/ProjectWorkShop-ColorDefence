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
            rangeInstance.transform.localPosition = Vector3.zero;
            rangeInstance.transform.localRotation = Quaternion.Euler(90, 0, 0);
        }
        rangeInstance.transform.localScale = Vector3.one * range/2;
        rangeInstance.SetActive(true);
    }

    // 사거리 시각화 숨김
    public void Hide()
    {
        if (rangeInstance != null)
            rangeInstance.SetActive(false);
    }
   
}
