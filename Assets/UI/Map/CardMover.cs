using UnityEngine;
using UnityEngine.EventSystems;

public class CardMover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Vector3 originalPosition;
    private CardManager cardManager;

    public void Initialize(CardManager manager)
    {
        cardManager = manager;
        originalPosition = transform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 모든 카드 원위치로 복귀
        cardManager.ResetAllCards();

        // 현재 카드만 위로 이동
        transform.localPosition = originalPosition + new Vector3(0, 5, 0);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 마우스가 카드에서 나가면, 카드 원위치로 돌아오게 설정할 수도 있습니다.
        transform.localPosition = originalPosition;
    }
}
