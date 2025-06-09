using UnityEngine;

[DisallowMultipleComponent]
public class MiniMapVisibilityMarker : MonoBehaviour
{
    [Header("마커 색상")]
    public Color markerColor = Color.red;

    [Header("마커 높이")]
    public float markerHeight = 5f;

    [SerializeField] private GameObject markerQuad;
    private TileData currentTile;

    private Vector2Int lastGridPos = new Vector2Int(-999, -999);

    private void Update()
    {
        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
      /*  if (TileGridManager.Instance == null || markerQuad == null)
            return;

        Vector3 groundPos = new Vector3(transform.position.x, 0f, transform.position.z);
        Vector2Int gridPos = TileGridManager.GetGridPositionFromWorld(groundPos);

        // 동일 위치면 스킵 (성능 최적화)
        if (gridPos == lastGridPos && currentTile != null)
            return;

        currentTile = TileGridManager.Instance.GetTile(gridPos.x, gridPos.y);
        lastGridPos = gridPos;

        if (currentTile == null)
        {
            if (markerQuad.activeSelf) markerQuad.SetActive(false);
            return;
        }

        bool isVisible = currentTile.fogState == FogState.Visible;
        Debug.Log(isVisible);

        if (markerQuad.activeSelf != isVisible)
            markerQuad.SetActive(isVisible);*/
    }
}
