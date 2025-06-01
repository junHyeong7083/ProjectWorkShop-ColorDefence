using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class AntSelectionManager : MonoBehaviour
{
    [Header("�巡�� �ڽ� UI (Canvas)")]
    public RectTransform selectionBox;      // Canvas �� �巡�� �ڽ� UI

    [Header("UI ����: BottomPanel")]
    [Tooltip("�� RectTransform �������� ����/�巡�� ������ �������� �ʽ��ϴ�.")]
    public RectTransform bottomPanel;

    LineRenderer moveLineRenderer;          // ���� ������ �׸� �̵� ��� ��

    private Vector2 startPos;
    private Vector2 endPos;

    private Vector3 dragWorldStartPos;      // �巡�� ���� ���� ���� ��ǥ

    private List<GameObject> selectedAnts = new List<GameObject>();

    private float clickThreshold = 5f;      // Ŭ�� vs �巡�� ���� �ȼ�
    private int arrivedCount = 0;

    private void Awake()
    {
        moveLineRenderer = GetComponent<LineRenderer>();
        // selectionBox�� �̸� Inspector���� Ȱ��ȭ ���� ���� ��, �ʿ��� �� �Ѱ� ���� �˴ϴ�.
        selectionBox.gameObject.SetActive(false);
    }

    void Update()
    {
        if (PlacementManager.Instance != null && PlacementManager.Instance.IsPlacing)
            return;

        // 1) ���콺 ��Ŭ�� Down ó��
        if (Input.GetMouseButtonDown(0))
        {
            // 1-1) �켱, ���콺�� BottomPanel ���� �ִ��� Ȯ��
            if (IsPointerOverBottomPanel())
            {
                // BottomPanel �������� ���� ���� ���
                return;
            }

            // ���ùڽ� ����
            startPos = Input.mousePosition;
            selectionBox.gameObject.SetActive(true);

            // �巡�� ���� ������ ���� ��ǥ ���� (LineRenderer ��)
            Ray ray = Camera.main.ScreenPointToRay(startPos);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                dragWorldStartPos = hit.point;
            }
        }

        // 2) ���콺 ��Ŭ�� Drag ó��
        if (Input.GetMouseButton(0))
        {
            // 2-1) ����������, �巡�� �߿��� ó�� ���� ������ BottomPanel�̾����� ���⸦ �ǳʶ�
            if (IsPointerOverBottomPanel())
            {
                // Drag ���̶� ���콺�� BottomPanel ����� ���ùڽ� �������� ����
                return;
            }

            endPos = Input.mousePosition;
            UpdateSelectionBox();
        }

        // 3) ���콺 ��Ŭ�� Up ó��
        if (Input.GetMouseButtonUp(0))
        {
            // 3-1) Up�ÿ� BottomPanel ����� ������ ��ҡ��� �ϰ� ����������
            if (IsPointerOverBottomPanel())
            {
                selectionBox.gameObject.SetActive(false);
                return;
            }

            // selection box ����
            selectionBox.gameObject.SetActive(false);

            // Ŭ������ �巡������ �Ǻ�
            if (Vector2.Distance(startPos, Input.mousePosition) < clickThreshold)
            {
                SelectSingleAnt(Input.mousePosition);
            }
            else
            {
                SelectAntsInBox();
            }
        }

        // 4) ���콺 ��Ŭ�� ó�� (�̵� ���)
        if (Input.GetMouseButtonDown(1))
        {
            // ��Ŭ�� �ÿ���, BottomPanel ����� ��� ����
            if (IsPointerOverBottomPanel())
            {
                return;
            }

            IssueMoveCommand();
        }
    }

    /// <summary>
    /// selectionBox(RectTransform)�� ȭ���� startPos~endPos �������� ������Ʈ�մϴ�.
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

        // selectionBox�� Canvas ���� RectTransform�̹Ƿ� position, sizeDelta�� ����
        selectionBox.position = pos + size * 0.5f;
        selectionBox.sizeDelta = size;
    }

    /// <summary>
    /// ���� Ŭ��(Left Click) �� �ش� ��ġ�� Raycast�� ���� FastAnt�� �����մϴ�.
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
    /// ���� ������ ��� �����մϴ�.
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

        // �̵� ��μ��� �ʱ�ȭ
        if (moveLineRenderer != null)
            moveLineRenderer.positionCount = 0;
    }

    /// <summary>
    /// Drag�� ������� �簢�� �ȿ� ���� FastAnt���� ��� ���� ���·� ����ϴ�.
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

            // selectionBox ���� ��ǥ��(localPoint) ���
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
    /// ��Ŭ�� �� ���õ� FastAnt�鿡�� �̵� ����� �����ϴ�.
    /// </summary>
    void IssueMoveCommand()
    {
        if (selectedAnts.Count == 0) return;

        arrivedCount = 0;  // �̵� �Ϸ� ī��Ʈ �ʱ�ȭ

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3 targetPos = hit.point;

            // ���õ� ���̵��� �߽��� ���
            Vector3 centerPos = Vector3.zero;
            foreach (var ant in selectedAnts)
                centerPos += ant.transform.position;
            centerPos /= selectedAnts.Count;

            // LineRenderer�� �̵� ��� ǥ��
            if (moveLineRenderer != null)
            {
                moveLineRenderer.positionCount = 2;
                moveLineRenderer.SetPosition(0, centerPos);
                moveLineRenderer.SetPosition(1, targetPos);
            }

            // �������� �ΰ� ���� ��ġ�� ���� movePos ���
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
                            moveLineRenderer.positionCount = 0;  // ��� �����ϸ� �� ����
                        }
                    };
                    movement.MoveTo(movePos);
                }
            }
        }
    }

    /// <summary>
    /// BottomPanel ���� ���� Ŀ���� �ö�� �ִ����� �˻��մϴ�.
    /// </summary>
    /// <returns>���콺 �����Ͱ� bottomPanel �ȿ� ������ true, �ƴϸ� false.</returns>
    private bool IsPointerOverBottomPanel()
    {
        if (bottomPanel == null)
            return false;

        // ���� ���콺 ������(��ũ�� ��ǥ)
        Vector2 screenPoint = Input.mousePosition;

        // bottomPanel�� RectTransform�� ��Ŀ ��ǥ�� ���� �ִ��� �˻�
        return RectTransformUtility.RectangleContainsScreenPoint(
            bottomPanel, screenPoint, null
        );
    }
}
