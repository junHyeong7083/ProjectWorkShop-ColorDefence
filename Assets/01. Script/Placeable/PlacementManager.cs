using UnityEngine;
using System.Collections.Generic;
using FischlWorks_FogWar;
using static FischlWorks_FogWar.csFogWar;

public enum PlacementType { Turret, Fence, Upgrade }

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [BigHeader("PlacementManager")]
    [SerializeField] private Material previewGreen;
    [SerializeField] private Material previewRed;
    [SerializeField] private Material previewUpgrade;
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
    public bool IsCanPlace => isCanPlace;

    public bool IsPlacing { get; private set; } = false;

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
        IsPlacing = true;

        if (type == PlacementType.Upgrade)
            return;

        previewInstance = Instantiate(prefab);
        previewInstance.GetComponent<Collider>().enabled = false;

        foreach (var r in previewInstance.GetComponentsInChildren<Renderer>())
            r.material = previewGreen;

        originCursorTexture = null;
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }

    private void Update()
    {
        if (!IsPlacing) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) return;

        if (placementType == PlacementType.Upgrade)
        {
            var turret = hit.collider.GetComponent<TurretBase>();
            if (turret != null)
            {
                if (upgradeTarget != turret)
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
                }
            }
            else
            {
                ClearUpgradePreview();
            }
            return;
        }

        if (previewInstance == null) return;

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

        foreach (var renderer in previewInstance.GetComponentsInChildren<Renderer>())
            renderer.material = isCanPlace ? previewGreen : previewRed;
    }

    public void PlaceTurret(int startX, int startZ, Vector3 pos)
    {
        var data = currentData as TurretData;
        if (data == null) return;

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

    public void PlaceFence(int startX, int startZ, Vector3 pos)
    {
        var data = currentData as FenceData;
        if (data == null) return;

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

        CancelPreview();
        foreach (var enemyPathfinder in FindObjectsByType<EnemyPathfinder>(FindObjectsSortMode.None))
        {
            enemyPathfinder.RecalculatePath();
        }
    }

    public void CancelPreview()
    {
        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = null;

        ClearUpgradePreview();

        IsPlacing = false;
        currentData = null;
        currentPrefab = null;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    private void ClearUpgradePreview()
    {
        if (upgradeTarget != null && originalMaterials != null)
        {
            var renderers = upgradeTarget.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (i < originalMaterials.Length)
                    renderers[i].material = originalMaterials[i];
            }
        }

        upgradeTarget = null;
        originalMaterials = null;
    }
}
