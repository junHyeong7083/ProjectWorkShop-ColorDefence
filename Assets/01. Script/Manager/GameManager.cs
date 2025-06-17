using MagicPigGames;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("UI")]
    [SerializeField] private Text TimerText;
    [SerializeField] private Text costText;

    [Header("Progress (Cost)")]
    [SerializeField] private HorizontalProgressBar progressBar;
    [SerializeField] private float addCostInterval = 1.5f; // 1.5초마다 코스트 0.1 회복

    private const int MAXCOST = 10;
    private float timer = 0f;
    private float lastCostAddTime = 0f;

    public float Timer => timer;
    public GameSpeedMode CurrentSpeed { get; private set; } = GameSpeedMode.Normal;

    private void Awake()
    {
        instance = this;
        SetSpeed(CurrentSpeed);

        if (progressBar != null)
            progressBar.SetProgress(0f);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // UI 업데이트
        UpdateTimerText();
        UpdateCostText();

        // 코스트 자동 회복
        if (Time.time - lastCostAddTime >= addCostInterval)
        {
            lastCostAddTime = Time.time;
            AddCost(1); // 0.1 단위 추가
        }
    }

    #region UI
    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(timer / 60f);
        int seconds = Mathf.FloorToInt(timer % 60f);
        TimerText.text = $"{minutes:00}:{seconds:00}";
    }

    private void UpdateCostText()
    {
        if (costText != null)
            costText.text = $"{CurrentCost} / {MAXCOST}";
    }
    #endregion

    #region Speed Control
    private void SetSpeed(GameSpeedMode mode)
    {
        CurrentSpeed = mode;
        Time.timeScale = GameSpeedUtility.GetTimeScale(mode);
    }

    public void IncreaseSpeed()
    {
        if (CurrentSpeed < GameSpeedMode.VeryFast)
        {
            CurrentSpeed++;
            SetSpeed(CurrentSpeed);
        }
    }

    public void DecreaseSpeed()
    {
        if (CurrentSpeed > GameSpeedMode.Normal)
        {
            CurrentSpeed--;
            SetSpeed(CurrentSpeed);
        }
    }
    #endregion

    #region Cost System
    public int CurrentCost => Mathf.FloorToInt(progressBar.Progress * MAXCOST);

    public bool CanUseCost(int cost)
    {
        return CurrentCost >= cost;
    }

    public void UseCost(int cost)
    {
        if (!CanUseCost(cost))
        {
            Debug.LogWarning("Not enough cost!");
            return;
        }

        float costRatio = (float)cost / MAXCOST;
        float nextProgress = Mathf.Clamp01(progressBar.Progress - costRatio);
        progressBar.SetProgress(nextProgress);
    }

    public void AddCost(int unit)
    {
        float ratio = (float)unit / MAXCOST;
        float nextProgress = Mathf.Clamp01(progressBar.Progress + ratio);
        progressBar.SetProgress(nextProgress);
    }
    #endregion
}
