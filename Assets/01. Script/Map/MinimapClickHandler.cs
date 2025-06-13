using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinimapClickHandler : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [SerializeField] private RenderTexture minimapTexture;
    [SerializeField] private Camera minimapCam;
    [SerializeField] private Camera mainCam;
    [SerializeField] private float moveHeight = 20;
    [SerializeField] private LineRenderer minimaplineRenderer;
    [SerializeField] private float lineDuration = 1.5f;

    private RawImage rawImage;
    private RectTransform rectTransform;
    private bool isDragging = false;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();
        rawImage.texture = minimapTexture;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, null, out Vector2 localPoint))
            return;

        float normalizedX = (localPoint.x / rectTransform.rect.width) + 0.5f;
        float normalizedY = (localPoint.y / rectTransform.rect.height) + 0.5f;

        float orthoHeight = minimapCam.orthographicSize;
        float orthoWidth = orthoHeight * minimapCam.aspect;

        Vector3 camCenter = minimapCam.transform.position;
        float worldX = camCenter.x + (normalizedX - 0.5f) * orthoWidth * 2f;
        float worldZ = camCenter.z + (normalizedY - 0.5f) * orthoHeight * 2f;

        // ↓ 바닥 감지를 위한 레이캐스트
        Vector3 rayOrigin = new Vector3(worldX, 100f, worldZ); // 위에서 아래로 쏨
        Ray ray = new Ray(rayOrigin, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, LayerMask.GetMask("placementLayer")))
        {
            Vector3 targetWorldPos = hit.point;

            if (eventData.button == PointerEventData.InputButton.Right)
            {
             //   Debug.Log("🖱 우클릭 → 유닛 이동");
              //  Debug.Log($"이동할 월드 좌표 : {targetWorldPos}");
                AntSelectionManager.Instance.IssueMoveCommand(targetWorldPos);
            }
            else if (eventData.button == PointerEventData.InputButton.Left)
            {
             //   Debug.Log("🖱 좌클릭 → 카메라 이동 시작");
                isDragging = true;
                RTSCameraController.instance.TeleportTo(new Vector3(
                    targetWorldPos.x,
                    RTSCameraController.instance.transform.position.y,
                    targetWorldPos.z
                ));
            }
        }
        else
        {
          //  Debug.LogWarning("❌ 바닥 레이어에서 감지 실패 (레이캐스트 실패)");
        }
    }


    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, null, out Vector2 localPoint))
            return;

        float normalizedX = (localPoint.x / rectTransform.rect.width) + 0.5f;
        float normalizedY = (localPoint.y / rectTransform.rect.height) + 0.5f;

        float orthoHeight = minimapCam.orthographicSize;
        float orthoWidth = orthoHeight * minimapCam.aspect;

        Vector3 camCenter = minimapCam.transform.position;
        float worldX = camCenter.x + (normalizedX - 0.5f) * orthoWidth * 2f;
        float worldZ = camCenter.z + (normalizedY - 0.5f) * orthoHeight * 2f;

        RTSCameraController.instance.TeleportTo(new Vector3(worldX, RTSCameraController.instance.transform.position.y, worldZ));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            isDragging = false;
    }
}
