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

            UpdateFog(); // 기존 visible => explored로 변경
            foreach (var revealer in revealers)
                revealer.RevealFog();
        }
        
    }


    public void Register(FogRevealFog revealer) => revealers.Add(revealer);
    public void Unregister(FogRevealFog revealer) => revealers.Remove(revealer);


    /// <summary>
    /// UpdateFog() → 모든 Revealer.RevealFog() 호출을 한 번에 수행
    /// </summary>
    public void RevealAll()
    {
        UpdateFog();            // 이전 Visible → Explored 처리
        foreach (var r in revealers)
            r.RevealFog();      // 각 TurretBase.RevealFog() → RevealAreaGradient()
    }

    public void RevealAreaGradient(Vector3 worldPos, float radius)
    {
        // 1) 터렛의 미니맵상 픽셀 좌표 구하기
        var center = MiniMapRenderer.Instance.WorldToMiniMap(worldPos);

        // 2) 세계 단위 → 픽셀 단위 비율 계산
        float worldW = TileGridManager.Instance.Width * TileGridManager.Instance.cubeSize;
        float pxPerUnit = MiniMapRenderer.Instance.TextureSize / worldW;
        int rpx = Mathf.CeilToInt(radius * pxPerUnit);

        int texSize = MiniMapRenderer.Instance.TextureSize;

        // 3) 픽셀 반경만큼 돌면서 원 형태 그라데이션
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

                // 0~1로 정규화
                float t = dist / rpx;
                float alpha = Mathf.Pow(1 - t, 0.4f);

                MiniMapRenderer.Instance.MarkVisiblePixel(new Vector2Int(x, y), alpha);
            }
        }
    }


    /// <summary>
    /// 모든 타일 순회 및 visible -> explored로 전환 (한번은 봤지만 지금은 안보임으로 세팅)
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
