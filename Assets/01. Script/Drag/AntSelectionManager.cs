using UnityEngine;
using System.Collections.Generic;

public class AntSelectionManager : MonoBehaviour
{
    public RectTransform selectionBox;      // Canvas 안 드래그 박스 UI
   LineRenderer moveLineRenderer;   // 월드 공간에 그릴 이동 경로 선

    private Vector2 startPos;
    private Vector2 endPos;

    private Vector3 dragWorldStartPos;      // 드래그 시작 지점 월드 좌표

    private List<GameObject> selectedAnts = new List<GameObject>();

    private float clickThreshold = 5f;      // 클릭 vs 드래그 구분 픽셀
    private int arrivedCount = 0;

    private void Awake()
    {
        moveLineRenderer = GetComponent<LineRenderer>();    
    }


    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);

            // 드래그 시작 지점 월드 좌표 저장
            Ray ray = Camera.main.ScreenPointToRay(startPos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                dragWorldStartPos = hit.point;
            }
        }

        if (Input.GetMouseButton(0))
        {
            endPos = Input.mousePosition;
            UpdateSelectionBox();
        }

        if (Input.GetMouseButtonUp(0))
        {
            selectionBox.gameObject.SetActive(false);

            if (Vector2.Distance(startPos, Input.mousePosition) < clickThreshold)
            {
                SelectSingleAnt(Input.mousePosition);
            }
            else
            {
                SelectAntsInBox();
            }
        }

        if (Input.GetMouseButtonDown(1))
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
            {
                selectable.SetSelected(false);
            }
        }
        selectedAnts.Clear();

        // 이동 경로선도 초기화
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
            if (screenPos.z < 0)
                continue;

            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                selectionBox, screenPos, null, out localPoint);

            bool isInBox = selectionBox.rect.Contains(localPoint);

            var selectable = ant.GetComponent<SelectableAnt>();
            if (selectable != null)
            {
                selectable.SetSelected(isInBox);
            }

            if (isInBox)
            {
                selectedAnts.Add(ant);
            }
        }
    }

    void IssueMoveCommand()
    {
        if (selectedAnts.Count == 0) return;

        arrivedCount = 0;  // 초기화

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

                        if (arrivedCount >= selectedAnts.Count)
                        {
                            if (moveLineRenderer != null)
                                moveLineRenderer.positionCount = 0;  // 모두 도착하면 선 제거
                        }
                    };
                    movement.MoveTo(movePos);
                }
            }
        }
    }



}
