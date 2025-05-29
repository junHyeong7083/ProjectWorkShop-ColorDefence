using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMapRenderer : MonoBehaviour
{
    public static MiniMapRenderer Instance { get; private set; }
    [SerializeField] private Transform rtsCameraTransform;
    [SerializeField] private RawImage miniMapImage;
    [SerializeField] private int textureSize = 256;
    [SerializeField] private float worldSize = 64f;

    private Texture2D texture;
    private Color32[] pixels;

    private List<MiniMapMarker> enemyMarkers = new();
    private List<MiniMapMarker> towerAntMarkers = new();
    private List<MiniMapMarker> fastAntMarkers = new();
    private List<MiniMapMarker> spawnAntMarkers = new();

    [SerializeField] private float renderInterval = 0.1f;
    private float timer;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        texture = new Texture2D(textureSize, textureSize);
        texture.filterMode = FilterMode.Point;
        pixels = new Color32[textureSize * textureSize];
        miniMapImage.texture = texture;

        ClearMap();

        var gridCenter = new Vector2Int(TileGridManager.Instance.Width / 2, TileGridManager.Instance.Height / 2);
        var worldCenter = TileGridManager.GetWorldPositionFromGrid(gridCenter.x, gridCenter.y);
        var minimapCenter = WorldToMiniMap(worldCenter);

        Debug.Log($"[중앙 디버그] GridPos: {gridCenter} → WorldPos: {worldCenter} → MiniMapPos: {minimapCenter} (expected around {textureSize / 2},{textureSize / 2})");
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= renderInterval)
        {
            timer = 0f;
            RenderMiniMap();
        }
    }

    public void RegisterEnemy(MiniMapMarker marker)
    {
        if (!enemyMarkers.Contains(marker))
            enemyMarkers.Add(marker);
    }

    public void RegisterTowerAnt(MiniMapMarker marker)
    {
        if (!towerAntMarkers.Contains(marker))
            towerAntMarkers.Add(marker);
    }
    public void RegisterFastAnt(MiniMapMarker marker)
    {
        if(!fastAntMarkers.Contains(marker))
            fastAntMarkers.Add(marker);
    }

    public void RegisterSpawnAnt(MiniMapMarker marker)
    {
        if(!spawnAntMarkers.Contains(marker))
            spawnAntMarkers.Add(marker);
    }

    public void Unregister(MiniMapMarker marker)
    {
        enemyMarkers.Remove(marker);
        towerAntMarkers.Remove(marker);
        fastAntMarkers.Remove(marker);
        spawnAntMarkers.Remove(marker);
    }

    private void ClearMap()
    {
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.black;

        ApplyPixels();
    }

    private void ApplyPixels()
    {
        texture.SetPixels32(pixels);
        texture.Apply();
    }

    private Vector2Int WorldToMiniMap(Vector3 worldPos)
    {
        float worldWidth = TileGridManager.Instance.Width * TileGridManager.Instance.cubeSize;
        float worldHeight = TileGridManager.Instance.Height * TileGridManager.Instance.cubeSize;

        // 월드 중심으로 기준 이동
        Vector3 centerOffset = new Vector3(worldWidth, 0f, worldHeight) * 0.5f;
        Vector3 localPos = worldPos - centerOffset;

        // 카메라 회전 적용 (반대로 회전)
        Vector3 rotated = Quaternion.Inverse(rtsCameraTransform.rotation) * localPos;

        // 다시 미니맵 좌표계로 변환
        float xRatio = (rotated.x + worldWidth / 2f) / worldWidth;
        float yRatio = (rotated.z + worldHeight / 2f) / worldHeight;

        int x = Mathf.Clamp((int)(xRatio * textureSize), 0, textureSize - 1);
        int y = Mathf.Clamp((int)(yRatio * textureSize), 0, textureSize - 1);

        // 디버그 로그
        Debug.Log($"[MiniMap] WorldPos: {worldPos} → Rotated: {rotated} → MiniMap: ({x}, {y})");

        return new Vector2Int(x, y);
    }



    private void DrawObject(Vector3 worldPos, Color color)
    {
        var texPos = WorldToMiniMap(worldPos);
        int index = texPos.y * textureSize + texPos.x;

        if (index >= 0 && index < pixels.Length)
            pixels[index] = color;
    }

    private void RenderMiniMap()
    {
        ClearMap();

        foreach (var marker in enemyMarkers)
        {
            if (marker == null || !marker.gameObject.activeInHierarchy) continue;
            DrawObject(marker.transform.position, marker.color);
        }

        foreach (var marker in towerAntMarkers)
        {
            if (marker == null || !marker.gameObject.activeInHierarchy) continue;
            DrawObject(marker.transform.position, marker.color);
        }

        foreach (var marker in fastAntMarkers)
        {
            if (marker == null || !marker.gameObject.activeInHierarchy) continue;
            DrawObject(marker.transform.position, marker.color);
        }

        foreach (var marker in spawnAntMarkers)
        {
            if (marker == null || !marker.gameObject.activeInHierarchy) continue;
            DrawObject(marker.transform.position, marker.color);
        }

        ApplyPixels();
    }

}
