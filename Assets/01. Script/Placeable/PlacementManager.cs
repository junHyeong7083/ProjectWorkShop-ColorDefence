using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

public enum PlacementType { Turret, Fence }

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [SerializeField] private Material previewGreen;
    [SerializeField] private Material previewRed;
    [SerializeField] private Color previewRangeColor = new Color(0f, 1f, 1f, 1f);

    private GameObject currentPrefab;
    private ScriptableObject currentData;
    private GameObject previewInstance;
    private PlacementType placementType;

    private Vector3 currentPreviewPos;
    private List<TileData> simulatedTiles = new();
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    public void StartPlacement(ScriptableObject data, GameObject prefab, PlacementType type)
    {
        Debug.Log($"StartPlacement called with type: {type}, prefab: {prefab?.name}, data: {data?.name}");
        CancelPreview();

        currentData = data;
        currentPrefab = prefab;
        placementType = type;

        previewInstance = Instantiate(prefab);
        previewInstance.GetComponent<Collider>().enabled = false;

        foreach (var renderer in previewInstance.GetComponentsInChildren<Renderer>())
            renderer.material = previewGreen;

        // 부드러운 위치 초기화
        currentPreviewPos = previewInstance.transform.position;
    }
    EnemyPathfinder enemyPathfinder = new EnemyPathfinder();
    private void Update()
    {
        if (previewInstance == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.Log($"[Ray] Origin: {ray.origin}, Direction: {ray.direction}");

        if (!Physics.Raycast(ray, out var hit))
        {
        //    Debug.LogWarning("[Raycast] Did not hit anything.");
            return;
        }

      //  Debug.Log($"Hit point: {hit.point}");

        int width = 0, height = 0;

        if (placementType == PlacementType.Turret)
        {
            var data = currentData as TurretData;
            if (data == null)
            {
                //Debug.LogError("TurretData is null!");
                return;
            }
            width = data.width;
            height = data.height;
        }
        else if (placementType == PlacementType.Fence)
        {
            var data = currentData as FenceData;
            if (data == null)
            {
             //   Debug.LogError("FenceData is null!");
                return;
            }
            width = data.Width;
            height = data.Height;
        }

        float tileSize = TileGridManager.Instance.cubeSize;
       // Debug.Log($"Tile size: {tileSize}, PlacementType: {placementType}, width: {width}, height: {height}");

        int startX = Mathf.FloorToInt(hit.point.x / tileSize);
        int startZ = Mathf.FloorToInt(hit.point.z / tileSize);

        float offsetX = (width - 1) * 0.5f * tileSize;
        float offsetZ = (height - 1) * 0.5f * tileSize;

        Vector3 previewPos = new Vector3(startX * tileSize + offsetX, 0f, startZ * tileSize + offsetZ);

      //  Debug.Log($"startX: {startX}, startZ: {startZ}, previewPos: {previewPos}");

        previewInstance.transform.position = previewPos;
        bool canPlace = TileGridManager.Instance.CanPlaceTurret(startX, startZ, width, height);

       


        foreach (var enemyPathfinder in FindObjectsByType<EnemyPathfinder>(FindObjectsSortMode.None))
        {
            if (!enemyPathfinder.WouldHavePathIf(simulatedTiles))
            {
                canPlace = false;
                break;
            }
        }


        if (canPlace)
        {
            foreach (var renderer in previewInstance.GetComponentsInChildren<Renderer>())
                renderer.material = previewGreen;
        }
        else
        {
            foreach (var renderer in previewInstance.GetComponentsInChildren<Renderer>())
                renderer.material = previewRed;
        }


        if (Input.GetMouseButtonDown(0) && canPlace)
        {
       //     Debug.Log($"Placing {placementType} at ({startX}, {startZ})");
            if (placementType == PlacementType.Turret)
                PlaceTurret(startX, startZ, previewPos);
            else if (placementType == PlacementType.Fence)
                PlaceFence(startX, startZ, previewPos);  // Fence 설치 함수 필요
        }
    }

    private void PlaceTurret(int startX, int startZ, Vector3 pos)
    {
        var data = currentData as TurretData;
        if (data == null) return;

        if (!GameManager.instance.SpendGold(data.placementCost))
        {
            Debug.LogWarning("Not enough gold to place turret.");
            return;
        }

        GameObject turret = Instantiate(currentPrefab, pos, Quaternion.identity);
        TurretBase turretBase = turret.GetComponent<TurretBase>();
        turretBase.SetData(data);

        for (int x = 0; x < data.width; x++)
        {
            for (int z = 0; z < data.height; z++)
            {
                var tile = TileGridManager.Instance.GetTile(startX + x, startZ + z);
                if (tile != null)
                {
                    tile.TargetingTurret = turretBase;
                    tile.ColorState = TileColorState.Player;
                }
            }
        }

        CancelPreview();
    }

    // PlacementManager.cs 내 PlaceFence 함수 전체 수정 예시
    private void PlaceFence(int startX, int startZ, Vector3 pos)
    {
        var data = currentData as FenceData;
        if (data == null)
        {
            Debug.LogError("FenceData is null in PlaceFence!");
            return;
        }

        if (!GameManager.instance.SpendGold(data.placementCost))
        {
            Debug.LogWarning("Not enough gold to place fence.");
            return;
        }

        GameObject fence = Instantiate(currentPrefab, pos, Quaternion.identity);
        Fence fenceBase = fence.GetComponent<Fence>();
        if (fenceBase != null)
            fenceBase.SetData(data);

        for (int x = 0; x < data.Width; x++)
        {
            for (int z = 0; z < data.Height; z++)
            {
                var tile = TileGridManager.Instance.GetTile(startX + x, startZ + z);
                if (tile != null)
                {
                    tile.OccupyingFence = fenceBase;   // 울타리 점유 표시
                    tile.ColorState = TileColorState.Player; // 필요시 타일 상태 변경
                }
            }
        }

        CancelPreview();
        foreach (var enemyPathfinder in FindObjectsByType<EnemyPathfinder>(FindObjectsSortMode.None))
        {
            enemyPathfinder.RecalculatePath();
        }
    }


    private void CancelPreview()
    {
        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = null;

        currentData = null;
        currentPrefab = null;
    }
}
