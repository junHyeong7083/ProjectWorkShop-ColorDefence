using FischlWorks_FogWar;
using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public enum FogState { Hidden, Explored, Visible }

public interface FogRevealFog
{
    void RevealFog();
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
            RevealAll();
        }
    }

    /// <summary>
    /// �ͷ��� ��ġ�� �� ȣ��.
    /// csFogWar.Instance.AddFogRevealer(...) �� ��ȯ��(�ε���)�� TurretBase.FogRevealerIndex�� ����.
    /// </summary>
    public void RegisterAssetFog(TurretBase turret)
    {
        int viewRangeUnits = Mathf.RoundToInt(turret.viewRange / csFogWar.Instance._UnitScale);
        var revealer = new csFogWar.FogRevealer(
            turret.transform,
            viewRangeUnits,
            false   // �ͷ��� �������̹Ƿ� ������ �߻� �ø� ������ �ʿ� ����
        );

        // fog ����Ʈ�� �߰��ϰ�, �ε����� TurretBase�� ����
        turret.FogRevealerIndex = csFogWar.Instance.AddFogRevealer(revealer);
    }

    /// <summary>
    /// �ͷ��� �Ǹ�(����)�� �� ȣ��.
    /// TurretBase.FogRevealerIndex�� �̿��� csFogWar.Instance.RemoveFogRevealer(index) ȣ��.
    /// </summary>
    public void UnregisterAssetFog(TurretBase turret)
    {
        if (turret.FogRevealerIndex >= 0)
        {
            csFogWar.Instance.RemoveFogRevealer(turret.FogRevealerIndex);
            turret.FogRevealerIndex = -1;
        }
    }

    /// <summary>
    /// Visible -> Explored ��ȯ �� ��� RevealFog ȣ��
    /// </summary>
    public void RevealAll()
    {
        UpdateFog();
        foreach (var r in revealers)
            r.RevealFog();
    }

    public IReadOnlyList<FogRevealFog> GetRevealers() => revealers;
    public void Register(FogRevealFog revealer) => revealers.Add(revealer);
    public void Unregister(FogRevealFog revealer) => revealers.Remove(revealer);

    public void RevealAreaGradient(Vector3 worldPos, float radius)
    {
        var center = MiniMapRenderer.Instance.WorldToMiniMap(worldPos);
        float worldW = TileGridManager.Instance.Width * TileGridManager.Instance.cubeSize;
        float pxPerUnit = MiniMapRenderer.Instance.TextureSize / worldW;
        int rpx = Mathf.CeilToInt(radius * pxPerUnit);
        int texSize = MiniMapRenderer.Instance.TextureSize;

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
                float alpha = Mathf.Pow(1 - (dist / rpx), 0.4f);
                MiniMapRenderer.Instance.MarkVisiblePixel(new Vector2Int(x, y), alpha);
            }
        }
    }

    public void UpdateFog()
    {
        var tiles = TileGridManager.Instance.tiles;
        int w = TileGridManager.Instance.Width;
        int h = TileGridManager.Instance.Height;
        for (int x = 0; x < w; ++x)
            for (int z = 0; z < h; ++z)
            {
                var tile = tiles[x, z];
                if (tile.fogState == FogState.Visible)
                    tile.fogState = FogState.Explored;
            }
    }
}