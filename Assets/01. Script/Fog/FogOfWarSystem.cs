using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;
public enum FogState { Hidden, Explored, Visible }

public interface FogRevealFog
{
    public void RevealFog();
}

public class FogOfWarSystem : MonoBehaviour
{
    public static FogOfWarSystem Instance;
    private readonly List<FogRevealFog> revealers = new();
    private float updateInterval = 0.1f;
    private float timer = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer > updateInterval) 
        {
            timer = 0f;

            UpdateFog(); // ���� visible => explored�� ����
            foreach (var revealer in revealers)
                revealer.RevealFog();
        }
        
    }


    public void Register(FogRevealFog revealer) => revealers.Add(revealer);
    public void Unregister(FogRevealFog revealer) => revealers.Remove(revealer);


    /// <summary>
    /// UpdateFog() �� ��� Revealer.RevealFog() ȣ���� �� ���� ����
    /// </summary>
    public void RevealAll()
    {
        UpdateFog();            // ���� Visible �� Explored ó��
        foreach (var r in revealers)
            r.RevealFog();      // �� TurretBase.RevealFog() �� RevealAreaGradient()
    }

    public void RevealAreaGradient(Vector3 worldPos, float radius)
    {
        // 1) �ͷ��� �̴ϸʻ� �ȼ� ��ǥ ���ϱ�
        var center = MiniMapRenderer.Instance.WorldToMiniMap(worldPos);

        // 2) ���� ���� �� �ȼ� ���� ���� ���
        float worldW = TileGridManager.Instance.Width * TileGridManager.Instance.cubeSize;
        float pxPerUnit = MiniMapRenderer.Instance.TextureSize / worldW;
        int rpx = Mathf.CeilToInt(radius * pxPerUnit);

        int texSize = MiniMapRenderer.Instance.TextureSize;

        // 3) �ȼ� �ݰ游ŭ ���鼭 �� ���� �׶��̼�
        for (int dx = -rpx; dx <= rpx; dx++)
        {
            int x = center.x + dx;
            if (x < 0 || x >= texSize) continue;

            for (int dy = -rpx; dy <= rpx; dy++)
            {
                int y = center.y + dy;
                if (y < 0 || y >= texSize) continue;

                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > rpx) continue;

                // 0~1�� ����ȭ
                float t = dist / rpx;
                float alpha = Mathf.Pow(1 - t, 0.4f);

                MiniMapRenderer.Instance.MarkVisiblePixel(new Vector2Int(x, y), alpha);
            }
        }
    }


    /// <summary>
    /// ��� Ÿ�� ��ȸ �� visible -> explored�� ��ȯ (�ѹ��� ������ ������ �Ⱥ������� ����)
    /// </summary>
    public void UpdateFog()
    {
        var tiles = TileGridManager.Instance.tiles;
        for(int x = 0; x< TileGridManager.Instance.Width; ++x)
        {
            for(int z = 0; z < TileGridManager.Instance.Height; ++z)
            {
                var tile = tiles[x, z];
                if(tile.fogState == FogState.Visible)
                    tile.fogState = FogState.Explored;
            }
        }
    }
  

}
