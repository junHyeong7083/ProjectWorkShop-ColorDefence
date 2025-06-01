using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// ī�� UI�� �ٿ���, Ŭ��-�巡�� �� ī�带 ����ٴϰ�,
/// �巡�� ���� �� ���� ���� ��ġ�� ���ư��� ũ�Ⱑ Ŀ���ٰ� �۾����� ȿ���� �ݴϴ�.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CardEffect : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [Header("�巡�� �� ī�� ũ�� ����")]
    [Tooltip("�巡�׸� ������ �� ī�尡 Ŀ�� ���� (1 = ���� ũ��)")]
    public float dragScale = 1.5f;

    [Header("�巡�� ���� �� ���ƿ� �ִϸ��̼� �ð�")]
    [Tooltip("�巡�׸� ������ �� ���� ��ġ�� ���ƿ��� �� �ɸ��� �ð� (��)")]
    public float returnDuration = 0.2f;

    // private ������
    private RectTransform _rect;              // �� ī���� RectTransform
    private Canvas _parentCanvas;             // ���� Canvas (��ġ ��ȯ��)
    private Vector2 _originalAnchoredPos;     // ī�尡 ������ �ִ� ���� ���� ��ġ
    private Vector3 _originalScale;           // ī���� ���� ������
    private CanvasGroup _canvasGroup;         // �巡�� �߿� Raycast ����� ���ų� ����ϱ� ����

    private void Awake()
    {
        _rect = GetComponent<RectTransform>();
        _originalScale = _rect.localScale;
        _originalAnchoredPos = _rect.anchoredPosition;

        // ī�尡 ���� �ֻ��� ĵ������ ã�´� (�巡�� ��ġ ����)
        _parentCanvas = GetComponentInParent<Canvas>();
        if (_parentCanvas == null)
        {
            Debug.LogError($"[{name}] �θ� Canvas �� ã�� �� �����ϴ�. CardEffect ��ũ��Ʈ�� ���� �����Ϸ���, ī�尡 Canvas ������ �־�� �մϴ�.");
        }

        // CanvasGroup�� ���ٸ� �߰��� �д�. �巡�� �߿��� Ŭ�� �̺�Ʈ�� UI �ڷ� ������ ���� ó�� � Ȱ�� ����
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
        {
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    /// <summary>
    /// �巡�װ� ���۵� �� ȣ��˴ϴ�.
    /// ī�� ũ�⸦ dragScale ������ŭ Ű���, Raycast ������ ���� blocksRaycasts�� false�� �����մϴ�.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1) ī�� ũ�� Ű��� (�巡�� ��)
        _rect.DOScale(_originalScale * dragScale, returnDuration * 0.5f)
             .SetEase(Ease.OutBack);

        // 2) �巡�� �߿� �ٸ� UI ���� ���������� Ŭ���� �ޱ� ���� Raycast ����
        _canvasGroup.blocksRaycasts = false;

        // 3) ī�尡 �ٸ� ��ü ���� �׷������� �ֻ������ �����
        _rect.SetAsLastSibling();
    }

    /// <summary>
    /// �巡�� �� �� ������ ȣ��˴ϴ�.
    /// ���콺 ������ ��ġ�� ���� ī�带 ���� �����̵��� �մϴ�.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (_parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // �����ϰ� Screen Space Overlay ����̸� ī�� ��ġ�� ������ ��ġ�� ����
            _rect.position = eventData.position;
        }
        else
        {
            // Screen Space Camera or World Space �� ���, RectTransformUtility�� ����Ͽ� local position���� ��ȯ
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
    /// �巡�װ� ������ ���콺 ��ư�� ������ ȣ��˴ϴ�.
    /// </original>
    public void OnEndDrag(PointerEventData eventData)
    {
        // 1) Raycast �ٽ� ���
        _canvasGroup.blocksRaycasts = true;

        // 2) ī�� ũ�⸦ ���� �����Ϸ� �ǵ�����, ���� ��ġ(_originalAnchoredPos)�� �ִϸ��̼�
        Sequence seq = DOTween.Sequence();

        // (a) ���� �������� ���� ũ��� �۰� ���δ�
        seq.Append(_rect.DOScale(_originalScale, returnDuration * 0.5f).SetEase(Ease.InBack));

        // (b) ������ ��Ұ� ������ ���� ���� ��ġ�� �̵�
        seq.Append(_rect.DOAnchorPos(_originalAnchoredPos, returnDuration * 0.5f).SetEase(Ease.OutCubic));
    }

    /// <summary>
    /// (����) run-time�� ���α׷������� ���� ��ġ�� ������Ʈ�ϰ� ���� �� ȣ���մϴ�.
    /// ���� ���, �ܺο��� ī�尡 �з��� ������ ����Ǹ� ���ο� anchorPos�� ������ �ָ� �˴ϴ�.
    /// </summary>
    /// <param name="newAnchoredPos">���Ӱ� ������ ������ ��Ŀ�� ������</param>
    public void UpdateSlotPosition(Vector2 newAnchoredPos)
    {
        _originalAnchoredPos = newAnchoredPos;
        // �ʿ��ϴٸ� ��� ���� ��ġ�� ������ ���߰� ������:
        // _rect.anchoredPosition = _originalAnchoredPos;
    }
}
