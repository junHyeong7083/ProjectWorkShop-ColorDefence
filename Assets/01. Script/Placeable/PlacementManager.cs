using UnityEngine;
using System.Collections.Generic;
using FischlWorks_FogWar;

public enum PlacementType { Turret, Fence, Upgrade }

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [BigHeader("PlacementManager")]
    [SerializeField] private Material previewGreen;
    [SerializeField] private Material previewRed;
    [SerializeField] private Material previewUpgrade;

    [SerializeField] Material currentmaterial;
    [SerializeField] private Texture2D cursorTexture;

    private Texture2D originCursorTexture;
    private GameObject currentPrefab;
    private ScriptableObject currentData;
    private GameObject previewInstance;
    private PlacementType placementType;

    private int lastStartX;
    private int lastStartZ;
    private Vector3 lastPreviewPos;
    private bool isCanPlace = false;
    public bool IsCanPlace => isCanPlace;

    public bool IsPlacing { get;  set; } = false;

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
        {
            // 업그레이드는 설치 시스템과 분리
            return;
        }

        IsPlacing = true;

        previewInstance = Instantiate(prefab);
        previewInstance.GetComponent<Collider>().enabled = false;

        Transform installParts = previewInstance.transform.Find("InstallParts");
        if (installParts != null)
            installParts.gameObject.SetActive(false);

        foreach (var r in previewInstance.GetComponentsInChildren<Renderer>())
            r.material = previewGreen;

        originCursorTexture = null;
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
    }

    private void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out var hit)) return;

        // 업그레이드 프리뷰 처리
        if (placementType == PlacementType.Upgrade)
        {
           /* if (!IsPlacing)
                return; // 업그레이드 완료 후에도 마우스 올릴 때 보이는 문제 방지*/

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

                    Transform installParts = turret.transform.Find("InstallParts");
                    if (installParts != null)
                        installParts.gameObject.SetActive(false);
                }
            }
            else
            {
                ClearUpgradePreview();
            }
            return;
        }


        if (!IsPlacing || previewInstance == null) return;

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
        simulatedTiles = new();
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                var tile = TileGridManager.Instance.GetTile(startX + x, startZ + z);
                if (tile != null)
                    simulatedTiles.Add(tile);
            }
        }
        isCanPlace = TileGridManager.Instance.CanPlaceTurret(startX, startZ, width, height);
        foreach (var tile in simulatedTiles)
        {
            if (tile == null)
            {
                Debug.LogWarning("[설치불가] 타일이 null임");
                isCanPlace = false;
                break;
            }

            if (tile.ColorState != TileColorState.Player)
            {
                Debug.LogWarning($"[설치불가] 타일 ({tile.GridPos}) = {tile.ColorState}");
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
        Transform installParts = turret.transform.Find("InstallParts");
        if (installParts != null)
            installParts.gameObject.SetActive(true);


        TurretBase turretBase = turret?.GetComponent<TurretBase>();
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

        //var marker = turret.GetComponent<MiniMapMarker>();
        //if (marker != null) marker.enabled = true;

        CancelPreview();
    }

    public void PlaceFence(int startX, int startZ, Vector3 pos)
    {
        var data = currentData as FenceData;
        if (data == null) return;

        GameObject fence = Instantiate(currentPrefab, pos, Quaternion.identity);
        Fence fenceBase = fence?.GetComponent<Fence>();
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
            {
                if (i < originalMaterials.Length)
                    renderers[i].material = originalMaterials[i];
            }

            Transform installParts = upgradeTarget.transform.Find("InstallParts");
            if (installParts != null)
            {
                installParts.gameObject.SetActive(true);

                if (currentmaterial != null && installParts.childCount > 0)
                {
                    var childRenderer = installParts.GetChild(0).GetComponent<Renderer>();
                    if (childRenderer != null)
                        childRenderer.material = currentmaterial;

                    Debug.Log($"ChildName : {childRenderer.gameObject.name}");

                    //currentmaterial = null;
                }
            }
        }

        upgradeTarget = null;
        originalMaterials = null;
    }
}
