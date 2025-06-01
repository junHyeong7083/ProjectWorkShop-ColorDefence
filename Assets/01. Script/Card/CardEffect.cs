using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 카드 UI에 붙여서, 클릭-드래그 시 카드를 따라다니고,
/// 드래그 해제 시 원래 슬롯 위치로 돌아가며 크기가 커졌다가 작아지는 효과를 줍니다.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CardEffect : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("드래그 중 카드 크기 배율")]
    [Tooltip("드래그를 시작할 때 카드가 커질 배율 (1 = 원래 크기)")]
    public float dragScale = 1.5f;

    [Header("드래그 해제 후 돌아올 애니메이션 시간")]
    [Tooltip("드래그를 놓았을 때 원래 위치로 돌아오는 데 걸리는 시간 (초)")]
    public float returnDuration = 0.2f;

    // private 변수들
    private RectTransform _rect;              // 이 카드의 RectTransform
    private Canvas _parentCanvas;             // 상위 Canvas (위치 변환용)
    private Vector2 _originalAnchoredPos;     // 카드가 가지고 있던 원래 슬롯 위치
    private Vector3 _originalScale;           // 카드의 원래 스케일
    private CanvasGroup _canvasGroup;         // 드래그 중에 Raycast 통과를 막거나 허용하기 위해

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _originalScale = _rect.localScale;
        _originalAnchoredPos = _rect.anchoredPosition;

        // 카드가 속한 최상위 캔버스를 찾는다 (드래그 위치 계산용)
        _parentCanvas = GetComponentInParent<Canvas>();
        if (_parentCanvas == null)
        {
            Debug.LogError($"[{name}] 부모 Canvas 를 찾을 수 없습니다. CardEffect 스크립트가 정상 동작하려면, 카드가 Canvas 하위에 있어야 합니다.");
        }

        // CanvasGroup이 없다면 추가해 둔다. 드래그 중에는 클릭 이벤트가 UI 뒤로 가도록 투명 처리 등에 활용 가능
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// 드래그가 시작될 때 호출됩니다.
    /// 카드 크기를 dragScale 배율만큼 키우고, Raycast 차단을 위해 blocksRaycasts를 false로 설정합니다.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1) 카드 크기 키우기 (드래그 중)
        _rect.DOScale(_originalScale * dragScale, returnDuration * 0.5f)
             .SetEase(Ease.OutBack);

        // 2) 드래그 중에 다른 UI 위로 지나가더라도 클릭을 받기 위해 Raycast 차단
        _canvasGroup.blocksRaycasts = false;

        // 3) 카드가 다른 객체 위에 그려지도록 최상단으로 끌어낸다
        _rect.SetAsLastSibling();
    }

    /// <summary>
    /// 드래그 중 매 프레임 호출됩니다.
    /// 마우스 포인터 위치에 맞춰 카드를 따라 움직이도록 합니다.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (_parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // 간단하게 Screen Space Overlay 모드이면 카드 위치를 포인터 위치로 설정
            _rect.position = eventData.position;
        }
        else
        {
            // Screen Space Camera or World Space 인 경우, RectTransformUtility를 사용하여 local position으로 변환
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _parentCanvas.transform as RectTransform,
                eventData.position,
                _parentCanvas.worldCamera,
                out localPoint);
            _rect.anchoredPosition = localPoint;
        }
    }

    /// <summary>
    /// 드래그가 끝나서 마우스 버튼을 놓으면 호출됩니다.
    /// </original>
    public void OnEndDrag(PointerEventData eventData)
    {
        // 1) Raycast 다시 허용
        _canvasGroup.blocksRaycasts = true;

        // 2) 카드 크기를 원래 스케일로 되돌리고, 슬롯 위치(_originalAnchoredPos)로 애니메이션
        Sequence seq = DOTween.Sequence();

        // (a) 먼저 스케일을 원래 크기로 작게 줄인다
        seq.Append(_rect.DOScale(_originalScale, returnDuration * 0.5f).SetEase(Ease.InBack));

        // (b) 스케일 축소가 끝나면 원래 슬롯 위치로 이동
        seq.Append(_rect.DOAnchorPos(_originalAnchoredPos, returnDuration * 0.5f).SetEase(Ease.OutCubic));
    }

    /// <summary>
    /// (선택) run-time에 프로그램적으로 슬롯 위치를 업데이트하고 싶을 때 호출합니다.
    /// 예를 들어, 외부에서 카드가 밀려서 슬롯이 변경되면 새로운 anchorPos를 전달해 주면 됩니다.
    /// </summary>
    /// <param name="newAnchoredPos">새롭게 적용할 슬롯의 앵커드 포지션</param>
    public void UpdateSlotPosition(Vector2 newAnchoredPos)
    {
        _originalAnchoredPos = newAnchoredPos;
        // 필요하다면 즉시 슬롯 위치를 강제로 맞추고 싶으면:
        // _rect.anchoredPosition = _originalAnchoredPos;
    }
}
