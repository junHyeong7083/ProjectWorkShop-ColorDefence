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
     
    // fence���� ��ġ������ ����Ʈ�� �ش� ��ġ���� �����ؼ� ���߿� �Ǹű���� �߰��ҰŸ� ����Ʈ���� ����ִ� �ڵ常�����ҵ�?
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

        // ȸ��
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
            Debug.Log($"��ġ �Ұ�: �˻� ��ǥ ({startX}, {startZ}) ~ ({startX + width - 1}, {startZ + height - 1}) �� �Ϻΰ� null�̰ų� ������");
            return;
        }

        if (!GameManager.instance.SpendGold(fenceData.placementCost))
        {
            Debug.LogWarning("��ġ �Ұ�: ��� ����");
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

        // �Ź� �����ϸ� ��ȿ���̴� ��Ÿ����ġ�ɶ��� ��� ���
        foreach (var enemy in FindObjectsByType<EnemyPathfinder>(FindObjectsSortMode.None))
        {
            enemy.RecalculatePath();
        }
    }
}
