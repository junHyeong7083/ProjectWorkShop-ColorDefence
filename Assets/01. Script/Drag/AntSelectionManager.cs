using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class AntSelectionManager : MonoBehaviour
{
    [Header("드래그 박스 UI (Canvas)")]
    public RectTransform selectionBox;      // Canvas 안 드래그 박스 UI

    [Header("UI 영역: BottomPanel")]
    [Tooltip("이 RectTransform 위에서는 선택/드래그 로직을 실행하지 않습니다.")]
    public RectTransform bottomPanel;

    LineRenderer moveLineRenderer;          // 월드 공간에 그릴 이동 경로 선

    private Vector2 startPos;
    private Vector2 endPos;

    private Vector3 dragWorldStartPos;      // 드래그 시작 지점 월드 좌표

    private List<GameObject> selectedAnts = new List<GameObject>();

    private float clickThreshold = 5f;      // 클릭 vs 드래그 구분 픽셀
    private int arrivedCount = 0;

    private void Awake()
    {
        moveLineRenderer = GetComponent<LineRenderer>();
        // selectionBox는 미리 Inspector에서 활성화 상태 꺼둔 뒤, 필요할 때 켜고 끄면 됩니다.
        selectionBox.gameObject.SetActive(false);
    }

    void Update()
    {
        if (PlacementManager.Instance != null && PlacementManager.Instance.IsPlacing)
            return;

        // 1) 마우스 좌클릭 Down 처리
        if (Input.GetMouseButtonDown(0))
        {
            // 1-1) 우선, 마우스가 BottomPanel 위에 있는지 확인
            if (IsPointerOverBottomPanel())
            {
                // BottomPanel 위에서는 선택 로직 취소
                return;
            }

            // 선택박스 열기
            startPos = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);

            // 드래그 시작 지점의 월드 좌표 저장 (LineRenderer 용)
            Ray ray = Camera.main.ScreenPointToRay(startPos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                dragWorldStartPos = hit.point;
            }
        }

        // 2) 마우스 좌클릭 Drag 처리
        if (Input.GetMouseButton(0))
        {
            // 2-1) 마찬가지로, 드래그 중에도 처음 누른 시점이 BottomPanel이었으면 여기를 건너뜀
            if (IsPointerOverBottomPanel())
            {
                // Drag 중이라도 마우스가 BottomPanel 위라면 선택박스 갱신하지 않음
                return;
            }

            endPos = Input.mousePosition;
            UpdateSelectionBox();
        }

        // 3) 마우스 좌클릭 Up 처리
        if (Input.GetMouseButtonUp(0))
        {
            // 3-1) Up시에 BottomPanel 위라면 “선택 취소”만 하고 빠져나간다
            if (IsPointerOverBottomPanel())
            {
                selectionBox.gameObject.SetActive(false);
                return;
            }

            // selection box 끄기
            selectionBox.gameObject.SetActive(false);

            // 클릭인지 드래그인지 판별
            if (Vector2.Distance(startPos, Input.mousePosition) < clickThreshold)
            {
                SelectSingleAnt(Input.mousePosition);
            }
            else
            {
                SelectAntsInBox();
            }
        }

        // 4) 마우스 우클릭 처리 (이동 명령)
        if (Input.GetMouseButtonDown(1))
        {
            // 우클릭 시에도, BottomPanel 위라면 명령 무시
            if (IsPointerOverBottomPanel())
            {
                return;
            }

            IssueMoveCommand();
        }
    }

    /// <summary>
    /// selectionBox(RectTransform)를 화면의 startPos~endPos 영역으로 업데이트합니다.
    /// </summary>
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

        // selectionBox는 Canvas 상의 RectTransform이므로 position, sizeDelta로 조절
        selectionBox.position = pos + size * 0.5f;
        selectionBox.sizeDelta = size;
    }

    /// <summary>
    /// 단일 클릭(Left Click) 시 해당 위치에 Raycast를 쏴서 FastAnt를 선택합니다.
    /// </summary>
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

    /// <summary>
    /// 기존 선택을 모두 해제합니다.
    /// </summary>
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

    /// <summary>
    /// Drag로 만들어진 사각형 안에 들어온 FastAnt들을 모두 선택 상태로 만듭니다.
    /// </summary>
    void SelectAntsInBox()
    {
        selectedAnts.Clear();
        var allAnts = GameObject.FindGameObjectsWithTag("FastAnt");

        foreach (var ant in allAnts)
        {
            Vector3 screenPos = Camera.main.WorldToScreenPoint(ant.transform.position);
            if (screenPos.z < 0)
                continue;

            // selectionBox 내부 좌표계(localPoint) 계산
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
                selectedAnts.Add(ant);
        }
    }

    /// <summary>
    /// 우클릭 시 선택된 FastAnt들에게 이동 명령을 내립니다.
    /// </summary>
    void IssueMoveCommand()
    {
        if (selectedAnts.Count == 0) return;

        arrivedCount = 0;  // 이동 완료 카운트 초기화

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPos = hit.point;

            // 선택된 개미들의 중심점 계산
            Vector3 centerPos = Vector3.zero;
            foreach (var ant in selectedAnts)
                centerPos += ant.transform.position;
            centerPos /= selectedAnts.Count;

            // LineRenderer로 이동 경로 표시
            if (moveLineRenderer != null)
            {
                moveLineRenderer.positionCount = 2;
                moveLineRenderer.SetPosition(0, centerPos);
                moveLineRenderer.SetPosition(1, targetPos);
            }

            // 반지름을 두고 원형 배치로 각각 movePos 계산
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
                        {
                            moveLineRenderer.positionCount = 0;  // 모두 도착하면 선 제거
                        }
                    };
                    movement.MoveTo(movePos);
                }
            }
        }
    }

    /// <summary>
    /// BottomPanel 영역 위에 커서가 올라와 있는지를 검사합니다.
    /// </summary>
    /// <returns>마우스 포인터가 bottomPanel 안에 있으면 true, 아니면 false.</returns>
    private bool IsPointerOverBottomPanel()
    {
        if (bottomPanel == null)
            return false;

        // 현재 마우스 포인터(스크린 좌표)
        Vector2 screenPoint = Input.mousePosition;

        // bottomPanel의 RectTransform의 앵커 좌표계 내에 있는지 검사
        return RectTransformUtility.RectangleContainsScreenPoint(
            bottomPanel, screenPoint, null
        );
    }
}
