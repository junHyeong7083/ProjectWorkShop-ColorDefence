using UnityEngine;
using UnityEngine.UI;

public class RankingSlotUI : MonoBehaviour
{
    public Text timeText;
    public Text scoreText;
    public Text playerText;

    public void SetData(int rank, RankEntry entry)
    {
        timeText.text = $"{entry.battleTime:F1}s";
        scoreText.text = entry.score.ToString();
        playerText.text = entry.playerName;
    }
}
