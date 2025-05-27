using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TileColorState { Neutral, Player, Enemy }

public class Tile : MonoBehaviour
{
    public Vector2Int GridPos { get; private set; }
    public TileColorState ColorState { get; private set; }

    public float LastChangedTime { get; private set; } = -999f;
    public Vector3 CenterWorldPos => new Vector3(
        GridPos.x * TileGridManager.Instance.cubeSize,
        0,
        GridPos.y * TileGridManager.Instance.cubeSize
    );

    public bool IsOccupied { get; set; }
    public bool IsReserved { get; private set; } = false;
    public bool IsBumping => isBumping;
    public TurretBase TargetingTurret { get; set; } = null;

    private Renderer rend;
    private MaterialPropertyBlock block;
    private bool isBumping = false;

    private bool isPreviewing = false;
    private Color? originalColor = null;

    private void Awake()
    {
        block = new MaterialPropertyBlock();
        rend = GetComponent<Renderer>();
    }

    public void Init(int x, int z)
    {
        GridPos = new Vector2Int(x, z);
        SetColor(TileColorState.Neutral);
        IsOccupied = false;
    }

    public void SetColor(TileColorState newColor)
    {
        if (ColorState == newColor || isBumping) return;

        if (isPreviewing)
            RevertPreviewColor(); // ������ ���̸� �����ϰ� ���� �ݿ�

        Release();

        TileCounter.Instance.Decrement(ColorState);
        ColorState = newColor;
        TileCounter.Instance.Increment(newColor);

        LastChangedTime = Time.time;
        AnimateBump();

        Color c = GetColorFromState(newColor);
        rend.GetPropertyBlock(block);

        block.Clear();
        block.SetColor("_BaseColor", c);
        rend.SetPropertyBlock(block);
    }

    public void AnimateBump()
    {
        if (isBumping) return;
        StartCoroutine(BumpRoutine());
    }

    private IEnumerator BumpRoutine()
    {
        isBumping = true;
        Vector3 origin = transform.position;
        float time = 0f;
        float duration = 0.3f;
        float height = TileGridManager.Instance.cubeSize * 0.6f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float progress = time / duration;
            float yOffset = Mathf.Sin(progress * Mathf.PI) * height;
            transform.position = origin + new Vector3(0, yOffset, 0);
            yield return null;
        }

        transform.position = origin;
        isBumping = false;
    }

    public void Reserve()
    {
        IsReserved = true;
    }

    public void Release()
    {
        IsReserved = false;
    }

    private Color GetColorFromState(TileColorState state)
    {
        return state switch
        {
            TileColorState.Player => Color.blue,
            TileColorState.Enemy => Color.red,
            TileColorState.Neutral => Color.gray,
            _ => Color.black
        };
    }

    public void SetPreviewColor(Color previewColor)
    {
        if (!isPreviewing)
        {
            originalColor = rend.sharedMaterial.GetColor("_BaseColor");
            isPreviewing = true;
        }

        block.Clear();
        block.SetColor("_BaseColor", previewColor);
        rend.SetPropertyBlock(block);
    }

    public void RevertPreviewColor()
    {
        if (!isPreviewing || originalColor == null) return;

        rend.GetPropertyBlock(block);
        block.Clear();
        block.SetColor("_BaseColor", originalColor.Value);
        rend.SetPropertyBlock(block);

        isPreviewing = false;
        originalColor = null;
    }
}
