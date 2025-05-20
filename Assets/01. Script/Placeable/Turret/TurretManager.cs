using UnityEngine;
using UnityEngine.UI;

public class TurretManager : MonoBehaviour
{
    public static TurretManager Instance { get; private set; }

    TurretBase selectedTurret;

    private void Awake()
    {
        Instance = this;
    }

    // 터렛 설치 시작 -> PlacementManager에 위임
    public void StartPlacement(TurretData data, GameObject prefab)
    {
        PlacementManager.Instance.StartPlacement(data, prefab, PlacementType.Turret);
    }

    public void SelectTurret(TurretBase turret) => selectedTurret = turret;

    public void ClearSelection() => selectedTurret = null;

    public void OnClickUpgrade()
    {
        if (selectedTurret == null) return;
        selectedTurret.Upgrade();
    }

    public void OnClickSell()
    {
        if (selectedTurret == null) return;
        selectedTurret.Sell();
    }
}
