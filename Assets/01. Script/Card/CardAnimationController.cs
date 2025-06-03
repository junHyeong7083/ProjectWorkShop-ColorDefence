using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardAnimationController : MonoBehaviour
{
    [Header("카드 배치(덱→손) 위치")]
    public RectTransform deckPoint;

    [Header("Hand 슬롯 5개")]
    public RectTransform[] handSlots;

    [Header("카드 Prefab")]
    public GameObject cardPrefab;

    [Header("애니메이션 설정")]
    public float moveDuration = 0.4f;
    public float initialPopupScale = 1.2f;
    public float popupBackDuration = 0.15f;

    [Header("덱 매니저")]
    public DeckManager deckManager;

    [Header("카드 드래그하여 소환할 영역")]
    public RectTransform topPanel;

    private List<GameObject> handCards = new List<GameObject>();
    private readonly float[] slotZRotations = new float[5] { 15f, 5f, 0f, -5f, -15f };

    [Header("실제 카드 덱")]
    public RectTransform deckParent;
    private Stack<GameObject> deckVisualStack = new Stack<GameObject>();

    private void Start()
    {
        InitializeDeckVisual();
        StartCoroutine(DrawInitialFiveCards_Fan());
    }

    private void InitializeDeckVisual()
    {
        deckVisualStack.Clear();
        for (int i = 0; i < deckParent.childCount; i++)
            deckVisualStack.Push(deckParent.GetChild(i).gameObject);
    }

    private bool DeckNeedsRefill()
    {
        return deckVisualStack.Count == 0;
    }

    private IEnumerator DrawInitialFiveCards_Fan()
    {
        for (int i = 0; i < 5; i++)
        {
            yield return DrawCardSafely(i);
            yield return YieldCache.WaitForSeconds(0.05f);
        }
    }

    private IEnumerator DrawCardSafely(int targetIndex)
    {
        CardData cd = deckManager.DrawCardData();
        if (cd == null) yield break;

        yield return StartCoroutine(AnimateDrawCardToSlot_Fan(cd, targetIndex));
     
        if (DeckNeedsRefill())
        {
            yield return StartCoroutine(AnimateDeckRefill());
        }
    }

    private IEnumerator AnimateDrawCardToSlot_Fan(CardData cd, int targetIndex)
    {
        if (handCards.Count >= 5) yield break;

        RectTransform targetSlot = handSlots[targetIndex];
        float targetZ = slotZRotations[targetIndex];

        yield return AnimateDeckCardDraw(targetSlot, targetZ);

        GameObject newCard = Instantiate(cardPrefab, deckPoint.parent);
        RectTransform rt = newCard.GetComponent<RectTransform>();

        rt.anchoredPosition = targetSlot.anchoredPosition;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.Euler(0, 0, targetZ);

        var binder = newCard.GetComponent<CardBinderUI>();
        if (binder != null)
        {
            binder.cardData = cd;
            binder.Bind();
        }

        var dragHandler = newCard.GetComponent<CardDragHandler>();
        if (dragHandler != null)
        {
            dragHandler.animationController = this;
            dragHandler.topPanel = topPanel;
            dragHandler.slotIndex = targetIndex;
            dragHandler.InitializeSlot(targetSlot.anchoredPosition, targetZ, cd);
        }

        handCards.Add(newCard);
    }

    private IEnumerator AnimateDeckCardDraw(RectTransform targetSlot, float targetZ)
    {
        if (deckVisualStack.Count == 0) yield break;

        GameObject topCard = deckVisualStack.Pop();
        RectTransform rt = topCard.GetComponent<RectTransform>();
       // rt.SetParent(targetSlot.parent, false);
        // 카드 보이게 만들고 위치 초기화
        rt.gameObject.SetActive(true);

        // 병렬 애니메이션 처리
        Sequence seq = DOTween.Sequence();
        Vector3 worldTargetPos = targetSlot.position;

        seq.Append(rt.DOScale(initialPopupScale, moveDuration * 0.3f).SetEase(Ease.OutBack));
        seq.Join(rt.DOMove(worldTargetPos, moveDuration).SetEase(Ease.OutCubic));
        seq.Join(rt.DOScale(1f, moveDuration).SetEase(Ease.InBack));
        seq.Join(rt.DOLocalRotate(new Vector3(0f, 0f, targetZ), moveDuration).SetEase(Ease.OutCubic));

        // 팝업 효과 병렬 처리 (선택 사항)
        seq.Join(rt.DOScale(initialPopupScale, popupBackDuration).SetEase(Ease.OutQuad).SetDelay(moveDuration));
        seq.Join(rt.DOScale(1f, popupBackDuration).SetEase(Ease.InQuad).SetDelay(moveDuration + popupBackDuration));

        bool sequenceDone = false;
        seq.OnComplete(() => sequenceDone = true);

        while (!sequenceDone)
            yield return null;

        // 덱 시각 카드 숨김
        rt.gameObject.SetActive(false);
    }


    public void UseCardAtIndex_Fan(int i)
    {
        if (i < 0 || i >= handCards.Count) return;

        GameObject usedCard = handCards[i];
        RectTransform rt = usedCard.GetComponent<RectTransform>();
        rt?.DOKill();

        Destroy(usedCard);
        handCards.RemoveAt(i);

        float shiftDuration = moveDuration * 0.5f;
        for (int j = i; j < handCards.Count; j++)
        {
            rt = handCards[j].GetComponent<RectTransform>();
            Vector2 newPos = handSlots[j].anchoredPosition;
            float newZ = slotZRotations[j];

            rt.DOAnchorPos(newPos, shiftDuration).SetEase(Ease.OutCubic);
            rt.DOLocalRotate(new Vector3(0f, 0f, newZ), shiftDuration).SetEase(Ease.OutCubic);

            var dragHandler = handCards[j].GetComponent<CardDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.slotIndex = j;
                dragHandler.InitializeSlot(newPos, newZ, dragHandler.cardData);
            }
        }

        int newIndex = handCards.Count;
        if (newIndex < 5)
            StartCoroutine(DrawCardSafely(newIndex));
    }

    public IEnumerator AnimateDeckRefill()
    {
        Transform[] allCards = new Transform[deckParent.childCount];

        for (int i = 0; i < deckParent.childCount; i++)
            allCards[i] = deckParent.GetChild(i);

        Debug.Log("Deck Refill 시작");

        for (int i = 0; i < allCards.Length; i++)
        {
            Transform card = allCards[i];
            RectTransform rt = card.GetComponent<RectTransform>();

            card.gameObject.SetActive(true);
            rt.SetParent(deckParent, false);

            float y = i * 10f;
            rt.localPosition = new Vector3(0f, y, 0f);
            rt.localScale = Vector3.one * 0.8f;
            rt.localRotation = Quaternion.Euler(-45f, 0f, -30f);

            Sequence seq = DOTween.Sequence();
            seq.Append(rt.DOScale(1f, 0.2f));
            seq.Join(rt.DOLocalRotate(new Vector3(-45f, 0f, -30f), 0.2f));

            yield return YieldCache.WaitForSeconds(0.05f);
        }

        InitializeDeckVisual();
        deckManager.InitializeDeck();
    }
}
