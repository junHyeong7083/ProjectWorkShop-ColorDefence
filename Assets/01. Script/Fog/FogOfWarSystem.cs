using FischlWorks_FogWar;
using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEngine;

public enum FogState { Hidden, Explored, Visible }


public interface IFogRevealer
{
    void RevealFog();
    Transform transform { get; }
    float viewRange { get; }
    int FogRevealerIndex { get; set; }
}


public class FogOfWarSystem : MonoBehaviour
{
    public static FogOfWarSystem Instance;
    private readonly List<IFogRevealer> revealers = new();
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

    public void RegisterAssetFog(IFogRevealer revealer)
    {
        int viewRangeUnits = Mathf.RoundToInt(revealer.viewRange / csFogWar.Instance._UnitScale);
        var fogRevealer = new csFogWar.FogRevealer(
            revealer.transform,
            viewRangeUnits,
            false // 고정형
        );

        revealer.FogRevealerIndex = csFogWar.Instance.AddFogRevealer(fogRevealer);
    }

    public void UnregisterAssetFog(IFogRevealer revealer)
    {
        if (revealer.FogRevealerIndex >= 0)
        {
            csFogWar.Instance.RemoveFogRevealer(revealer.FogRevealerIndex);
            revealer.FogRevealerIndex = -1;
        }
    }

    /// <summary>
    /// Visible -> Explored 전환 후 모두 RevealFog 호출
    /// </summary>
    public void RevealAll()
    {
        UpdateFog();
      //  Debug.Log("RevealAll 실행됨");
        // Shadowcaster 내부 FogField 정보 기반으로 TileData.fogState를 업데이트
        var fogField = csFogWar.Instance.shadowcaster.fogField;
        var tiles = TileGridManager.Instance.tiles;
        int w = TileGridManager.Instance.Width;
        int h = TileGridManager.Instance.Height;

        for (int x = 0; x < w; x++)
        {
            for (int z = 0; z < h; z++)
            {
                var tile = tiles[x, z];
                var visibility = fogField[x][z]; // Shadowcaster 내부 데이터

                tile.fogState = visibility switch
                {
                    Shadowcaster.LevelColumn.ETileVisibility.Hidden => FogState.Hidden,
                    Shadowcaster.LevelColumn.ETileVisibility.PreviouslyRevealed => FogState.Explored,
                    Shadowcaster.LevelColumn.ETileVisibility.Revealed => FogState.Visible,
                    _ => FogState.Hidden
                };



            }
        }

        foreach (var r in revealers)
            r.RevealFog();
    }


    public IReadOnlyList<IFogRevealer> GetRevealers() => revealers;
    public void Register(IFogRevealer revealer) => revealers.Add(revealer);
    public void Unregister(IFogRevealer revealer) => revealers.Remove(revealer);

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