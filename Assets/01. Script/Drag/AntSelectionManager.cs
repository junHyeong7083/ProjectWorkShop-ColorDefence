using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AntSelectionManager : MonoBehaviour
{
    public static AntSelectionManager Instance; 


     bool IsAttackMode;
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
    [SerializeField] private Texture2D originCursor;
    [SerializeField] private Texture2D attackCursor;
    public List<GameObject> SelectedAnts => selectedAnts;
    private bool isCursorInAttackMode = false; // 중복 호출 방지용
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else 
            Destroy(Instance);


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
        if (Input.GetMouseButtonDown(0) && !IsPointerOverUIElement())
        {
            isDragging = true;
            startPos = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);

            Ray ray = Camera.main.ScreenPointToRay(startPos);
            if (Physics.Raycast(ray, out RaycastHit hit))
                dragWorldStartPos = hit.point;
        }

        if (isDragging && Input.GetMouseButton(0))
        {
            endPos = Input.mousePosition;
            UpdateSelectionBox();
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (isCursorInAttackMode)
            {
                Cursor.SetCursor(attackCursor, Vector2.zero, CursorMode.Auto);
                isCursorInAttackMode = false;
            }

            selectionBox.gameObject.SetActive(false);
            isDragging = false;

            if (Vector2.Distance(startPos, Input.mousePosition) < clickThreshold)
                SelectSingleAnt(Input.mousePosition);
            else
                SelectAntsInBox();
        }
        if (IsAttackMode && Input.GetMouseButtonDown(0) && !IsPointerOverUIElement())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Debug.Log($"AttackMode !! ||| MousePos {ray}");
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;
                Debug.Log($"Raycast hit: {clicked.name}");

                if (clicked.CompareTag("Enemy"))
                {
                    Debug.Log("적 태그 확인됨!");
                    if (!isCursorInAttackMode)
                    {
                        Cursor.SetCursor(attackCursor, Vector2.zero, CursorMode.Auto);
                        isCursorInAttackMode = true;
                    }

                    {
                        foreach (var ant in selectedAnts)
                        {
                            var movement = ant.GetComponent<AntMovement>();
                            var bee = ant.GetComponent<BeeController>();

                            if (movement != null && bee != null)
                            {
                                movement.OnArrive = () =>
                                {
                                    bee.SetAttackTarget(clicked);
                                };
                                movement.MoveTo(clicked.transform.position);
                            }
                        }
                    } // 선택된 유닛 foreach
                }
                else
                {
                    if (isCursorInAttackMode)
                    {
                        Cursor.SetCursor(originCursor, Vector2.zero, CursorMode.Auto);
                        isCursorInAttackMode = false;
                    }
                }
            }
            else
            {
                if (isCursorInAttackMode)
                {
                    Cursor.SetCursor(originCursor, Vector2.zero, CursorMode.Auto);
                    isCursorInAttackMode = false;
                }
            }
            IsAttackMode = false;
        }
        else if (Input.GetMouseButtonDown(1) && !IsPointerOverUIElement())
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Vector3 targetPos = hit.point;

                foreach (var ant in selectedAnts)
                {
                    var movement = ant.GetComponent<AntMovement>();
                    var bee = ant.GetComponent<BeeController>();

                    if (movement != null && bee != null)
                    {
                        movement.OnArrive = () =>
                        {
                            GameObject nearby = bee.FindClosestEnemyInRange();
                            if (nearby != null)
                                bee.SetAttackTarget(nearby);
                        };
                    }
                }
                IssueMoveCommand(targetPos);
            }
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

    public void IssueMoveCommand(Vector3 targetPos)
    {
        if (selectedAnts.Count == 0) return;

        arrivedCount = 0;

        Vector3 centerPos = Vector3.zero;
        foreach (var ant in selectedAnts)
            centerPos += ant.transform.position;
        centerPos /= selectedAnts.Count;

        if (moveLineRenderer != null)
        {
            moveLineRenderer.positionCount = 2;
            moveLineRenderer.SetPosition(0, centerPos);
            moveLineRenderer.SetPosition(1, targetPos);

            float len = Vector3.Distance(centerPos, targetPos);
            moveLineRenderer.material.SetFloat("_WorldLength", len);
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