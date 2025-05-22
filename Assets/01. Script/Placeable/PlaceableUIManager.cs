using UnityEngine;

public class PlaceableUIManager : MonoBehaviour
{
    public static PlaceableUIManager Instance;

    [Header("UI")]
    public UIReferences ui;

    private PlaceableBase selectedPlaceable;
    private IPlaceableUIBinder binder;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Select(PlaceableBase placeableBase)
    {
        selectedPlaceable = placeableBase;

        ui.selectPanel.SetActive(true);

        binder = GetBinder(placeableBase);
        binder?.BindUI(placeableBase);
    }

    public void RequestUpgrade()
    {
        selectedPlaceable?.Upgrade();
        Select(selectedPlaceable); // UI Àç°»½Å
    }

    public void RequestSell()
    {
        selectedPlaceable?.Sell();
        Hide();
    }

    public void Hide()
    {
        ui.selectPanel.SetActive(false);
        selectedPlaceable = null;
        binder = null;
    }

    private IPlaceableUIBinder GetBinder(PlaceableBase placeable)
    {
        if (placeable is TurretBase) return new TurretUIBinder(ui);
        if (placeable is Fence) return new FenceUIBinder(ui);
        return null;
    }
}
