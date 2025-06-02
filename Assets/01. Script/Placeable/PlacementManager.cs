using UnityEngine;
using System.Collections.Generic;
using FischlWorks_FogWar;
using static FischlWorks_FogWar.csFogWar;

public enum PlacementType { Turret, Fence }

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [BigHeader("PlacementManager")]
    [SerializeField] private Material previewGreen;
    [SerializeField] private Material previewRed;
    [SerializeField] private Texture2D cursorTexture;

    private Texture2D originCursorTexture;
    private GameObject currentPrefab;
    private ScriptableObject currentData;
    private GameObject previewInstance;
    private PlacementType placementType;

    private int lastStartX;
    private int lastStartZ;
    private Vector3 lastPreviewPos;
    bool isCanPlace = false;
    public bool IsCanPlace  => isCanPlace;

    public bool IsPlacing { get; private set; } = false;

    private List<TileData> simulatedTiles = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    /// <summary>
    /// 카드 드래그가 TopPanel 절반 이상 겹치는 순간 호출
    /// → Preview 인스턴스를 생성하고 IsPlacing을 true로 세팅
    /// </summary>
    public void StartPlacement(ScriptableObject data, GameObject prefab, PlacementType type)
    {
        CancelPreview();

        currentData = data;
        currentPrefab = prefab;
        placementType = type;
        IsPlacing = true;

        // Preview 인스턴스 생성
        previewInstance = Instantiate(prefab);
        previewInstance.GetComponent<Collider>().enabled = false;

        foreach (var r in previewInstance.GetComponentsInChildren<Renderer>())
            r.material = previewGreen;

        // 커서 변경
        originCursorTexture = null;
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }

    private void Update()
    {
        if (previewInstance == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) return;

        int width = 0, height = 0;
        if (placementType == PlacementType.Turret)
        {
            var data = currentData as TurretData;
            if (data == null) return;
            width = data.width;
            height = data.height;
        }
        else if (placementType == PlacementType.Fence)
        {
            var data = currentData as FenceData;
            if (data == null) return;
            width = data.Width;
            height = data.Height;
        }

        float tileSize = TileGridManager.Instance.cubeSize;
        int startX = Mathf.FloorToInt(hit.point.x / tileSize);
        int startZ = Mathf.FloorToInt(hit.point.z / tileSize);

        float offsetX = (width - 1) * 0.5f * tileSize;
        float offsetZ = (height - 1) * 0.5f * tileSize;
        Vector3 previewPos = new Vector3(startX * tileSize + offsetX, 0f, startZ * tileSize + offsetZ);

        previewInstance.transform.position = previewPos;

         isCanPlace = TileGridManager.Instance.CanPlaceTurret(startX, startZ, width, height);
        
        foreach (var enemyPathfinder in FindObjectsByType<EnemyPathfinder>(FindObjectsSortMode.None))
        {
            if (!enemyPathfinder.WouldHavePathIf(simulatedTiles))
            {
                isCanPlace = false;
                break;
            }
        }
        //  Debug.Log($"canPlace : {isCanPlace}");
      //  Debug.Log($"[설치 가능 판정] canPlace={isCanPlace}, start=({startX},{startZ}), size=({width}x{height})");

        foreach (var renderer in previewInstance.GetComponentsInChildren<Renderer>())
            renderer.material = isCanPlace ? previewGreen : previewRed;

        // ──────────────────────────────────────────
        // 아래 마우스 클릭 체크를 통째로 제거합니다.
        // if (Input.GetMouseButtonDown(0) && canPlace)
        // {
        //     if (placementType == PlacementType.Turret)
        //         PlaceTurret(startX, startZ, previewPos);
        //     else if (placementType == PlacementType.Fence)
        //         PlaceFence(startX, startZ, previewPos);
        // }
        // ──────────────────────────────────────────
    }


    /// <summary>
    /// 터렛 설치 로직
    /// </summary>
    public void PlaceTurret(int startX, int startZ, Vector3 pos)
    {
        var data = currentData as TurretData;
        if (data == null) return;

/*        if (!GameManager.instance.SpendGold(data.placementCost))
        {
            Debug.LogWarning("Not enough gold to place turret.");
            return;
        }
*/
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

        var marker = turret.GetComponent<MiniMapMarker>();
        if (marker != null) marker.enabled = true;

        CancelPreview();
    }

    /// <summary>
    /// 울타리 설치 로직
    /// </summary>
    public void PlaceFence(int startX, int startZ, Vector3 pos)
    {
        var data = currentData as FenceData;
        if (data == null) return;
/*
        if (!GameManager.instance.SpendGold(data.placementCost))
        {
            Debug.LogWarning("Not enough gold to place fence.");
            return;
        }
*/
        GameObject fence = Instantiate(currentPrefab, pos, Quaternion.identity);
        Fence fenceBase = fence.GetComponent<Fence>();
        if (fenceBase != null) fenceBase.SetData(data);

        for (int x = 0; x < data.Width; x++)
        {
            for (int z = 0; z < data.Height; z++)
            {
                var tile = TileGridManager.Instance.GetTile(startX + x, startZ + z);
                if (tile != null)
                {
                    tile.OccupyingFence = fenceBase;
                    tile.ColorState = TileColorState.Player;
                }
            }
        }

        // 설치 후 적 경로 재계산
        CancelPreview();
        foreach (var enemyPathfinder in FindObjectsByType<EnemyPathfinder>(FindObjectsSortMode.None))
        {
            enemyPathfinder.RecalculatePath();
        }
    }

    /// <summary>
    /// Preview 인스턴스를 즉시 파괴하고, 모든 상태 초기화
    /// </summary>
    public void CancelPreview()
    {
        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = null;

        IsPlacing = false;
        currentData = null;
        currentPrefab = null;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
