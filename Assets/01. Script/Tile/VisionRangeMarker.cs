using UnityEngine;
using System.Collections.Generic;

public class VisionRangeMarker : MonoBehaviour
{
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private float minAlpha = 0.1f;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            RevealTilesInRange();
        }
    }

    private void RevealTilesInRange()
    {
        float radius = Mathf.Max(transform.lossyScale.x, transform.lossyScale.z) * 0.5f;
        Vector3 center = transform.position;

        List<Vector2Int> tiles = TileGridManager.Instance.GetTilesInRange(center, radius);
        foreach (Vector2Int gridPos in tiles)
        {
            TileData tile = TileGridManager.Instance.GetTile(gridPos.x, gridPos.y);
            if (tile != null)
                tile.ColorState = TileColorState.Player;
        }
    }

}
