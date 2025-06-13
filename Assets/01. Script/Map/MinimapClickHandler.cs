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

    private void MoveCameraToPointer(PointerEventData eventData)
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

        Vector3 camPos = mainCam.transform.position;
        camPos.x = worldX;
        camPos.z = worldZ;
        mainCam.transform.position = camPos;

        Vector3 currentCamPos = mainCam.transform.position;
        RTSCameraController.instance.TeleportTo(new Vector3(worldX, RTSCameraController.instance.transform.position.y, worldZ));

    }
    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        MoveCameraToPointer(eventData);
    }
  
    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
            MoveCameraToPointer(eventData);
    }

}
