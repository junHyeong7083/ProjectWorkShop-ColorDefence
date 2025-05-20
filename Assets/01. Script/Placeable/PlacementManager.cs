// PlacementManager.cs - 울타리 설치 시 거리맵 기반 경로 재계산 최적화
using UnityEngine;
using System.Collections.Generic;

public enum PlacementType { Turret, Fence }

public class PlacementManager : MonoBehaviour
{
    public static PlacementManager Instance;

    [SerializeField] private Material previewGreen;
    [SerializeField] private Material previewRed;

    private GameObject currentPrefab;
    private ScriptableObject currentData;
    private GameObject previewInstance;
    private PlacementType placementType;

    private List<Tile> simulatedTiles = new();

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

        previewInstance = Instantiate(prefab);
        previewInstance.GetComponent<Collider>().enabled = false;

        foreach (var renderer in previewInstance.GetComponentsInChildren<Renderer>())
            renderer.material = previewGreen;
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

        Vector3 previewPos = new Vector3(startX * tileSize + offsetX, 0, startZ * tileSize + offsetZ);
        previewInstance.transform.position = previewPos;

        bool canPlace = TileGridManager.Instance.CanPlaceTurret(startX, startZ, width, height);
        Dictionary<Tile, int> cachedDistanceMap = null;

        if (canPlace && placementType == PlacementType.Fence)
        {
            simulatedTiles.Clear();
            for (int x = 0; x < width; x++)
            {
                for (int z = 0; z < height; z++)
                {
                    var tile = TileGridManager.Instance.GetTile(startX + x, startZ + z);
                    if (tile != null)
                        simulatedTiles.Add(tile);
                }
            }

            TileUtility.MarkTilesOccupied(simulatedTiles, true);

            Tile goalTile = TileGridManager.Instance.GetTile(0, 0);
            cachedDistanceMap = Pathfinding.GenerateDistanceMap(goalTile);

            foreach (var enemy in FindObjectsByType<EnemyPathfinder>(FindObjectsSortMode.None))
            {
                if (!cachedDistanceMap.ContainsKey(enemy.CurrentTile))
                {
                    canPlace = false;
                    break;
                }
            }

            TileUtility.MarkTilesOccupied(simulatedTiles, false);
        }

        foreach (var r in previewInstance.GetComponentsInChildren<Renderer>())
            r.material = canPlace ? previewGreen : previewRed;

        if (Input.GetMouseButtonDown(0) && canPlace)
        {
            if (placementType == PlacementType.Turret)
                PlaceTurret(startX, startZ, previewPos);
            else if (placementType == PlacementType.Fence)
                PlaceFence(startX, startZ, previewPos, cachedDistanceMap);
        }
    }

    private void PlaceTurret(int startX, int startZ, Vector3 pos)
    {
        var data = currentData as TurretData;
        if (data == null) return;

        if (!GameManager.instance.SpendGold(data.placementCost)) return;

        GameObject turret = Instantiate(currentPrefab, pos, Quaternion.identity);
        TurretBase turretBase = turret.GetComponent<TurretBase>();
        turretBase.SetData(data);

        List<Tile> tiles = new();
        for (int x = 0; x < data.width; x++)
        {
            for (int z = 0; z < data.height; z++)
            {
                var tile = TileGridManager.Instance.GetTile(startX + x, startZ + z);
                if (tile != null && tile.ColorState != TileColorState.Enemy)
                    tiles.Add(tile);
            }
        }

        TileUtility.MarkTilesOccupied(tiles, true);
        turretBase.occupiedTiles = tiles;

        CancelPreview();
    }


    private void PlaceFence(int startX, int startZ, Vector3 pos, Dictionary<Tile, int> distanceMap)
    {
        var data = currentData as FenceData;
        if (data == null) return;

        if (!GameManager.instance.SpendGold(data.placementCost)) return;

        GameObject fencePrefab = Instantiate(currentPrefab, pos, Quaternion.identity);
        Fence fence = fencePrefab.GetComponent<Fence>();
        fence.SetData(data);

        List<Tile> tiles = new();
        for (int x = 0; x < data.Width; x++)
        {
            for (int z = 0; z < data.Height; z++)
            {
                var tile = TileGridManager.Instance.GetTile(startX + x, startZ + z);
                if (tile != null)
                    tiles.Add(tile);
            }
        }


        TileUtility.MarkTilesOccupied(tiles, true);
        fence.occupiedTiles = tiles;

        foreach (var enemy in FindObjectsByType<EnemyPathfinder>(FindObjectsSortMode.None))
            enemy.RecalculatePathFromMap(distanceMap);

        CancelPreview();
    }

    private void CancelPreview()
    {
        if (previewInstance != null) Destroy(previewInstance);
        previewInstance = null;
        currentData = null;
        currentPrefab = null;
    }
}
