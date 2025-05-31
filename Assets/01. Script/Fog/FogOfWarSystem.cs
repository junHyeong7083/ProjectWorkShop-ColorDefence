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
    /// 터렛을 설치할 때 호출.
    /// csFogWar.Instance.AddFogRevealer(...) 의 반환값(인덱스)을 TurretBase.FogRevealerIndex에 저장.
    /// </summary>
    public void RegisterAssetFog(TurretBase turret)
    {
        int viewRangeUnits = Mathf.RoundToInt(turret.viewRange / csFogWar.Instance._UnitScale);
        var revealer = new csFogWar.FogRevealer(
            turret.transform,
            viewRangeUnits,
            false   // 터렛은 고정형이므로 움직임 발생 시만 갱신할 필요 없음
        );

        // fog 리스트에 추가하고, 인덱스를 TurretBase에 저장
        turret.FogRevealerIndex = csFogWar.Instance.AddFogRevealer(revealer);
    }

    /// <summary>
    /// 터렛을 판매(제거)할 때 호출.
    /// TurretBase.FogRevealerIndex를 이용해 csFogWar.Instance.RemoveFogRevealer(index) 호출.
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
    /// Visible -> Explored 전환 후 모두 RevealFog 호출
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