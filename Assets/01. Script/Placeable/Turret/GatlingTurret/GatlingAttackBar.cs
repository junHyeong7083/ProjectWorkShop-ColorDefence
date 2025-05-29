using UnityEngine;

public class GatlingAttackBar : MonoBehaviour
{
    [SerializeField] private GameObject attackBarPrefab;
    [SerializeField] private float offsetY = 1f;
    [SerializeField] private float offsetZ = 10f;

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
        instance.transform.localPosition = new Vector3(-0.5f, 0.2f, -0.5f);

        hpBarUI = instance.GetComponentInChildren<HpBarUI>(); // 공격 바 UI 컴포넌트 가져오기
        target = followTarget;
    }

    public void SetFillAmount(float value)
    {
        if (hpBarUI != null)
            hpBarUI.SetFill(Mathf.Clamp01(value));
    }

    private void LateUpdate()
    {
        if (instance == null || target == null || mainCam == null) return;

        attackBarPrefab.transform.LookAt(attackBarPrefab.transform.position + mainCam.transform.rotation * Vector3.back, mainCam.transform.rotation * Vector3.up);
    }
}
