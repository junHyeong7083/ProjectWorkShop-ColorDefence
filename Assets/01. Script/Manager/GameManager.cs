using MagicPigGames;
using System;
using UnityEngine;
using UnityEngine.UI;

public enum GameState
{
    NONE = 1,
    STOP = 0
}


public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header("Start Script Controller")]
    [SerializeField] private DeckShuffler deckShuffler;
    [SerializeField] private RTSCameraController rtsCamera;
    [SerializeField] private CardAnimationController cardAnim;


    [Header("UI")]
    [SerializeField] private Text TimerText;
    [SerializeField] private Text costText;

    [Header("Progress (Cost)")]
    [SerializeField] private HorizontalProgressBar progressBar;
    [SerializeField] private float addCostInterval = 1.5f; // 1.5초마다 코스트 0.1 회복

    private const int MAXCOST = 10;
    private float timer = 0f;
    private float lastCostAddTime = 0f;

    [SerializeField] Image endingPanel;
    [SerializeField] Image VictoryImg;
    [SerializeField] Image LoseImg;


    GameState gameState = GameState.NONE;

    public float Timer => timer;
    public GameSpeedMode CurrentSpeed { get; private set; } = GameSpeedMode.Normal;

    private void Awake()
    {
        instance = this;
      //  SetSpeed(CurrentSpeed);

        if (progressBar != null)
            progressBar.SetProgress(0f);


        rtsCamera.enabled = false;
        cardAnim.enabled = false;


        deckShuffler.OnShuffleComplete += () =>
        {
            rtsCamera.enabled = true;
            cardAnim.enabled = true;
        };

        VictoryImg.gameObject.SetActive(false);
        LoseImg.gameObject.SetActive(false);
        endingPanel.gameObject.SetActive(false);
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // UI 업데이트
        UpdateTimerText();


        #region 치트키
        if (Input.GetKeyDown(KeyCode.F1))
        {
            HandleWin();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            HandleLose();
        }
        #endregion


        /*        UpdateCostText();

                // 코스트 자동 회복
                if (Time.time - lastCostAddTime >= addCostInterval)
                {
                    lastCostAddTime = Time.time;
                    AddCost(1); // 0.1 단위 추가
                }*/
    }

    private void HandleWin()
    {
        endingPanel.gameObject.SetActive(true);

        VictoryImg.gameObject.SetActive(true);
        LoseImg.gameObject.SetActive(false);
        Debug.Log("?? 승리 처리 실행됨");

        // 랭킹 등록 예시
        string playerName = PlayerPrefs.GetString("PlayerName", "Unknown");
        int finalScore = Mathf.FloorToInt(timer * 10f); // 점수 예시: 시간 기반
        RankingManager.Instance.RegisterNewResult(playerName, timer, finalScore);
    }

    private void HandleLose()
    {
        endingPanel.gameObject.SetActive(true);

        VictoryImg.gameObject.SetActive(false);
        LoseImg.gameObject.SetActive(true);
        //
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

    public void GameStateChange()
    {
        if (gameState == GameState.NONE)
            gameState = GameState.STOP;
        else
            gameState = GameState.NONE;
        ApplyTimeScale();

    }
    private void ApplyTimeScale()
    {
        switch (gameState)
        {
            case GameState.NONE:
                Time.timeScale = 1f;
                break;
            case GameState.STOP:
                Time.timeScale = 0f;
                break;
        }
    }
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
