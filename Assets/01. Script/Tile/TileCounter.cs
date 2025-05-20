using UnityEngine;

public class TileCounter : MonoBehaviour
{
    public static TileCounter Instance { get; private set; }

    public int PlayerCount { get; private set; }
    public int EnemyCount { get; private set; }

    void Awake() => Instance = this;

    public void Increment(TileColorState state)
    {
        switch (state)
        {
            case TileColorState.Player: PlayerCount++; break;
            case TileColorState.Enemy: EnemyCount++; break;
        }
    }

    public void Decrement(TileColorState state)
    {
        switch (state)
        {
            case TileColorState.Player: PlayerCount--; break;
            case TileColorState.Enemy: EnemyCount--; break;
        }
    }
}
