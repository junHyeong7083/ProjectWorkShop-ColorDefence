// DeckManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("덱에 포함될 모든 카드 데이터 목록")]
    [Tooltip("에디터에서 CardData 에셋을 모두 등록하세요.")]
    public List<CardData> allCards;

    private List<CardData> deck;      // 실제로 셔플하고 뽑을 카드 리스트
    private List<CardData> discard;   // 사용된(버린) 카드 보관용 리스트
    private System.Random rng;        // 랜덤 시드 관리

    private void Awake()
    {
        rng = new System.Random();
        InitializeDeck();
    }

    /// <summary>
    /// 게임 시작 또는 덱 리셋 시 호출.
    /// allCards를 복사해서 deck으로 만들고, discard를 클리어한 뒤 셔플한다.
    /// </summary>
    public void InitializeDeck()
    {
        deck = new List<CardData>(allCards);
        discard = new List<CardData>();
        ShuffleDeck();
    }

    /// <summary>
    /// Fisher–Yates 알고리즘으로 deck 리스트를 무작위로 섞는다.
    /// </summary>
    private void ShuffleDeck()
    {
        int n = deck.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int j = rng.Next(i, n);
            CardData tmp = deck[i];
            deck[i] = deck[j];
            deck[j] = tmp;
        }
    }

    /// <summary>
    /// 덱에서 카드 한 장을 뽑아 반환.
    /// 만약 덱이 비어 있으면 discard를 섞어서 재사용한다.
    /// </summary>
    public CardData DrawCardData()
    {
        if (deck == null || deck.Count == 0)
        {
            // 덱이 비어 있으면 discard를 deck으로 재활용
            if (discard == null || discard.Count == 0)
                return null;    // 완전히 뽑을 카드가 없는 상태

            deck = new List<CardData>(discard);
            discard.Clear();
            ShuffleDeck();
        }

        CardData cd = deck[0];
        deck.RemoveAt(0);

        // 사용된 카드를 discard로 보내기
        discard.Add(cd);

        return cd;
    }

    /// <summary>
    /// deck에 남은 카드 수 반환
    /// </summary>
    public int GetDeckCount()
    {
        return deck?.Count ?? 0;
    }

    /// <summary>
    /// discard에 쌓인 카드 수 반환
    /// </summary>
    public int GetDiscardCount()
    {
        return discard?.Count ?? 0;
    }
}
