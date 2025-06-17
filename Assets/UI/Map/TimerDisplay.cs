using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    public TMP_Text timerText;      // Inspector에서 TMP 텍스트 오브젝트 연결
    public float timeLimit = 3600f; // 예: 60분(3600초)로 시작
    private float timeRemaining;

    void Start()
    {
        timeRemaining = timeLimit;
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0) timeRemaining = 0;
        }

        int minutes = (int)timeRemaining / 60;
        int seconds = (int)timeRemaining % 60;
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        // 또는 C# 6.0 이상에서는
        // timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
