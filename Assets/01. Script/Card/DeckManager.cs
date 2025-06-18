using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    [Header("덱에 포함될 모든 카드 데이터 목록")]
    [Tooltip("에디터에서 CardData 에셋을 모두 등록하세요.")]
    public List<CardData> allCards;

    private List<CardData> deck;      // 실제 셔플 덱
    private List<CardData> discard;   // 버린 카드
    private System.Random rng;        // 랜덤

    [SerializeField] private Text cardCount;

    private void Awake()
    {
        rng = new System.Random();
        InitializeDeck();
    }

    public void InitializeDeck()
    {
        deck = new List<CardData>(allCards);
        discard = new List<CardData>();
        ShuffleDeck();
        UpdateCardCountText(); // 처음에도 텍스트 갱신
    }

    private void ShuffleDeck()
    {
        int n = deck.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int j = rng.Next(i, n);
            var temp = deck[i];
            deck[i] = deck[j];
            deck[j] = temp;
        }
    }

    public CardData DrawCardData()
    {
        if (deck == null || deck.Count == 0)
            return null;

        CardData cd = deck[0];
        deck.RemoveAt(0);
        discard.Add(cd);
        UpdateCardCountText();
        return cd;
    }

    private void UpdateCardCountText()
    {
        int remaining = deck?.Count ?? 0;
        int total = allCards?.Count ?? 0;
        cardCount.text = $"{remaining} / {total}";
    }

    public int GetDeckCount() => deck?.Count ?? 0;
    public int GetDiscardCount() => discard?.Count ?? 0;
}
