using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class AntSelectionManager : MonoBehaviour
{
    public static AntSelectionManager Instance;

    bool IsAttackMode;
    [Header("\uB4DC\uB798\uADF8 \uBC15\uC2A4 UI (Canvas)")]
    public RectTransform selectionBox;

    [Header("UI \uC601\uC5ED: BottomPanel")]
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

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(Instance);

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

        if (Input.GetKey(KeyCode.A))
        {
            IsAttackMode = true;
            Cursor.SetCursor(attackCursor, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            IsAttackMode = false;
            Cursor.SetCursor(originCursor, Vector2.zero, CursorMode.Auto);
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            foreach (var ant in selectedAnts)
            {
                var movement = ant.GetComponent<AntMovement>();
                var bee = ant.GetComponent<BeeController>();

                movement?.Stop();

                if (bee != null)
                {
                    bee.SetAttackTarget(null);
                    bee.SetCombatMode(true);
                    var nearby = bee.FindClosestEnemyInRange();
                    if (nearby != null) bee.SetAttackTarget(nearby);
                }
            }
            moveLineRenderer.positionCount = 0;
            return;
        }

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
            selectionBox.gameObject.SetActive(false);
            isDragging = false;

            if (IsAttackMode) return;

            if (Vector2.Distance(startPos, Input.mousePosition) < clickThreshold)
                SelectSingleAnt(Input.mousePosition);
            else
                SelectAntsInBox();
        }

        if (IsAttackMode && Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;

                if (clicked.CompareTag("Enemy"))
                {
                    foreach (var ant in selectedAnts)
                    {
                        var movement = ant.GetComponent<AntMovement>();
                        var bee = ant.GetComponent<BeeController>();

                        if (movement != null && bee != null)
                        {
                            bee.SetCombatMode(true);
                            movement.OnArrive = () => bee.SetAttackTarget(clicked);
                            movement.MoveTo(clicked.transform.position);
                        }
                    }
                }
                else
                {
                    IssueMoveCommand(hit.point);
                    foreach (var ant in selectedAnts)
                    {
                        var movement = ant.GetComponent<AntMovement>();
                        var bee = ant.GetComponent<BeeController>();

                        if (movement != null && bee != null)
                        {
                            bee.SetCombatMode(true);
                            movement.OnArrive = () =>
                            {
                                GameObject nearby = bee.FindClosestEnemyInRange();
                                if (nearby != null) bee.SetAttackTarget(nearby);
                            };
                        }
                    }
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
                        bee.SetCombatMode(false);
                        movement.OnArrive = null;
                    }
                }

                IssueMoveCommand(targetPos);
            }
        }
    }

    void UpdateSelectionBox()
    {
        Vector2 size = endPos - startPos;
        Vector2 pos = startPos;

        if (size.x < 0) { pos.x = endPos.x; size.x = -size.x; }
        if (size.y < 0) { pos.y = endPos.y; size.y = -size.y; }

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
            else ClearSelection();
        }
        else ClearSelection();
    }

    void ClearSelection()
    {
        foreach (var ant in selectedAnts)
            ant.GetComponent<SelectableAnt>()?.SetSelected(false);

        selectedAnts.Clear();
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

            RectTransformUtility.ScreenPointToLocalPointInRectangle(selectionBox, screenPos, null, out Vector2 localPoint);

            bool isInBox = selectionBox.rect.Contains(localPoint);
            ant.GetComponent<SelectableAnt>()?.SetSelected(isInBox);

            if (isInBox) selectedAnts.Add(ant);
        }
    }

    public void IssueMoveCommand(Vector3 targetPos)
    {
        if (selectedAnts.Count == 0)
        {
            Debug.LogWarning("[MoveCommand] \uC120\uD0DD\uB41C \uAC1C\uBBF8\uAC00 \uC5C6\uC2B5\uB2C8\uB2E4!");
            return;
        }

        arrivedCount = 0;
        Vector3 centerPos = Vector3.zero;
        foreach (var ant in selectedAnts)
            centerPos += ant.transform.position;
        centerPos /= selectedAnts.Count;

        if (moveLineRenderer != null)
        {
            NavMeshAgent sampleAgent = selectedAnts[0].GetComponent<NavMeshAgent>();
            if (sampleAgent != null)
            {
                NavMeshPath previewPath = new NavMeshPath();
                if (sampleAgent.CalculatePath(targetPos, previewPath))
                {
                    moveLineRenderer.positionCount = previewPath.corners.Length;
                    moveLineRenderer.SetPositions(previewPath.corners);

                    float len = 0f;
                    for (int i = 1; i < previewPath.corners.Length; i++)
                        len += Vector3.Distance(previewPath.corners[i - 1], previewPath.corners[i]);

                    moveLineRenderer.material.SetFloat("_WorldLength", len);
                }
                else
                {
                    moveLineRenderer.positionCount = 0;
                }
            }
        }

        float radius = 3f;
        int count = selectedAnts.Count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * Mathf.PI * 2f / count;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 movePos = targetPos + offset;

            GameObject ant = selectedAnts[i];
            var movement = ant.GetComponent<AntMovement>();
            if (movement != null)
            {
                movement.OnArrive = () =>
                {
                    arrivedCount++;
                    if (arrivedCount >= selectedAnts.Count)
                        moveLineRenderer.positionCount = 0;
                };
                movement.MoveTo(movePos);
            }
        }

        RTSCameraController.instance.followTransform = selectedAnts[0].transform;
    }

    private bool IsPointerOverUIElement()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }
}