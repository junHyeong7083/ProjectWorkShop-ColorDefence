using UnityEngine;

public class UIButtonTurret : MonoBehaviour
{
    private TurretData turretData;
    private GameObject turretPrefab;

    public void Init(TurretData data, GameObject prefab)
    {
        turretData = data;
        turretPrefab = prefab;
    }

    public void OnClick()
    {
        TurretManager.Instance.StartPlacement(turretData, turretPrefab);
    }
}
