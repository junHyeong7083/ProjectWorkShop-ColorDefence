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
    private bool wasDragged = false;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 1f;

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
            Debug.LogError($"[{name}] 부모 Canvas를 찾을 수 없습니다.");
    }

    public void InitializeSlot(Vector2 anchoredPos, float zRotation, CardData data)
    {
        originalAnchoredPos = anchoredPos;
        originalZRotation = zRotation;
        originalScale = rect.localScale;
        cardData = data;
    }

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

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!wasDragged) return;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        bool overHalf = IsOverTopPanelByHalf();

        if (overHalf)
        {
            Ray ray = Camera.main.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                switch (cardData.cardType)
                {
                    case CardType.TURRET:
                    case CardType.FENCE:
                        HandlePlaceableCard(hit);
                        return;

                    case CardType.UPGRADE:
                        HandleUpgradeCard(hit);
                        return;
                    case CardType.UNIT:
                        HandlePlaceableCard(hit);
                        break;
                    default:
                        ReturnImmediately();
                        return;
                }
            }
        }

        ReturnImmediately();
    }

    // 타워 및 소환(설치가능한)유닛들
    private void HandlePlaceableCard(RaycastHit hit)
    {
        int width = 0, height = 0;

        // 타입별 width, height 설정
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
            // UNIT은 기본 1x1 타일로 가정
            width = 1;
            height = 1;
        }

        float tileSize = TileGridManager.Instance.cubeSize;
        int startX = Mathf.FloorToInt(hit.point.x / tileSize);
        int startZ = Mathf.FloorToInt(hit.point.z / tileSize);

        Vector3 worldPos = new Vector3(
            startX * tileSize + (width - 1) * 0.5f * tileSize,
            0f,
            startZ * tileSize + (height - 1) * 0.5f * tileSize
        );

        if (!PlacementManager.Instance.IsCanPlace)
        {
            ShowCannotPlaceFeedback();
            ReturnImmediately();
            return;
        }

        switch (cardData.cardType)
        {
            case CardType.TURRET:
                PlacementManager.Instance.PlaceTurret(startX, startZ, worldPos);
                break;


            case CardType.UNIT: 
                PlacementManager.Instance.PlaceUnit(startX, startZ, worldPos);
                break;
            case CardType.FENCE:
                PlacementManager.Instance.PlaceFence(startX, startZ, worldPos);
                break;
            default:
                ReturnImmediately();
                return;
        }

        animationController.UseCardAndReposition(slotIndex);
        PlacementManager.Instance.CancelPreview();
    }




    private void HandleUpgradeCard(RaycastHit hit)
    {
        var turret = hit.collider.GetComponent<TurretBase>();
        if (turret == null)
        {
          //  Debug.LogWarning("업그레이드 실패: 터렛이 아닙니다.");
            ReturnImmediately();
            return;
        }

        turret.Upgrade();
        Debug.Log($"업그레이드 완료: {turret.name} → Lv.{turret.CurrentLevel}");

        animationController.UseCardAndReposition(slotIndex);

        PlacementManager.Instance.CancelPreview(); 
        isPreviewActive = false;
    }

    private void UpdateOverlapStateAndColor()
    {
        if (canvasGroup == null || topPanel == null || cardData == null)
            return;

        bool overHalf = IsOverTopPanelByHalf();

        // 기본값
        canvasGroup.alpha = 1f;

        if (!overHalf)
        {
            if (isPreviewActive)
            {
                PlacementManager.Instance.CancelPreview();
                isPreviewActive = false;
            }
            return;
        }

        // TopPanel 절반 이상 겹침

        switch (cardData.cardType)
        {
            case CardType.UNIT:
            case CardType.TURRET:
            case CardType.FENCE:
                canvasGroup.alpha = 0f;
                if (!isPreviewActive)
                {
                    PlacementManager.Instance.StartPlacement(
                        cardData.scriptable,
                        cardData.prefabToSpawn,
                        cardData.cardType == CardType.TURRET ? PlacementType.Turret : PlacementType.Fence
                    );
                    isPreviewActive = true;
                }
                break;

            case CardType.UPGRADE:
                canvasGroup.alpha = 0.5f;

                Transform back = transform.Find("Back");
                if(back != null)
                {
                    Image backImage = back.GetComponent<Image>();
                    Color c = backImage.color;
                    c.a = 0f;
                    backImage.color = c;
                }


                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    var turret = hit.collider.GetComponent<TurretBase>();
                    if (turret != null)
                    {
                        canvasGroup.alpha = 0f;

                        if (!isPreviewActive)
                        {
                            PlacementManager.Instance.StartPlacement(
                                cardData.scriptable,
                                cardData.prefabToSpawn,
                                PlacementType.Upgrade
                            );
                            isPreviewActive = true;
                        }
                    }
                    else
                    {
                        if (isPreviewActive)
                        {
                            PlacementManager.Instance.CancelPreview();
                            isPreviewActive = false;
                        }
                    }
                }
                else
                {
                    if (isPreviewActive)
                    {
                        PlacementManager.Instance.CancelPreview();
                        isPreviewActive = false;
                    }
                }
                break;

        }
    }

    private bool IsOverTopPanelByHalf()
    {
        if (topPanel == null)
            return false;

        Vector3[] cardCorners = new Vector3[4];
        rect.GetWorldCorners(cardCorners);
        Vector2 cardMin = RectTransformUtility.WorldToScreenPoint(null, cardCorners[0]);
        Vector2 cardMax = RectTransformUtility.WorldToScreenPoint(null, cardCorners[2]);
        Rect cardRect = Rect.MinMaxRect(cardMin.x, cardMin.y, cardMax.x, cardMax.y);

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

    private void ShowCannotPlaceFeedback()
    {
        Debug.LogWarning("이 위치에는 설치할 수 없습니다!");
    }

    private void ReturnImmediately()
    {
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        if (isPreviewActive)
        {
            PlacementManager.Instance.CancelPreview();
            isPreviewActive = false;
        }

        ReturnToSlot();
    }

    private void ReturnToSlot()
    {
        Sequence seq = DOTween.Sequence();
        seq.Append(rect.DOScale(originalScale, returnDuration * 0.5f).SetEase(Ease.InBack));
        seq.Append(rect.DOAnchorPos(originalAnchoredPos, returnDuration * 0.5f).SetEase(Ease.OutCubic));
        seq.Join(rect.DOLocalRotate(new Vector3(0f, 0f, originalZRotation), returnDuration * 0.5f).SetEase(Ease.OutCubic));
        GetComponent<Button>().interactable = false;
    }
}
