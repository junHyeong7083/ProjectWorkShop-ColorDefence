// ✅ 수정 완료된 PlacementManager.cs
using UnityEngine;
using System.Collections.Generic;

public enum PlacementType { Turret, Fence, Upgrade, Unit }

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [SerializeField] private Material previewGreen;
    [SerializeField] private Material previewRed;
    [SerializeField] private Material previewUpgrade;

    [SerializeField] private Material currentmaterial;
    [SerializeField] private Texture2D cursorTexture;

    private Texture2D originCursorTexture;
    private GameObject currentPrefab;
    private ScriptableObject currentData;
    private GameObject previewInstance;
    private PlacementType placementType;

    private Vector3 lastPreviewPos;
    private bool isCanPlace = false;
    public bool IsCanPlace => isCanPlace;

    public bool IsPlacing { get; set; } = false;

    private List<TileData> simulatedTiles = new();

    private TurretBase upgradeTarget;
    private Material[] originalMaterials;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    public void StartPlacement(ScriptableObject data, GameObject prefab, PlacementType type)
    {
        CancelPreview();

        currentData = data;
        currentPrefab = prefab;
        placementType = type;

        if (type == PlacementType.Upgrade)
            return;

        IsPlacing = true;

        previewInstance = Instantiate(prefab);
        previewInstance.GetComponent<Collider>().enabled = false;

        Transform installParts = previewInstance.transform.Find("InstallParts");
        if (installParts != null)
            installParts.gameObject.SetActive(false);

        // ✅ 유닛도 포함하여 머티리얼 적용
        foreach (var r in previewInstance.GetComponentsInChildren<Renderer>())
            r.material = previewGreen;

        originCursorTexture = null;
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }

    private void Update()
    {
        if (!IsPlacing || previewInstance == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) return;

        // 업그레이드 전용 처리
        if (placementType == PlacementType.Upgrade)
        {
            var turret = hit.collider.GetComponent<TurretBase>();
            if (turret != null && upgradeTarget != turret)
            {
                ClearUpgradePreview();
                upgradeTarget = turret;
                var renderers = turret.GetComponentsInChildren<Renderer>();
                originalMaterials = new Material[renderers.Length];
                for (int i = 0; i < renderers.Length; i++)
                {
                    originalMaterials[i] = renderers[i].material;
                    renderers[i].material = previewUpgrade;
                }

                Transform installParts = turret.transform.Find("InstallParts");
                if (installParts != null)
                    installParts.gameObject.SetActive(false);
            }
            else if (turret == null)
            {
                ClearUpgradePreview();
            }
            return;
        }

        // ✅ 타일 크기 및 시작 좌표 계산
        float tileSize = TileGridManager.Instance.cubeSize;
        int width = 1, height = 1;
        if (placementType == PlacementType.Turret && currentData is TurretData tData)
        {
            width = tData.width;
            height = tData.height;
        }
        else if (placementType == PlacementType.Fence && currentData is FenceData fData)
        {
            width = fData.Width;
            height = fData.Height;
        }

        int startX = Mathf.FloorToInt(hit.point.x / tileSize);
        int startZ = Mathf.FloorToInt(hit.point.z / tileSize);

        float offsetX = (width - 1) * 0.5f * tileSize;
        float offsetZ = (height - 1) * 0.5f * tileSize;
        Vector3 previewPos = new Vector3(startX * tileSize + offsetX, 0f, startZ * tileSize + offsetZ);
        previewInstance.transform.position = previewPos;

        simulatedTiles.Clear();
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                var tile = TileGridManager.Instance.GetTile(startX + x, startZ + z);
                if (tile != null) simulatedTiles.Add(tile);
            }
        }

        isCanPlace = TileGridManager.Instance.CanPlaceTurret(startX, startZ, width, height);
        foreach (var tile in simulatedTiles)
        {
            if (tile == null || tile.ColorState != TileColorState.Player)
            {
                isCanPlace = false;
                break;
            }
        }

        foreach (var r in previewInstance.GetComponentsInChildren<Renderer>())
            r.material = isCanPlace ? previewGreen : previewRed;
    }

    public void PlaceUnit(int startX, int startZ, Vector3 pos)
    {
        if (!isCanPlace) return;

        GameObject unit = Instantiate(currentPrefab, pos, Quaternion.identity);
        if (unit.transform.childCount > 0)
            unit.transform.GetChild(0).gameObject.SetActive(true);

        CancelPreview();
    }

    public void PlaceTurret(int startX, int startZ, Vector3 pos)
    {
        var data = currentData as TurretData;
        if (data == null) return;

        GameObject turret = Instantiate(currentPrefab, pos, Quaternion.identity);
        var installParts = turret.transform.Find("InstallParts");
        if (installParts != null)
            installParts.gameObject.SetActive(true);

        var turretBase = turret.GetComponent<TurretBase>();
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

    public void PlaceFence(int startX, int startZ, Vector3 pos)
    {
        var data = currentData as FenceData;
        if (data == null) return;

        GameObject fence = Instantiate(currentPrefab, pos, Quaternion.identity);
        var fenceBase = fence.GetComponent<Fence>();
        fenceBase.SetData(data);

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
        CancelPreview();

       /* foreach (var enemyPathfinder in FindObjectsByType<EnemyPathfinder>(FindObjectsSortMode.None))
            enemyPathfinder.RecalculatePath();*/
    }

    public void CancelPreview()
    {
        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = null;

        ClearUpgradePreview();

        IsPlacing = false;
        currentData = null;
        currentPrefab = null;
        isCanPlace = false;
        simulatedTiles.Clear();

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void ClearUpgradePreview()
    {
        if (upgradeTarget != null && originalMaterials != null)
        {
            var renderers = upgradeTarget.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
                renderers[i].material = originalMaterials[i];

            var installParts = upgradeTarget.transform.Find("InstallParts");
            if (installParts != null)
            {
                installParts.gameObject.SetActive(true);
                if (currentmaterial != null && installParts.childCount > 0)
                {
                    var childRenderer = installParts.GetChild(0).GetComponent<Renderer>();
                    if (childRenderer != null)
                        childRenderer.material = currentmaterial;
                }
            }
        }
        upgradeTarget = null;
        originalMaterials = null;
    }
}