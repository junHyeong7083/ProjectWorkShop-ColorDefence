using UnityEngine;

public class GatlingAttackBar : MonoBehaviour
{
    [SerializeField] private GameObject attackBarPrefab;
    [SerializeField] private float offsetY = 0.5f;
    [SerializeField] private float offsetZ = -0.6f;

    private GameObject instance;
    private HpBarUI hpBarUI;
    private Transform target;
    private Camera mainCam;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    public void SpawnBar(Transform followTarget)
    {
        if (attackBarPrefab == null) return;

        instance = Instantiate(attackBarPrefab, transform);
        instance.transform.localPosition = new Vector3(0, offsetY, offsetZ); 

        hpBarUI = instance.GetComponentInChildren<HpBarUI>(); // 여기서 가져와야 함
        target = followTarget;
    }

    public void SetFillAmount(float value)
    {
        if (hpBarUI != null)
            hpBarUI.SetFill(Mathf.Clamp01(value));
    }

   /* private void LateUpdate()
    {
        if (instance == null || target == null || mainCam == null) return;

        Vector3 offset = new Vector3(0, offsetY, offsetZ);
        instance.transform.position = target.position + offset;
        instance.transform.forward = mainCam.transform.forward;
    }*/
}
