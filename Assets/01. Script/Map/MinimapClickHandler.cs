using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinimapClickHandler : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private RenderTexture minimapTexture;
    [SerializeField] private Camera minimapCam;
    [SerializeField] private Camera mainCam;
    [SerializeField] private float moveHeight = 20;
    [SerializeField] private LineRenderer minimaplineRenderer;
    [SerializeField] private float lineDuration = 1.5f;

    private RawImage rawImage;
    private RectTransform rectTransform;

    void Start()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        rawImage.texture = minimapTexture;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, null, out localPoint))
        {
            float normalizedX = (localPoint.x / rectTransform.rect.width) + 0.5f;
            float normalizedY = (localPoint.y / rectTransform.rect.height) + 0.5f;

            float orthoHeight = minimapCam.orthographicSize;
            float orthoWidth = orthoHeight * minimapCam.aspect;

            Vector3 camCenter = minimapCam.transform.position;
            float worldX = camCenter.x + (normalizedX - 0.5f) * orthoWidth * 2f;
            float worldZ = camCenter.z + (normalizedY - 0.5f) * orthoHeight * 2f;

            // 레이캐스트로 y=0 NavMesh 보정
            Vector3 rayOrigin = new Vector3(worldX, 100f, worldZ);
            Vector3 worldTarget = new Vector3(worldX, moveHeight, worldZ); // 기본값

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 200f, LayerMask.GetMask("placementLayer")))
            {
                worldTarget = hit.point;
            }
            else
            {
                Debug.LogWarning($"❌ 바닥(Raycast) 감지 실패: {rayOrigin}");
                return;
            }
            Vector3 start = GetSelectedUnitsCenter(); // 중심 좌표 계산
            DrawClickLine(start, worldTarget);
            MoveSelectedUnitsTo(worldTarget);

            MoveSelectedUnitsTo(worldTarget);
        }
    }
    private void DrawClickLine(Vector3 start, Vector3 end)
    {
        if (minimaplineRenderer == null) return;

        minimaplineRenderer.gameObject.SetActive(true);
        minimaplineRenderer.positionCount = 2;
        minimaplineRenderer.SetPosition(0, start);
        minimaplineRenderer.SetPosition(1, end);

        CancelInvoke(nameof(HideClickLine)); // 중복 타이머 방지
        Invoke(nameof(HideClickLine), lineDuration);
    }
    private void HideClickLine()
    {
        if (minimaplineRenderer != null)
            minimaplineRenderer.gameObject.SetActive(false);
    }

    private Vector3 GetSelectedUnitsCenter()
    {
        var selectionManager = AntSelectionManager.Instance;
        if (selectionManager == null || selectionManager.SelectedAnts.Count == 0)
            return Vector3.zero;

        var selected = selectionManager.SelectedAnts;

        if (selected.Count == 1)
            return selected[0].transform.position;

        Vector3 sum = Vector3.zero;
        foreach (var ant in selected)
        {
            sum += ant.transform.position;
        }
        return sum / selected.Count;
    }

    private void MoveSelectedUnitsTo(Vector3 target)
    {
        var selectionManager = AntSelectionManager.Instance;
        if (selectionManager == null) return;
        var selectedAnts = selectionManager.SelectedAnts;
        if (selectedAnts == null || selectedAnts.Count == 0) return;

        float radius = 3f;
        int count = selectedAnts.Count;
        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 movePos = target + offset;

            var movement = selectedAnts[i].GetComponent<AntMovement>();
            if (movement != null)
            {
                movement.MoveTo(movePos);
            }
        }
    }
}
