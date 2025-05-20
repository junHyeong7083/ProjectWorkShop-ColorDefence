using UnityEngine;

public enum PlacementType { Turret, Fence } // 확장시 추가하는방식으로
public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [SerializeField] private Material previewGreen;
    [SerializeField] private Material previewRed;

    private GameObject currentPrefab;
    private ScriptableObject currentData;

    private GameObject previewInstance;
    private PlacementType placementType;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }


    public void StartPlacement(ScriptableObject _data, GameObject _prefab, PlacementType _placementType)
    {
        CancelPreview();

        currentData = _data;
        currentPrefab = _prefab;
        placementType = _placementType;

        previewInstance = Instantiate(_prefab);
        previewInstance.GetComponent<Collider>().enabled = false;

        foreach (var render in previewInstance.GetComponentsInChildren<Renderer>()) render.material = previewGreen;
    }

    void PlaceTurret(int _startX, int _startZ, Vector3 _pos)
    {
        var turretData = currentData as TurretData;
        if (turretData == null) return;

        if (!GameManager.instance.SpendGold(turretData.placementCost)) return;

        GameObject turret = Instantiate(currentPrefab, _pos, Quaternion.identity);
        TurretBase turretBase = turret.GetComponent<TurretBase>();
        turretBase.SetTurretData(turretData);

        float tileSize = TileGridManager.Instance.cubeSize;
        for (int x = 0; x < turretData.width; x++)
        {
            for (int z = 0; z < turretData.height; z++)
            {
                var tile = TileGridManager.Instance.GetTile(_startX + x, _startZ + z);
                if (tile != null)
                {
                    tile.IsOccupied = true;
                    turretBase.occupiedTiles.Add(tile);
                }
            }
        }
        // preview 지우는 코드
        CancelPreview();
    }

    void PlaceFence(int _startX, int _startZ, Vector3 _pos)
    {
        var fenceData = currentData as FenceData;
        if(fenceData == null) return;   

        if (!GameManager.instance.SpendGold(fenceData.placementCost)) return;

        GameObject fence = Instantiate(currentPrefab, _pos, Quaternion.identity);

        float tileSize = TileGridManager.Instance.cubeSize;
        for (int x = 0; x < fenceData.Width; x++)
        {
            for (int z = 0; z < fenceData.Height; z++)
            {
                var tile = TileGridManager.Instance.GetTile(_startX + x, _startZ + z);

                if (tile != null) tile.IsOccupied = true;

            }
        }


        // 울타리는 설치시 경로 재계산하는게 성능 덜먹을듯
        foreach (var enemy in FindObjectsByType<EnemyPathfinder>(FindObjectsSortMode.None))
            enemy.RecalculatePath();


        // preview 지우는 코드
        CancelPreview();
    }


    void CancelPreview()
    {
        if (previewInstance != null) Destroy(previewInstance);

        previewInstance = null;
        currentData = null;
        currentPrefab = null;
    }

    private void Update()
    {
        if (previewInstance == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) return;

        int width = 0, height =0;
        if (placementType == PlacementType.Turret)
        {
            var turretData = currentData as TurretData;
            width = turretData.width;
            height = turretData.height;
        }
        else if (placementType == PlacementType.Fence)
        {
            var fenceData = currentData as FenceData;
            width = fenceData.Width;
            height = fenceData.Height;
        }

            float tileSize = TileGridManager.Instance.cubeSize;
        int startX = Mathf.FloorToInt(hit.point.x / tileSize);
        int startZ = Mathf.FloorToInt(hit.point.z / tileSize);

        float offsetX = (width - 1) * 0.5f * tileSize;
        float offsetZ = (height - 1) * 0.5f * tileSize;

        Vector3 previewPos = new Vector3(startX * tileSize + offsetX, 0, startZ * tileSize + offsetZ);
        previewInstance.transform.position = previewPos;

        bool canPlace = TileGridManager.Instance.CanPlaceTurret(startX, startZ, width, height);
        foreach (var r in previewInstance.GetComponentsInChildren<Renderer>())
            r.material = canPlace ? previewGreen : previewRed;

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            if (placementType == PlacementType.Turret)
                PlaceTurret(startX, startZ, previewPos);
            else if (placementType == PlacementType.Fence)
                PlaceFence(startX, startZ, previewPos);
        }
    }
}
