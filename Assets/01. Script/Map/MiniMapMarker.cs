using UnityEngine;

public class MiniMapMarker : MonoBehaviour
{
    public enum MarkerType { Enemy, TowerAnt, FastAnt, SpawnAnt  }
    public MarkerType type;
    public Color color = Color.red;

    private void OnEnable()
    {
        if (MiniMapRenderer.Instance == null) return;

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

        }
    }

    private void OnDisable()
    {
        if (MiniMapRenderer.Instance == null) return;
        MiniMapRenderer.Instance.Unregister(this);
    }
}
