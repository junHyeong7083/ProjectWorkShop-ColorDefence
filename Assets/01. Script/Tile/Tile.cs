using UnityEngine;
using System.Collections;
public enum TileColorState { Neutral, Player, Enemy }

public class Tile : MonoBehaviour
{
    public Vector2Int GridPos { get; private set; }
    public TileColorState ColorState { get; private set; }

    private Renderer rend;
    MaterialPropertyBlock block;

    private void Awake()
    {
        block = new MaterialPropertyBlock();
        rend = GetComponent<Renderer>();
    }
    public void Init(int x, int z)
    {
        GridPos = new Vector2Int(x, z);
        SetColor(TileColorState.Neutral);

    }

    public void AnimateBump()
    {
        StartCoroutine(BumpRoutine());
    }

    IEnumerator BumpRoutine()
    {
        Vector3 origin = transform.position;
        float time = 0f;
        float duration = 0.2f;
        float height = 0.2f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * height; // 부드럽게 올렸다가 내림
            transform.position = origin + new Vector3(0, yOffset, 0);
            yield return null;
        }

        transform.position = origin;
    }
    public void SetColor(TileColorState newColor)
    {
        if (ColorState == newColor) return;
        AnimateBump();
        ColorState = newColor;

        Color c = GetColorFromState(newColor);

        rend.GetPropertyBlock(block);
        block.SetColor("_BaseColor", c);
        rend.SetPropertyBlock(block);
    }

    Color GetColorFromState(TileColorState state)
    {
        switch (state)
        {
            case TileColorState.Player: return Color.blue;
            case TileColorState.Enemy: return Color.red;
            case TileColorState.Neutral: return Color.gray;
            default: return Color.black;
        }
    }
}
