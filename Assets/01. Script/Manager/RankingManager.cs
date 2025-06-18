using System.Collections.Generic;
using UnityEngine;






[System.Serializable]
public class RankEntry
{
    public string playerName;
    public float battleTime;
    public int score;



    public RankEntry(string name, float time, int _score)
    {
        playerName = name;
        battleTime = time;
        score = _score;
    }
}





public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance;

    [Header("1~5등 위치 프리셋")]
    public List<RectTransform> rankSlots;

    [SerializeField] private RankingSlotUI[] rankingSlots; // UI 슬롯 5개

    private List<RankEntry> rankList = new();
    private const int MaxRankCount = 5;
    // tq
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadRanking();
        UpdateRankingUI();
    }

    public void RegisterNewResult(string playerName, float battleTime, int score)
    {
        RankEntry newEntry = new(playerName, battleTime, score);
        rankList.Add(newEntry);

        rankList.Sort((a, b) =>
        {
            int scoreCompare = b.score.CompareTo(a.score);
            return scoreCompare == 0 ? a.battleTime.CompareTo(b.battleTime) : scoreCompare;
        });

        if (rankList.Count > MaxRankCount)
            rankList.RemoveRange(MaxRankCount, rankList.Count - MaxRankCount);

        SaveRanking();
        UpdateRankingUI();
    }

    private void UpdateRankingUI()
    {
        for (int i = 0; i < rankingSlots.Length; i++)
        {
            RectTransform targetTransform = rankSlots[i]; // 고정된 위치
            RectTransform uiTransform = rankingSlots[i].GetComponent<RectTransform>(); // 보여줄 슬롯

            if (i < rankList.Count)
                rankingSlots[i].SetData(i + 1, rankList[i]);
            else
                rankingSlots[i].SetData(i + 1, new RankEntry("-", 0f, 0));

            // 위치, 회전, 스케일 맞춤
            uiTransform.SetParent(targetTransform.parent);
            uiTransform.anchoredPosition = targetTransform.anchoredPosition;
            uiTransform.localRotation = targetTransform.localRotation;
            uiTransform.localScale = targetTransform.localScale;

        
            foreach (RectTransform child in uiTransform)
            {
                Vector2 anchored = child.anchoredPosition;
                anchored.y = 0f;
                child.anchoredPosition = anchored;
            }
        }
    }


    private void SaveRanking()
    {
        for (int i = 0; i < rankList.Count; i++)
        {
            PlayerPrefs.SetString($"Rank_Name_{i}", rankList[i].playerName);
            PlayerPrefs.SetFloat($"Rank_Time_{i}", rankList[i].battleTime);
            PlayerPrefs.SetInt($"Rank_Score_{i}", rankList[i].score);
        }
        PlayerPrefs.SetInt("Rank_Count", rankList.Count);
        PlayerPrefs.Save();
    }

    private void LoadRanking()
    {
        rankList.Clear();
        int count = PlayerPrefs.GetInt("Rank_Count", 0);
        for (int i = 0; i < count; i++)
        {
            string name = PlayerPrefs.GetString($"Rank_Name_{i}", "-");
            float time = PlayerPrefs.GetFloat($"Rank_Time_{i}", 0f);
            int score = PlayerPrefs.GetInt($"Rank_Score_{i}", 0);

            rankList.Add(new RankEntry(name, time, score));
        }
    }
}
