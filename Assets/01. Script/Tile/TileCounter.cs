using UnityEngine;
using UnityEngine.UI;

public class TileCounter : MonoBehaviour
{
    public static TileCounter Instance { get; private set; }

    public int PlayerCount { get; private set; }
    public int EnemyCount { get; private set; }
    public int NeutralCount { get; private set; }

    [SerializeField] private Text playerCounter;
    [SerializeField] private Text enemyCounter;
    [SerializeField] private Text neutralCounter;


    [SerializeField] private Image playerCountBar;
    [SerializeField] private Image enemyCountBar;

    private int totalTiles = 0;

    void Awake() => Instance = this;

    public void Init(int total)
    {
        totalTiles = total;
        RefreshUI();

    }

    public void RefreshUI()
    {
        int player = PlayerCount;
        int enemy = EnemyCount;
        int neutral = totalTiles - player - enemy;

        float total = totalTiles > 0 ? (float)totalTiles : 1f;

        float playerRatio = player / total;
        float neutralRatio = neutral / total;
        float enemyRatio = enemy / total;

        playerCounter.text = player.ToString();
        enemyCounter.text = enemy.ToString();
        neutralCounter.text = neutral.ToString();

        playerCountBar.fillAmount = playerRatio;
        enemyCountBar.fillAmount = enemyRatio;
    }

    public void Increment(TileColorState state)
    {
        switch (state)
        {
            case TileColorState.Player: PlayerCount++; break;
            case TileColorState.Enemy: EnemyCount++; break;
        }
        RefreshUI();
    }

    public void Decrement(TileColorState state)
    {
        switch (state)
        {
            case TileColorState.Player: PlayerCount--; break;
            case TileColorState.Enemy: EnemyCount--; break;
        }
        RefreshUI();
    }
}
