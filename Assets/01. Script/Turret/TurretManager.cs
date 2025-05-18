using UnityEngine;
using UnityEngine.UI;

public class TurretManager : MonoBehaviour
{
    public static TurretManager Instance { get; private set; }

    TurretData currentData;
    GameObject currentPrefab;
    GameObject previewInstance;


    TurretBase selectTurret;
    [Header("UI")]
    public GameObject selectionPanel;
    public Text levelText, damageText, rangeText, rateText, upgradeCostText, sellCostText;
    [SerializeField] private Material previewGreenMaterial;
    [SerializeField] private Material previewRedMaterial;

    private void Awake()
    {
        Instance = this;
    }

    // �ͷ� ��ġ ����(������ �ͷ� ���� ����)
    public void StartPlacement(TurretData data, GameObject prefab)
    {
        currentData = data;
        currentPrefab = prefab;

        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = Instantiate(prefab);
        foreach (var renderer in previewInstance.GetComponentsInChildren<Renderer>())
        {
            renderer.material = previewGreenMaterial;
        }
        previewInstance.GetComponent<Collider>().enabled = false;
        previewInstance.GetComponent<TurretBase>().enabled = false;
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // �׻� ����Ǵ� ��� Ŭ�� ó��
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out var hit1))
            {
                if (!hit1.collider.TryGetComponent<TurretBase>(out _))
                {
                    HidePanel(); // ��� Ŭ�� �� �г� ����
                }
            }
        }

        // ��ġ�� �ƴ� �� �ߴ�
        if (currentData == null || currentPrefab == null || previewInstance == null)
            return;

        if (Physics.Raycast(ray, out var hit))
        {
            int width = currentData.width;
            int height = currentData.height;
            float tileSize = TileGridManager.Instance.cubeSize;

            int startX = Mathf.FloorToInt(hit.point.x / tileSize);
            int startZ = Mathf.FloorToInt(hit.point.z / tileSize);

            float offsetX = (width - 1) * 0.5f * tileSize;
            float offsetZ = (height - 1) * 0.5f * tileSize;

            Vector3 previewPos = new Vector3((startX * tileSize) + offsetX,0, (startZ * tileSize) + offsetZ );
            previewInstance.transform.position = previewPos;

            bool canPlace = TileGridManager.Instance.CanPlaceTurret(startX, startZ, width, height);
            Material mat = canPlace ? previewGreenMaterial : previewRedMaterial;

            // ��ġ���� ���ο� ���� ��Ƽ���� ����
            foreach (var renderer in previewInstance.GetComponentsInChildren<Renderer>())
            {
                renderer.material = mat;
            }
            if (Input.GetMouseButtonDown(0))
            {
                PlaceTurret(hit.point);
            }
        }
    }

    // �ͷ� ��ġ �Լ�
    void PlaceTurret(Vector3 worldPos)
    {
        int width = currentData.width;
        int height = currentData.height;
        float tileSize = TileGridManager.Instance.cubeSize;

        int startX = Mathf.FloorToInt(worldPos.x / tileSize);
        int startZ = Mathf.FloorToInt(worldPos.z / tileSize);

        if (!TileGridManager.Instance.CanPlaceTurret(startX, startZ, width, height))
        {
            Debug.Log("��ġ �Ұ�: Ÿ�� ��ħ �Ǵ� ��� �ʰ�");
            return;
        }

        int cost = currentData.placementCost;
        if (!GameManager.instance.SpendGold(cost))
        {
            Debug.Log("��� ����!");
            return;
        }

        // ��Ȯ�� �߾� ��ġ ���
        float offsetX = (width - 1) * 0.5f * tileSize;
        float offsetZ = (height - 1) * 0.5f * tileSize;
        Vector3 placePos = new Vector3(
            (startX * tileSize) + offsetX,
            0,
            (startZ * tileSize) + offsetZ
        );

        Destroy(previewInstance);
        GameObject turret = Instantiate(currentPrefab, placePos, Quaternion.identity);
        turret.GetComponent<TurretBase>().SetTurretData(currentData);

        // Ÿ�� ���� ��ŷ
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                var tile = TileGridManager.Instance.GetTile(startX + x, startZ + z);
                if (tile != null)
                    tile.IsOccupied = true;
            }
        }

        currentData = null;
        currentPrefab = null;
        previewInstance = null;
    }


    // �ͷ� Ŭ���� ���� �г� ����
    public void SelectTurret(TurretBase turret)
    {
        selectTurret = turret;
        selectionPanel.SetActive(true);

        levelText.text = $"Lv.{turret.CurrentLevel}";
        damageText.text = $"Damage: {turret.GetDamage()}";
        rangeText.text = $"Range: {turret.GetRange():F1}";
        rateText.text = $"Rate: {turret.GetAttackRate():F2}";
        upgradeCostText.text = $"Upgrade Cost: {turret.GetUpgradeCost()}";
    }


    // �г� ����� && ���� ����
    public void HidePanel()
    {
        selectionPanel.SetActive(false);
        selectTurret = null;
    }

    // UI��ư�� ���׷��̵�
    public void OnClickUpgrade()
    {
        if(selectTurret != null)
        {
            selectTurret.Upgrade();
            SelectTurret(selectTurret);
        }
    }

    // UI��ư�� �Ǹ�

    public void OnClickSell()
    {
        if(selectTurret != null) 
        {
            selectTurret.Sell();

            HidePanel();
        }
    }
}
