using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AntSelectionManager : MonoBehaviour
{
    [Header("드래그 박스 UI (Canvas)")]
    public RectTransform selectionBox;

    [Header("UI 영역: BottomPanel")]
    public RectTransform bottomPanel;

    private LineRenderer moveLineRenderer;

    private Vector2 startPos;
    private Vector2 endPos;
    private Vector3 dragWorldStartPos;

    private List<GameObject> selectedAnts = new List<GameObject>();

    private float clickThreshold = 5f;
    private int arrivedCount = 0;

    private bool isDragging = false;
    private bool readyToDrag = false;

    private void Awake()
    {
        moveLineRenderer = GetComponent<LineRenderer>();
        selectionBox.gameObject.SetActive(false);
    }

    private void Start()
    {
        StartCoroutine(EnableDragNextFrame());
    }

    IEnumerator EnableDragNextFrame()
    {
        yield return null;
        readyToDrag = true;
    }

    private void Update()
    {
        if (!readyToDrag) return;

        // 마우스 Down (드래그 시작)
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIElement())
        {
            isDragging = true;
            startPos = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);

            Ray ray = Camera.main.ScreenPointToRay(startPos);
            if (Physics.Raycast(ray, out RaycastHit hit))
                dragWorldStartPos = hit.point;
        }

        // 드래그 중
        if (isDragging && Input.GetMouseButton(0))
        {
            endPos = Input.mousePosition;
            UpdateSelectionBox();
        }

        // 마우스 Up (드래그 종료)
        if (Input.GetMouseButtonUp(0))
        {
            selectionBox.gameObject.SetActive(false);

            if (isDragging)
            {
                isDragging = false;

                if (Vector2.Distance(startPos, Input.mousePosition) < clickThreshold)
                    SelectSingleAnt(Input.mousePosition);
                else
                    SelectAntsInBox();
            }
        }

        // 우클릭으로 이동 명령
        if (Input.GetMouseButtonDown(1) && !IsPointerOverUIElement())
        {
            IssueMoveCommand();
        }
    }

    void UpdateSelectionBox()
    {
        Vector2 boxStart = startPos;
        Vector2 boxEnd = endPos;

        Vector2 size = boxEnd - boxStart;
        Vector2 pos = boxStart;

        if (size.x < 0)
        {
            pos.x = boxEnd.x;
            size.x = -size.x;
        }
        if (size.y < 0)
        {
            pos.y = boxEnd.y;
            size.y = -size.y;
        }

        selectionBox.position = pos + size * 0.5f;
        selectionBox.sizeDelta = size;
    }

    void SelectSingleAnt(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var ant = hit.collider.gameObject;
            if (ant.CompareTag("FastAnt"))
            {
                ClearSelection();
                var selectable = ant.GetComponent<SelectableAnt>();
                if (selectable != null)
                {
                    selectable.SetSelected(true);
                    selectedAnts.Add(ant);
                }
            }
            else
            {
                ClearSelection();
            }
        }
        else
        {
            ClearSelection();
        }
    }

    void ClearSelection()
    {
        foreach (var ant in selectedAnts)
        {
            var selectable = ant.GetComponent<SelectableAnt>();
            if (selectable != null)
                selectable.SetSelected(false);
        }

        selectedAnts.Clear();

        if (moveLineRenderer != null)
            moveLineRenderer.positionCount = 0;
    }

    void SelectAntsInBox()
    {
        selectedAnts.Clear();
        var allAnts = GameObject.FindGameObjectsWithTag("FastAnt");

        foreach (var ant in allAnts)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(ant.transform.position);
            if (screenPos.z < 0) continue;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(selectionBox, screenPos, null, out localPoint);

            bool isInBox = selectionBox.rect.Contains(localPoint);
            var selectable = ant.GetComponent<SelectableAnt>();
            if (selectable != null)
                selectable.SetSelected(isInBox);

            if (isInBox)
                selectedAnts.Add(ant);
        }
    }

    void IssueMoveCommand()
    {
        if (selectedAnts.Count == 0) return;

        arrivedCount = 0;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPos = hit.point;

            Vector3 centerPos = Vector3.zero;
            foreach (var ant in selectedAnts)
                centerPos += ant.transform.position;
            centerPos /= selectedAnts.Count;

            if (moveLineRenderer != null)
            {
                moveLineRenderer.positionCount = 2;
                moveLineRenderer.SetPosition(0, centerPos);
                moveLineRenderer.SetPosition(1, targetPos);
            }

            float radius = 3f;
            int count = selectedAnts.Count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * Mathf.PI * 2f / count;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
                Vector3 movePos = targetPos + offset;

                var movement = selectedAnts[i].GetComponent<AntMovement>();
                if (movement != null)
                {
                    movement.OnArrive = () =>
                    {
                        arrivedCount++;
                        if (arrivedCount >= selectedAnts.Count && moveLineRenderer != null)
                            moveLineRenderer.positionCount = 0;
                    };

                    movement.MoveTo(movePos);
                }
            }
        }
    }

    /// <summary>
    /// 모든 UI 요소 위에 마우스가 있는지 검사
    /// </summary>
    private bool IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }
}
