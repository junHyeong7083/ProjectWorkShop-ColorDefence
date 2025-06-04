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
        //Debug.Log("���׷��̵�");
        selectedPlaceable?.Upgrade();
        Select(selectedPlaceable); // UI �簻��
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
                // �ͷ�/�潺 �� Placeable�� �ش�ǵ��� �˻�
                if (!hit.collider.GetComponent<PlaceableBase>())
                {
                    Hide();
                }
            }
            else
            {
                // ���� �� ���� Ŭ��
                Hide();
            }
        }
        // ��Ŭ�� ����
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
