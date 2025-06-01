// CardDragHandler.cs

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(RectTransform))]
public class CardDragHandler : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("드래그 중 확대 배율")]
    public float dragScale = 1.2f;
    [Header("슬롯 복귀 애니메이션 시간")]
    public float returnDuration = 0.2f;
    [Header("사용(소환) 직전 대기 시간")]
    public float useDelay = 0.1f;

    // 슬롯 정보
    private Vector2 originalAnchoredPos;
    private float originalZRotation;
    private Vector3 originalScale;

    private RectTransform rect;
    private Canvas parentCanvas;
    private CanvasGroup canvasGroup;
    private Vector2 pointerOffset;

    [HideInInspector] public CardAnimationController animationController;
    [HideInInspector] public RectTransform topPanel;
    [HideInInspector] public int slotIndex = -1;
    [HideInInspector] public CardData cardData;

    private bool isPreviewActive = false;
    //  LayerMask placementLayer;
    private bool wasDragged = false;
    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
      //  placementLayer = LayerMask.GetMask("PlacementLayer"); // 레이어 설정 필요
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            Debug.LogError($"[{name}] 부모 Canvas를 찾을 수 없습니다.");
    }

    /// <summary>
    /// CardAnimationController에서 슬롯에 배치할 때 호출
    /// 슬롯 정보(위치, 회전)와 CardData를 세팅
    /// </summary>
    public void InitializeSlot(Vector2 anchoredPos, float zRotation, CardData data)
    {
        originalAnchoredPos = anchoredPos;
        originalZRotation = zRotation;
        originalScale = rect.localScale;
        cardData = data;
    }

    //───────────────────────────────────────────────────────────────────────────────────
    // 드래그 시작: 스케일 키우고, 포인터-카드 오프셋 계산, UpdateOverlapStateAndColor 호출
    //───────────────────────────────────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        wasDragged = false;

        rect.DOKill();
        rect.DOScale(originalScale * dragScale, returnDuration * 0.5f).SetEase(Ease.OutBack);

        Vector2 localMousePos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out localMousePos
        );
        pointerOffset = rect.anchoredPosition - localMousePos;
        GetComponent<Button>().interactable = false;
        rect.SetAsLastSibling();
        UpdateOverlapStateAndColor();
    }

    //───────────────────────────────────────────────────────────────────────────────────
    // 드래그 중: 카드 위치 업데이트, UpdateOverlapStateAndColor 호출
    //───────────────────────────────────────────────────────────────────────────────────
    public void OnDrag(PointerEventData eventData)
    {
        wasDragged = true;
        Vector2 localMousePos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                eventData.position,
                parentCanvas.worldCamera,
                out localMousePos))
        {
            rect.anchoredPosition = localMousePos + pointerOffset;
        }
        UpdateOverlapStateAndColor();
    }

    //───────────────────────────────────────────────────────────────────────────────────
    // 드래그 종료: 설치 여부 판단 → 설치(PlaceTurret/PlaceFence) 혹은 복귀
    //───────────────────────────────────────────────────────────────────────────────────
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!wasDragged) return;


        // Debug.Log("Drag End!");

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        bool overHalf = IsOverTopPanelByHalf();

        if (overHalf && PlacementManager.Instance.IsPlacing && PlacementManager.Instance.IsCanPlace)
        {
          //  Debug.Log("ifff");
            // 드래그가 TopPanel 위에 있고, Preview도 설치 가능 상태면 → 설치 시도
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
            //    Debug.Log("ray");
                int width = 0, height = 0;
                if (cardData.scriptable is TurretData tData)
                {
                    width = tData.width;
                    height = tData.height;
                }
                else if (cardData.scriptable is FenceData fData)
                {
                    width = fData.Width;
                    height = fData.Height;
                }
                else
                {
                    ReturnImmediately();
                    return;
                }

                float tileSize = TileGridManager.Instance.cubeSize;
                int startX = Mathf.FloorToInt(hit.point.x / tileSize);
                int startZ = Mathf.FloorToInt(hit.point.z / tileSize);

                Vector3 worldPos = new Vector3(
                    startX * tileSize + (width - 1) * 0.5f * tileSize,
                    0f,
                    startZ * tileSize + (height - 1) * 0.5f * tileSize
                );

                // 설치 수행
                if (cardData.scriptable is TurretData)
                    PlacementManager.Instance.PlaceTurret(startX, startZ, worldPos);
                else
                    PlacementManager.Instance.PlaceFence(startX, startZ, worldPos);

                animationController.UseCardAtIndex_Fan(slotIndex);
                return;
            }
        }

        // 설치 불가 상황 → 복귀 처리
        ReturnImmediately();
    }


    //───────────────────────────────────────────────────────────────────────────────────
    // 드래그 중 실시간으로 “TopPanel 절반 이상 겹쳤는지” 판단
    // • 겹치면 카드 전체 투명 + Preview 생성
    // • 안 겹치면 카드 전체 복원 + Preview 취소
    //───────────────────────────────────────────────────────────────────────────────────
    private void UpdateOverlapStateAndColor()
    {
        if (canvasGroup == null || topPanel == null || cardData == null)
            return;

        bool overHalf = IsOverTopPanelByHalf();

        if (overHalf)
        {
            canvasGroup.alpha = 0f;
            if (!isPreviewActive)
            {
                PlacementType type = (cardData.scriptable is TurretData)
                                        ? PlacementType.Turret
                                        : PlacementType.Fence;

                PlacementManager.Instance.StartPlacement(
                    cardData.scriptable,
                    cardData.prefabToSpawn,
                    type
                );
                isPreviewActive = true;
            }
        }
        else
        {
            canvasGroup.alpha = 1f;
            if (isPreviewActive)
            {
                PlacementManager.Instance.CancelPreview();
                isPreviewActive = false;
            }
        }
    }

    /// <summary>
    /// 카드 UI가 TopPanel과 절반 이상 겹쳤는지 계산
    /// </summary>
    private bool IsOverTopPanelByHalf()
    {
        if (topPanel == null)
            return false;

        // 카드 UI 사각형 (world → screen 좌표)
        Vector3[] cardCorners = new Vector3[4];
        rect.GetWorldCorners(cardCorners);
        Vector2 cardMin = RectTransformUtility.WorldToScreenPoint(null, cardCorners[0]);
        Vector2 cardMax = RectTransformUtility.WorldToScreenPoint(null, cardCorners[2]);
        Rect cardRect = Rect.MinMaxRect(cardMin.x, cardMin.y, cardMax.x, cardMax.y);

        // topPanel 사각형 (world → screen 좌표)
        Vector3[] panelCorners = new Vector3[4];
        topPanel.GetWorldCorners(panelCorners);
        Vector2 panelMin = RectTransformUtility.WorldToScreenPoint(null, panelCorners[0]);
        Vector2 panelMax = RectTransformUtility.WorldToScreenPoint(null, panelCorners[2]);
        Rect panelRect = Rect.MinMaxRect(panelMin.x, panelMin.y, panelMax.x, panelMax.y);

        if (!cardRect.Overlaps(panelRect))
            return false;

        Rect intersection = RectIntersect(cardRect, panelRect);
        float interArea = intersection.width * intersection.height;
        float cardArea = cardRect.width * cardRect.height;

        return interArea >= (cardArea * 0.5f);
    }

    /// <summary>
    /// 두 Rect의 교집합을 계산하여 반환
    /// </summary>
    private Rect RectIntersect(Rect a, Rect b)
    {
        float xMin = Mathf.Max(a.xMin, b.xMin);
        float yMin = Mathf.Max(a.yMin, b.yMin);
        float xMax = Mathf.Min(a.xMax, b.xMax);
        float yMax = Mathf.Min(a.yMax, b.yMax);

        if (xMax < xMin || yMax < yMin)
            return new Rect(0, 0, 0, 0);

        return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
    }

    /// <summary>
    /// “설치 불가” 피드백 (임시로 로그 출력)
    /// 필요하다면 UI 텍스트나 사운드로 확장 가능
    /// </summary>
    private void ShowCannotPlaceFeedback()
    {
        Debug.LogWarning("이 위치에는 설치할 수 없습니다!");
    }

    /// <summary>
    /// 즉시 Preview 취소 후 슬롯 복귀
    /// </summary>
    private void ReturnImmediately()
    {
        //Debug.Log($"[카드 복귀] SlotIndex={slotIndex}, CardName={cardData?.cardName}");

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (isPreviewActive)
        {
            PlacementManager.Instance.CancelPreview();
            isPreviewActive = false;
        }

        ReturnToSlot();
    }

    /// <summary>
    /// 슬롯 복귀 애니메이션: 스케일 복원 → 위치/회전 복원
    /// </summary>
    private void ReturnToSlot()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOScale(originalScale, returnDuration * 0.5f).SetEase(Ease.InBack));
        seq.Append(rect.DOAnchorPos(originalAnchoredPos, returnDuration * 0.5f).SetEase(Ease.OutCubic));
        seq.Join(rect.DOLocalRotate(new Vector3(0f, 0f, originalZRotation), returnDuration * 0.5f).SetEase(Ease.OutCubic));
        GetComponent<Button>().interactable = false; // 클릭 허용
    }
}
