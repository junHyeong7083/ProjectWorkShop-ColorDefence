using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardAnimationController : MonoBehaviour
{
    [Header("카드 배치(덱→손) 위치")]
    public RectTransform deckPoint;           // 덱 위치
    [Header("Hand 슬롯 5개")]
    public RectTransform[] handSlots;         // 크기 5
    [Header("카드 Prefab")]
    public GameObject cardPrefab;             // 내부에 CardBinderUI, Button, Image, Text들 세팅済

    [Header("애니메이션 설정")]
    public float moveDuration = 0.4f;
    public float initialPopupScale = 1.2f;
    public float popupBackDuration = 0.15f;

    [Header("덱 매니저")]
    public DeckManager deckManager;           // CardData를 한 장씩 뽑아오는 역할

    [Header("카드 드래그하여 소환할 영역")]
    public RectTransform topPanel;            // Placement 용 UI 영역

    private List<GameObject> handCards = new List<GameObject>();
    private readonly float[] slotZRotations = new float[5] { 15f, 5f, 0f, -5f, -15f };

    private void Start()
    {
        // 게임 시작 시 0.05초 간격으로 5장 드로우
        StartCoroutine(DrawInitialFiveCards_Fan());
    }

    private IEnumerator DrawInitialFiveCards_Fan()
    {
        for (int i = 0; i < 5; i++)
        {
            CardData cd = deckManager.DrawCardData();
            if (cd != null)
            {
                yield return StartCoroutine(AnimateDrawCardToSlot_Fan(cd, i));
            }
            else
            {
                Debug.LogWarning($"덱에 남은 카드 없음: {i + 1}번째 드로우 실패");
            }

            yield return new WaitForSeconds(0.05f);
        }
    }

    /// <summary>
    /// 덱에서 한 장 드로우하여, Hand의 슬롯 targetIndex 위치로 애니메이션으로 이동시키고,
    /// 동시에 CardBinderUI를 통해 UI(아이콘, 이름, 비용, 설명)를 바인딩합니다.
    /// </summary>
    private IEnumerator AnimateDrawCardToSlot_Fan(CardData cd, int targetIndex)
    {
        if (handCards.Count >= 5)
        {
            Debug.Log($"current Count : {handCards.Count}");
            yield break;
        }

        // 1) 카드 Prefab 인스턴스 생성
        GameObject newCard = Instantiate(cardPrefab, deckPoint.parent);
        newCard.tag = "Card";
        RectTransform rt = newCard.GetComponent<RectTransform>();

        // 2) 카드를 덱 위치에서 시작하도록 세팅
        rt.anchoredPosition = deckPoint.anchoredPosition;
        rt.localScale = Vector3.zero;
        rt.localRotation = Quaternion.Euler(0f, 0f, 0f);

        // 3) CardBinderUI를 이용해 “CardData -> UI(아이콘, 이름, 비용, 설명)”을 한 번에 세팅
        var binder = newCard.GetComponent<CardBinderUI>();
        if (binder != null)
        {
            // 3-1) 카드에 CardData 할당
            binder.cardData = cd;
            // 3-2) Bind() 호출 → 내부에 구현한 대로 아이콘/이름/비용/설명 세팅
            binder.Bind();
        }
        else
        {
            Debug.LogWarning("CardPrefab에 CardBinderUI 컴포넌트가 없습니다! UI 바인딩이 되지 않습니다.");
        }

        // 4) 슬롯 위치/회전값 가져오기
        RectTransform targetSlot = handSlots[targetIndex];
        float targetZ = slotZRotations[targetIndex];

        // 5) DOTween 애니메이션:  
        //     (a) 크기 팝업 → (b) 슬롯으로 이동 + 회전 → (c) 다시 팝업 → 크기 복원
        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOScale(initialPopupScale, moveDuration * 0.4f).SetEase(Ease.OutBack));
        seq.Append(rt.DOAnchorPos(targetSlot.anchoredPosition, moveDuration * 0.6f).SetEase(Ease.OutCubic));
        seq.Join(rt.DOScale(1f, moveDuration * 0.6f).SetEase(Ease.InBack));
        seq.Join(rt.DOLocalRotate(new Vector3(0f, 0f, targetZ), moveDuration * 0.6f).SetEase(Ease.OutCubic));
        seq.Append(rt.DOScale(initialPopupScale, popupBackDuration).SetEase(Ease.OutQuad));
        seq.Append(rt.DOScale(1f, popupBackDuration).SetEase(Ease.InQuad));

        bool sequenceDone = false;
        seq.OnComplete(() =>
        {
            sequenceDone = true;
            handCards.Add(newCard);

            // 6) CardDragHandler 초기화: 슬롯 정보(위치, 회전), CardData 전달
            var dragHandler = newCard.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.animationController = this;
                dragHandler.topPanel = topPanel;
                dragHandler.slotIndex = targetIndex;
                dragHandler.InitializeSlot(
                    targetSlot.anchoredPosition,
                    slotZRotations[targetIndex],
                    cd
                );
            }

            // 7) (선택) 버튼 클릭으로도 카드 사용 가능하도록 이벤트 연결
            Button btn = newCard.GetComponent<Button>();
            if (btn != null)
            {
                int capturedIndex = targetIndex;
                btn.onClick.AddListener(() => UseCardAtIndex_Fan(capturedIndex));
            }
        });

        while (!sequenceDone)
        {
            Debug.Log("sequenceDone");
            yield return null;
        }
        Debug.Log("sequenceEnd");
    }

    /// <summary>
    /// 슬롯 i에 있는 카드를 사용(삭제)하고, 남은 카드들을 왼쪽으로 이동(회전 갱신),
    /// 빈 슬롯에 새로운 카드 드로우 → CardBinderUI로 UI 세팅 → 애니메이션.
    /// </summary>
    public void UseCardAtIndex_Fan(int i)
    {
        Debug.Log($"[카드 사용] {i}번 슬롯 카드 사용 시도");
        if (i < 0 || i >= handCards.Count)
        {
            Debug.LogWarning($"[카드 사용 실패] 유효하지 않은 인덱스 {i}");
            return;
        }

        // 1) 해당 카드 삭제
        GameObject usedCard = handCards[i];
        // 1) 파괴하기 전에, DOTween에서 남아있는 모든 트윈을 제거
        //    (GetComponent<RectTransform>() 대신 Tween을 건 대상 컴포넌트를 사용해도 됩니다)
        RectTransform rt = usedCard.GetComponent<RectTransform>();
        if (rt != null)
        {
            // 현재 rt에 걸려 있는 모든 트윈을 즉시 제거합니다.
            rt.DOKill();
        }

        Destroy(usedCard);
        handCards.RemoveAt(i);

        // 2) 나머지 카드들 왼쪽으로 한 칸씩 이동 (회전값도 재설정)
        float shiftDuration = moveDuration * 0.5f;
        for (int j = i; j < handCards.Count; j++)
        {
             rt = handCards[j].GetComponent<RectTransform>();
            Vector2 newPos = handSlots[j].anchoredPosition;
            float newZ = slotZRotations[j];

            rt.DOAnchorPos(newPos, shiftDuration).SetEase(Ease.OutCubic);
            rt.DOLocalRotate(new Vector3(0f, 0f, newZ), shiftDuration).SetEase(Ease.OutCubic);

            // 동시에 CardDragHandler 슬롯 정보도 갱신
            var dragHandler = handCards[j].GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.slotIndex = j;
                dragHandler.InitializeSlot(newPos, newZ, dragHandler.cardData);
            }
        }

        // 3) 빈 슬롯 인덱스 구함 (0~handCards.Count-1까지 카드가 채워져 있으므로, 빈 슬롯 = handCards.Count)
        int newIndex = handCards.Count;

        if (newIndex < 5)
            StartCoroutine(AnimateDrawCardAtUse_Fan(newIndex));
    }



    /// <summary>
    /// UseCardAtIndex_Fan()로 인해 빈 슬롯(newIndex)이 생기면 덱에서 카드 한 장 드로우하여 슬롯에 배치합니다.
    /// </summary>
    private IEnumerator AnimateDrawCardAtUse_Fan(int newIndex)
    {
        CardData cd = deckManager.DrawCardData();
        if (cd == null) yield break;

        GameObject newCard = Instantiate(cardPrefab, deckPoint.parent);
        newCard.tag = "Card";
        RectTransform rt = newCard.GetComponent<RectTransform>();

        rt.anchoredPosition = deckPoint.anchoredPosition;
        rt.localScale = Vector3.zero;
        rt.localRotation = Quaternion.Euler(0f, 0f, 0f);

        //  CardBinderUI를 이용해 UI 세팅
        var binder = newCard.GetComponent<CardBinderUI>();
        if (binder != null)
        {
            binder.cardData = cd;
            binder.Bind();
        }
        else
        {
            Debug.LogWarning("CardPrefab에 CardBinderUI 컴포넌트가 없습니다!");
        }

        RectTransform targetSlot = handSlots[newIndex];
        float targetZ = slotZRotations[newIndex];

        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOScale(initialPopupScale, moveDuration * 0.4f).SetEase(Ease.OutBack));
        seq.Append(rt.DOAnchorPos(targetSlot.anchoredPosition, moveDuration * 0.6f).SetEase(Ease.OutCubic));
        seq.Join(rt.DOScale(1f, moveDuration * 0.6f).SetEase(Ease.InBack));
        seq.Join(rt.DOLocalRotate(new Vector3(0f, 0f, targetZ), moveDuration * 0.6f).SetEase(Ease.OutCubic));
        seq.Append(rt.DOScale(initialPopupScale, popupBackDuration).SetEase(Ease.OutQuad));
        seq.Append(rt.DOScale(1f, popupBackDuration).SetEase(Ease.InQuad));

        bool done = false;
        seq.OnComplete(() =>
        {
            done = true;
            handCards.Add(newCard);

            var dragHandler = newCard.GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.animationController = this;
                dragHandler.topPanel = topPanel;
                dragHandler.slotIndex = newIndex;
                dragHandler.InitializeSlot(
                    targetSlot.anchoredPosition,
                    slotZRotations[newIndex],
                    cd
                );
            }

            Button btn = newCard.GetComponent<Button>();
            if (btn != null)
            {
                int capturedIndex = newIndex;
                btn.onClick.AddListener(() => UseCardAtIndex_Fan(capturedIndex));
            }
        });

        while (!done)
        {
            Debug.Log("done");
            yield return null;
        }
        Debug.Log("doneEnd");
    }

    /// <summary>
    /// Hand에 있는 모든 카드를 삭제하고 리스트 초기화 (예: 게임 리셋 시)
    /// </summary>
    public void ResetHand_Fan()
    {
        foreach (Transform child in deckPoint.parent)
        {
            if (child.CompareTag("Card"))
                Destroy(child.gameObject);
        }
        handCards.Clear();
    }
}
