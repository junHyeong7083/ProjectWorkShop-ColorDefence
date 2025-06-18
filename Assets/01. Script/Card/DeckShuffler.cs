using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class DeckShuffler : MonoBehaviour
{
    [SerializeField] private RectTransform deckParent;
    [SerializeField] private RectTransform shuffleStartPoint;
    [SerializeField] private RectTransform shuffleEndPoint;
    [SerializeField] private float shuffleDuration = 0.15f;
    [SerializeField] private float shuffleOffsetY = 80f;

    [SerializeField] GameObject PlayerBase;
 
    private Dictionary<RectTransform, Vector2> originalPositions = new();
    private Dictionary<RectTransform, Quaternion> originalRotations = new();


    public System.Action OnShuffleComplete;
    private void Start()
    {
        StartShuffle();
    }

    public void StartShuffle()
    {
        StartCoroutine(FullShuffleRoutine());
    }

    IEnumerator FullShuffleRoutine()
    {
        // 1. 덱을 시작 위치(중앙)으로 이동
        deckParent.position = shuffleStartPoint.position;

        // 2. 카드 초기화 및 위치/회전 저장
        List<RectTransform> cards = new();
        originalPositions.Clear();
        originalRotations.Clear();

        foreach (Transform child in deckParent)
        {
            RectTransform rt = child as RectTransform;
            if (rt != null)
            {
                cards.Add(rt);
                rt.anchoredPosition = Vector2.zero;
                originalPositions[rt] = rt.anchoredPosition;
                originalRotations[rt] = rt.localRotation;
            }
        }
/*
        // 3. Cut 셔플 3회
        for (int i = 0; i < 5; i++)
            yield return StartCoroutine(CutShuffle(cards));*/

        // 4. Riffle 셔플 2회
        for (int i = 0; i < 4; i++)
            yield return StartCoroutine(RiffleShuffle(cards));

        // 5. 덱 최종 위치로 이동
        yield return YieldCache.WaitForSeconds(0.3f);
        deckParent.DOMove(shuffleEndPoint.position, 0.5f).SetEase(Ease.InOutCubic);

        yield return YieldCache.WaitForSeconds(0.5f);
        PlayerBase.gameObject.SetActive(true);
        OnShuffleComplete?.Invoke();
    }

    IEnumerator CutShuffle(List<RectTransform> cards)
    {
        int start = cards.Count / 3;
        int count = cards.Count / 3;
        var cut = cards.GetRange(start, count);

        foreach (var card in cut)
        {
            card.DOAnchorPos(originalPositions[card] + new Vector2(0, shuffleOffsetY), shuffleDuration)
                .SetEase(Ease.OutSine);
        }

        yield return new WaitForSeconds(shuffleDuration);

        foreach (var card in cut)
        {
            card.SetSiblingIndex(0);
        }

        yield return YieldCache.WaitForSeconds(0.1f);
            
        // 덱처럼 쌓기 정렬
        ApplyStackedLayout(cards);
        yield return YieldCache.WaitForSeconds(shuffleDuration);
    }

    IEnumerator RiffleShuffle(List<RectTransform> cards)
    {
        int half = cards.Count / 2;
        var topHalf = cards.GetRange(0, half);
        var bottomHalf = cards.GetRange(half, cards.Count - half);

        int i = 0, j = 0;
        List<RectTransform> shuffled = new();
        SoundManager.Instance.PlaySFXSound("shuffleSFX", 0.3f);
        while (i < topHalf.Count || j < bottomHalf.Count)
        {
            if (i < topHalf.Count)
            {
                var card = topHalf[i];
                shuffled.Add(card);
                card.DOAnchorPos(originalPositions[card] + new Vector2(Random.Range(-10f, 10f), shuffleOffsetY), shuffleDuration)
                    .SetEase(Ease.OutSine);
                i++;
                yield return new WaitForSeconds(shuffleDuration * 0.5f);
            }

            if (j < bottomHalf.Count)
            {
                var card = bottomHalf[j];
                shuffled.Add(card);
                card.DOAnchorPos(originalPositions[card] + new Vector2(Random.Range(-10f, 10f), -shuffleOffsetY), shuffleDuration)
                    .SetEase(Ease.OutSine);
                j++;
                yield return YieldCache. WaitForSeconds(shuffleDuration * 0.5f);
            }
        }

        yield return YieldCache.WaitForSeconds(shuffleDuration);

        // 덱처럼 쌓기 정렬
        ApplyStackedLayout(cards);
        yield return YieldCache.WaitForSeconds(0.25f);
    }

    // 🧩 카드들을 덱처럼 층층이 정렬 + 회전 복원
    void ApplyStackedLayout(List<RectTransform> cards)
    {
        float offsetStep = 2f; // Y 간격
        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];

            // ✅ anchoredPosition 기준으로 PosY만 위로 2씩 추가
            Vector2 targetPos = originalPositions[card] + new Vector2(0, i * offsetStep);

            card.SetSiblingIndex(i);
            card.DOAnchorPos(targetPos, 0.2f).SetEase(Ease.InOutCubic);

            card.DORotateQuaternion(originalRotations[card], 0.2f).SetEase(Ease.InOutCubic);
        }
        SoundManager.Instance.StopSFXSound("shuffleSFX");
    
    }


}
