using UnityEngine;

public class UIButtonTurret : MonoBehaviour
{
    private TurretData turretData;
    private GameObject turretPrefab;


    private FenceData fenceData;
    private GameObject fencePrefab;

    public void InitTurret(TurretData data, GameObject prefab)
    {
        turretData = data;
        turretPrefab = prefab;
    }

    public void InitFence(FenceData data, GameObject prefab)
    {
        fenceData = data;
        fencePrefab = prefab;
    }

    public void OnClickTurret()
    {
        PlacementManager.Instance.StartPlacement(turretData, turretPrefab, PlacementType.Turret);
    }

    public void OnClickFence()
    {
        PlacementManager.Instance.StartPlacement(fenceData, fencePrefab, PlacementType.Fence);
    }
}
