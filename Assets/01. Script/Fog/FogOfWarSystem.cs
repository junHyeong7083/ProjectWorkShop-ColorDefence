using UnityEngine;

public class FogOfWarSystem : MonoBehaviour
{
    public static FogOfWarSystem Instance;

    public enum FogState {  Unexplored, Explored, Visible }

    [Header("¼³Á¤")]
    public int textureSize = 256;
    public float worldSize = 64f;

    private FogState[] fogStates;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        fogStates = new FogState[textureSize * textureSize];
    }

    public void RevealArea(Vector3 worldPos, float radius)
    {
        Vector2Int center = WorldToMiniMap(worldPos);
        int pixelRadius = Mathf.CeilToInt((radius / worldSize) * textureSize);
        Debug.Log($"center : {center}, pixelRadius : {pixelRadius}");
        for (int y = -pixelRadius; y <= pixelRadius; y++)
        {
            for (int x = -pixelRadius; x <= pixelRadius; x++)
            {
                int px = center.x + x;
                int py = center.y + y;

                if (px < 0 || py < 0 || px >= textureSize || py >= textureSize)
                    continue;

                float dist = new Vector2(x, y).magnitude;
                if (dist > pixelRadius) continue;

                int index = py * textureSize + px;
                fogStates[index] = FogState.Visible;
            }
        }
    }

    public void DowngradeVisibleToExplored()
    {
        for (int i = 0; i < fogStates.Length; i++)
        {
            if (fogStates[i] == FogState.Visible)
                fogStates[i] = FogState.Explored;
        }
    }

    public FogState[] GetFogStates()
    {
        return fogStates;
    }

    public FogState GetStateAt(Vector2Int pos)
    {
        int index = pos.y * textureSize + pos.x;
        if (index < 0 || index >= fogStates.Length)
            return FogState.Unexplored;
        return fogStates[index];
    }

    private Vector2Int WorldToMiniMap(Vector3 worldPos)
    {
        float halfSize = worldSize * 0.5f;
        float xRatio = (worldPos.x + halfSize) / worldSize;
        float yRatio = (worldPos.z + halfSize) / worldSize;

        int x = Mathf.Clamp((int)(xRatio * textureSize), 0, textureSize - 1);
        int y = Mathf.Clamp((int)(yRatio * textureSize), 0, textureSize - 1);

        return new Vector2Int(x, y);
    }
}
