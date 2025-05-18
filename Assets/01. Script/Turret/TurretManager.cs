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

    // 터렛 배치 시작(프리뷰 터렛 생성 포함)
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

        // 항상 실행되는 빈곳 클릭 처리
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out var hit1))
            {
                if (!hit1.collider.TryGetComponent<TurretBase>(out _))
                {
                    HidePanel(); // 빈곳 클릭 시 패널 꺼짐
                }
            }
        }

        // 배치중 아닐 때 중단
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

            // 배치가능 여부에 따른 머티리얼 변경
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

    // 터렛 배치 함수
    void PlaceTurret(Vector3 worldPos)
    {
        int width = currentData.width;
        int height = currentData.height;
        float tileSize = TileGridManager.Instance.cubeSize;

        int startX = Mathf.FloorToInt(worldPos.x / tileSize);
        int startZ = Mathf.FloorToInt(worldPos.z / tileSize);

        if (!TileGridManager.Instance.CanPlaceTurret(startX, startZ, width, height))
        {
            Debug.Log("설치 불가: 타일 겹침 또는 경계 초과");
            return;
        }

        int cost = currentData.placementCost;
        if (!GameManager.instance.SpendGold(cost))
        {
            Debug.Log("골드 부족!");
            return;
        }

        // 정확한 중앙 위치 계산
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

        // 타일 점유 마킹
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


    // 터렛 클릭시 정보 패널 열기
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


    // 패널 숨기기 && 선택 해제
    public void HidePanel()
    {
        selectionPanel.SetActive(false);
        selectTurret = null;
    }

    // UI버튼용 업그레이드
    public void OnClickUpgrade()
    {
        if(selectTurret != null)
        {
            selectTurret.Upgrade();
            SelectTurret(selectTurret);
        }
    }

    // UI버튼용 판매

    public void OnClickSell()
    {
        if(selectTurret != null) 
        {
            selectTurret.Sell();

            HidePanel();
        }
    }
}
