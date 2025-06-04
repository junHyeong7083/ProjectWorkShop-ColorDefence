using UnityEngine;
using UnityEngine.EventSystems;

public class PlaceableUIManager : MonoBehaviour
{
    public static PlaceableUIManager Instance;

    [Header("UI")]
    public UIReferences ui;

    private PlaceableBase selectedPlaceable;
    private TurretBase turretBase;
    private IPlaceableUIBinder binder;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Select(PlaceableBase placeableBase)
    {
        /*selectedPlaceable = placeableBase;
        ui.selectPanel.SetActive(true);

        binder = GetBinder(placeableBase);
        binder?.BindUI(placeableBase);

        PositionPanelNextTo(placeableBase.transform);*/
    }
    void PositionPanelNextTo(Transform target)
    {
        Vector3 offset = target.right * 3.5f; 

        Vector3 worldPos = target.position + offset;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        ui.selectPanel.transform.position = screenPos;
    }
    public void RequestUpgrade()
    {
        //Debug.Log("업그레이드");
        selectedPlaceable?.Upgrade();
        Select(selectedPlaceable); // UI 재갱신
    }

    public void RequestSell()
    {
        selectedPlaceable?.Sell();
        Hide();
    }
    void Update()
    {
        /*if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // 터렛/펜스 등 Placeable만 해당되도록 검사
                if (!hit.collider.GetComponent<PlaceableBase>())
                {
                    Hide();
                }
            }
            else
            {
                // 완전 빈 공간 클릭
                Hide();
            }
        }
        // 우클릭 해제
        else if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("??");
            Hide();
        }*/
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
