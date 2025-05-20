using UnityEngine;

public class FenceManager : MonoBehaviour
{
    public static FenceManager Instance;

     FenceData fenceData;
    GameObject fencePrefab;

    GameObject previewInstance;
    bool isPlacing = false;

    [SerializeField] private Material previewGreenMaterial;
    [SerializeField] private Material previewRedMaterial;

    Quaternion fenceQuaternion;
     
    // fence에도 설치했을때 리스트에 해당 위치값을 저장해서 나중에 판매기능을 추가할거면 리스트에서 비워주는 코드만들어야할듯?
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartPlacement(FenceData data, GameObject prefab)
    {
        fenceData = data;
        fencePrefab = prefab;
        fenceQuaternion = Quaternion.identity;
        isPlacing = true;

        if (previewInstance != null) Destroy(previewInstance);

        previewInstance = Instantiate(fencePrefab);
        previewInstance.GetComponent<Collider>().enabled = false;

        foreach (var r in previewInstance.GetComponentsInChildren<Renderer>())
            r.material = previewGreenMaterial;
    }

    void Update()
    {
        if (!isPlacing || previewInstance == null)
            return;

        // 회전
        if (Input.GetKeyDown(KeyCode.Q))
        {
            previewInstance.transform.Rotate(Vector3.up * 90f);
            fenceQuaternion = previewInstance.transform.rotation;

            int temp = fenceData.Width;
            fenceData.Width = fenceData.Height;
            fenceData.Height = temp;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out var hit))
        {
            int width = fenceData.Width;
            int height = fenceData.Height;
            float tileSize = TileGridManager.Instance.cubeSize;

            int startX = Mathf.FloorToInt(hit.point.x / tileSize);
            int startZ = Mathf.FloorToInt(hit.point.z / tileSize);

            float offsetX = (width - 1) * 0.5f * tileSize;
            float offsetZ = (height - 1) * 0.5f * tileSize;

            Vector3 previewPos = new Vector3(
                startX * tileSize + offsetX,
                0,
                startZ * tileSize + offsetZ
            );
            previewInstance.transform.position = previewPos;

            bool canPlace = TileGridManager.Instance.CanPlaceTurret(startX, startZ, width, height);

            Material mat = canPlace ? previewGreenMaterial : previewRedMaterial;
            foreach (var r in previewInstance.GetComponentsInChildren<Renderer>())
                r.material = mat;

            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                PlaceFence(startX, startZ);
            }
        }
    }

    void PlaceFence(int startX, int startZ)
    {
        int width = fenceData.Width;
        int height = fenceData.Height;
        float tileSize = TileGridManager.Instance.cubeSize;

        if (!TileGridManager.Instance.CanPlaceTurret(startX, startZ, width, height))
        {
            Debug.Log($"설치 불가: 검사 좌표 ({startX}, {startZ}) ~ ({startX + width - 1}, {startZ + height - 1}) 중 일부가 null이거나 점유됨");
            return;
        }

        if (!GameManager.instance.SpendGold(fenceData.placementCost))
        {
            Debug.LogWarning("설치 불가: 골드 부족");
            return;
        }

        float offsetX = (width - 1) * 0.5f * tileSize;
        float offsetZ = (height - 1) * 0.5f * tileSize;

        Vector3 placePos = new Vector3(
            startX * tileSize + offsetX,
            0,
            startZ * tileSize + offsetZ
        );

        Destroy(previewInstance);
        previewInstance = null;
        isPlacing = false;

        GameObject fence = Instantiate(fencePrefab, placePos, fenceQuaternion);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                var tile = TileGridManager.Instance.GetTile(startX + x, startZ + z);
                if (tile != null)
                    tile.IsOccupied = true;
            }
        }

        // 매번 연산하면 비효율이니 울타리설치될때만 경로 계산
        foreach (var enemy in FindObjectsByType<EnemyPathfinder>(FindObjectsSortMode.None))
        {
            enemy.RecalculatePath();
        }
    }
}
