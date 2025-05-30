using UnityEngine;

public class MiniMapMarker : MonoBehaviour
{
    public enum MarkerType { Enemy, TowerAnt, FastAnt, SpawnAnt, QueenAnt }
    public MarkerType type;
    public Color color = Color.red;

    private void OnEnable()
    {
        if (MiniMapRenderer.Instance == null) return;
      //  Debug.Log($"[MiniMapMarker] OnEnable for {gameObject.name}, Instance={MiniMapRenderer.Instance}");
        switch (type)
        {
            case MarkerType.Enemy:
                MiniMapRenderer.Instance.RegisterEnemy(this);
                break;
            case MarkerType.TowerAnt:
                MiniMapRenderer.Instance.RegisterTowerAnt(this);
                break;
            case MarkerType.FastAnt:
                MiniMapRenderer.Instance.RegisterFastAnt(this);
                break;
            case MarkerType.SpawnAnt:
                MiniMapRenderer.Instance.RegisterSpawnAnt(this);
                break;
            case MarkerType.QueenAnt:
                MiniMapRenderer.Instance.RegisterQueenAnt(this);
                break;

        }
    }

    // 비활성화 -> 항상 자신을 모든 리스트에서 제거
    private void OnDisable()
    {
        if (MiniMapRenderer.Instance == null) return;
        MiniMapRenderer.Instance.Unregister(this);
    }
}