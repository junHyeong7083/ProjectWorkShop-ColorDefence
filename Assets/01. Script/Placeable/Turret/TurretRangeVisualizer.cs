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


    public void Hide()
    {
        if (rangeInstance != null)
            rangeInstance.SetActive(false);
    }
   
}
