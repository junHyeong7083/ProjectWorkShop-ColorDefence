using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements.Experimental;

public class MiniMapRenderer : MonoBehaviour
{
    public static MiniMapRenderer Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Transform rtsCameraTransform;
    [SerializeField] private RawImage miniMapImage;

    [Header("Settings")]
    [SerializeField] private int textureSize = 256;
    [SerializeField] private float renderInterval = 0.1f;

    // �ܺ� ���ٿ�
    public int TextureSize => textureSize;

    private Texture2D texture;
    private Color32[] pixels;
    private float[] alphaBuffer;
    private bool[] exploredBuffer;    // �� ���̶� ��(Explored) �ȼ�
    
    private float timer = 0f;

    // ��Ŀ ����Ʈ
    private readonly List<MiniMapMarker> enemyMarkers = new();
    private readonly List<MiniMapMarker> towerAntMarkers = new();
    private readonly List<MiniMapMarker> fastAntMarkers = new();
    private readonly List<MiniMapMarker> spawnAntMarkers = new();
    private readonly List<MiniMapMarker> queenAntMarkers = new();


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
        // �ؽ�ó �� ���� �ʱ�ȭ
        texture = new Texture2D(textureSize, textureSize)
        {
            filterMode = FilterMode.Point
        };
        pixels = new Color32[textureSize * textureSize];
        alphaBuffer = new float[textureSize * textureSize];
        exploredBuffer = new bool[textureSize * textureSize];

        miniMapImage.texture = texture;
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

    #region Registration
    public void RegisterEnemy(MiniMapMarker m)
    {
        if (!enemyMarkers.Contains(m)) enemyMarkers.Add(m);
    }
    public void RegisterTowerAnt(MiniMapMarker m)
    {
        if (!towerAntMarkers.Contains(m)) towerAntMarkers.Add(m);
    }
    public void RegisterFastAnt(MiniMapMarker m)
    {
        if (!fastAntMarkers.Contains(m)) fastAntMarkers.Add(m);
    }
    public void RegisterSpawnAnt(MiniMapMarker m)
    {
        if (!spawnAntMarkers.Contains(m)) spawnAntMarkers.Add(m);
    }
    public void RegisterQueenAnt(MiniMapMarker m)
    {
        if (!queenAntMarkers.Contains(m)) queenAntMarkers.Add(m);
    }
    public void Unregister(MiniMapMarker m)
    {
        enemyMarkers.Remove(m);
        towerAntMarkers.Remove(m);
        fastAntMarkers.Remove(m);
        spawnAntMarkers.Remove(m);
        queenAntMarkers.Remove(m);
    }
    #endregion

    /// <summary>
    /// FogOfWarSystem.RevealAreaGradient() �κ��� ȣ��Ǿ�
    /// �ش� �ȼ��� �׶��̼� ���ĸ� �ִ밪���� �����մϴ�.
    /// </summary>
    public void MarkVisiblePixel(Vector2Int tex, float alpha)
    {
       // Debug.Log($"[Fog] Mark {tex} ��={alpha:F2}");
        int idx = tex.y * textureSize + tex.x;
        if (idx >= 0 && idx < alphaBuffer.Length)
            alphaBuffer[idx] = Mathf.Max(alphaBuffer[idx], alpha);

        if (alpha > 0f)
            exploredBuffer[idx] = true;
    }

    /// <summary>
    /// �ؽ�ó ��ǥ �� ���� ��ǥ ��ȯ (FogOfWarSystem ���)
    /// </summary>
    public Vector3 MiniMapToWorld(Vector2Int tex)
    {
        float worldW = TileGridManager.Instance.Width * TileGridManager.Instance.cubeSize;
        float worldH = TileGridManager.Instance.Height * TileGridManager.Instance.cubeSize;

        // [0..textureSize] �� [-worldW/2..+worldW/2]
        float xr = (tex.x + 0.5f) / textureSize * worldW - (worldW * 0.5f);
        float zr = (tex.y + 0.5f) / textureSize * worldH - (worldH * 0.5f);

        // ī�޶� ȸ�� ����
        Vector3 local = rtsCameraTransform.rotation * new Vector3(xr, 0f, zr);

        // �� ���� ��ǥ�� ������
        return local + new Vector3(worldW * 0.5f, 0f, worldH * 0.5f);
    }

    private void ClearMap()
    {
        int len = pixels.Length;
        for (int i = 0; i < len; i++)
        {
            pixels[i] = Color.black;
            alphaBuffer[i] = 0f;
        }
    }

    private void ApplyPixels()
    {
        texture.SetPixels32(pixels);
        texture.Apply();
    }

    private void RenderMiniMap()
    {
        // 1) �ʱ�ȭ: ���� Ŭ����
        ClearMap();

        // 2) �Ȱ� ���� ���� ȣ��
        FogOfWarSystem.Instance.RevealAll();

        // 3) alphaBuffer �� pixels ����
        for (int i = 0; i < pixels.Length; i++)
        {
            if (alphaBuffer[i] > 0f)
            {
                // Visible: �׶��̼� ȭ��Ʈ
                byte v = (byte)(Mathf.Clamp01(alphaBuffer[i]) * 255);
                pixels[i] = new Color32(v, v, v, 255);
            }
            else if (exploredBuffer[i])
            {
                // Explored(�� �� �� ����): £�� ȸ��
                pixels[i] = new Color32(30, 30, 30, 255);
            }
            else
            {
                // Hidden: ���� ����(�⺻)
                pixels[i] = Color.black;
            }
        }


        // 4) ��Ŀ(��/�ͷ� ��) ��������
        foreach (var m in enemyMarkers)
        {
            if (m == null || !m.gameObject.activeInHierarchy)
                continue;

            //  ���� �� �̴ϸ� �ȼ� ��ǥ
            var mini = WorldToMiniMap(m.transform.position);
            int idx = mini.y * textureSize + mini.x;

            //  alphaBuffer[idx] > 0 �̸� visible!
            if (alphaBuffer[idx] > 0f)
                DrawObject(m.transform.position, m.color);
        }
        foreach (var m in towerAntMarkers)
            if (m != null && m.gameObject.activeInHierarchy)
                DrawObject(m.transform.position, m.color);
        foreach (var m in fastAntMarkers)
            if (m != null && m.gameObject.activeInHierarchy)
                DrawObject(m.transform.position, m.color);
        foreach (var m in spawnAntMarkers)
            if (m != null && m.gameObject.activeInHierarchy)
                DrawObject(m.transform.position, m.color);
        foreach (var m in queenAntMarkers)
            if (m != null && m.gameObject.activeInHierarchy)
                DrawObject(m.transform.position, m.color);

        // 5) �ؽ�ó ����
        ApplyPixels();
    }

    private void DrawObject(Vector3 worldPos, Color color)
    {
        Vector2Int mini = WorldToMiniMap(worldPos);
        int idx = mini.y * textureSize + mini.x;
        if (idx >= 0 && idx < pixels.Length)
            pixels[idx] = color;
    }

    public Vector2Int WorldToMiniMap(Vector3 worldPos)
    {
        float worldWidth = TileGridManager.Instance.Width * TileGridManager.Instance.cubeSize;
        float worldHeight = TileGridManager.Instance.Height * TileGridManager.Instance.cubeSize;

        Vector3 centerOffset = new Vector3(worldWidth, 0f, worldHeight) * 0.5f;
        Vector3 localPos = worldPos - centerOffset;

        Vector3 rotated = Quaternion.Inverse(rtsCameraTransform.rotation) * localPos;

        float xRatio = (rotated.x + worldWidth / 2f) / worldWidth;
        float yRatio = (rotated.z + worldHeight / 2f) / worldHeight;

        int x = Mathf.Clamp((int)(xRatio * textureSize), 0, textureSize - 1);
        int y = Mathf.Clamp((int)(yRatio * textureSize), 0, textureSize - 1);

        return new Vector2Int(x, y);
    }
}