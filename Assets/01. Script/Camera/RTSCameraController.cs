using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEngine.GraphicsBuffer;

public class RTSCameraController : MonoBehaviour
{
    public static RTSCameraController instance;

    [Header("General")]
    [SerializeField] Transform cameraTransform;
    public Transform followTransform;
    Vector3 newPosition;
    Vector3 dragStartPosition;
    Vector3 dragCurrentPosition;

    [Header("Optional Functionality")]
    [SerializeField] bool moveWithKeyboad = true;
    [SerializeField] bool moveWithEdgeScrolling = true;
    [SerializeField] bool moveWithMouseDrag = true;

    [Header("Keyboard Movement")]
    [SerializeField] float fastSpeed = 2f;
    [SerializeField] float normalSpeed = 0.5f;
    [SerializeField] float movementSensitivity = 5f;
    float movementSpeed;

    [Header("Edge Scrolling Movement")]
    [SerializeField] float edgeSize = 50f;
    bool isCursorSet = false;
    public Texture2D cursorArrowUp;
    public Texture2D cursorArrowDown;
    public Texture2D cursorArrowLeft;
    public Texture2D cursorArrowRight;

    [Header("UI")]
    [SerializeField] private RectTransform bottomPanel;

    CursorArrow currentCursor = CursorArrow.DEFAULT;
    enum CursorArrow { UP, DOWN, LEFT, RIGHT, DEFAULT }
    [SerializeField] private float edgeScrollSpeed = 0.5f;
    private void Start()
    {
        instance = this;
        newPosition = transform.position;
        movementSpeed = normalSpeed;
    }

    [SerializeField] private Vector3 followOffset = new Vector3(-20.83f, 43.93f, -24.87f);
    [SerializeField] private float followLerpSpeed = 5f;
    private bool isTemporaryFollow = false;

    private void Update()
    {
        // 스페이스바 눌렀을 때만 따라감
        if (Input.GetKeyDown(KeyCode.Space) && followTransform != null)
        {
            isTemporaryFollow = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            isTemporaryFollow = false;
        }

        // 마우스 드래그 시 따라가기 해제
        if (Input.GetMouseButtonDown(2) && !EventSystem.current.IsPointerOverGameObject())
        {
            followTransform = null;
            isTemporaryFollow = false;
        }

        if (followTransform != null && isTemporaryFollow)
        {
            Vector3 targetPos = followTransform.position + followOffset;
            targetPos.y = transform.position.y;

            transform.position = Vector3.Lerp(transform.position, targetPos, followLerpSpeed * Time.deltaTime);
            newPosition = transform.position;
        }
        else
        {
            HandleCameraMovement();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            followTransform = null;
            isTemporaryFollow = false;
        }
    }

    void HandleCameraMovement()
    {
        Vector3 flatForward = new Vector3(cameraTransform.forward.x, 0f, cameraTransform.forward.z).normalized;
        Vector3 flatRight = new Vector3(cameraTransform.right.x, 0f, cameraTransform.right.z).normalized;

        if (moveWithMouseDrag)
        {
            HandleMouseDragInput();
        }

        if (moveWithKeyboad)
        {
            movementSpeed = Input.GetKey(KeyCode.LeftCommand) ? fastSpeed : normalSpeed;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                newPosition += flatForward * movementSpeed;

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                newPosition -= flatForward * movementSpeed;

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                newPosition += flatRight * movementSpeed;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                newPosition -= flatRight * movementSpeed;
        }

        if (moveWithEdgeScrolling)
        {
            if (Input.mousePosition.x > Screen.width - edgeSize)
            {
                newPosition += flatRight * edgeScrollSpeed;
                ChangeCursor(CursorArrow.RIGHT);
                isCursorSet = true;
            }
            else if (Input.mousePosition.x < edgeSize)
            {
                newPosition -= flatRight * edgeScrollSpeed;
                ChangeCursor(CursorArrow.LEFT);
                isCursorSet = true;
            }
            else if (Input.mousePosition.y > Screen.height - edgeSize)
            {
                newPosition += flatForward * edgeScrollSpeed;
                ChangeCursor(CursorArrow.UP);
                isCursorSet = true;
            }
            else if (Input.mousePosition.y < edgeSize)
            {
                newPosition -= flatForward * edgeScrollSpeed;
                ChangeCursor(CursorArrow.DOWN);
                isCursorSet = true;
            }
            else
            {
                if (isCursorSet)
                {
                    ChangeCursor(CursorArrow.DEFAULT);
                    isCursorSet = false;
                }
            }
        }

        transform.position = newPosition;
        Cursor.lockState = CursorLockMode.Confined;
    }

    private bool IsMouseOverUIRect(RectTransform rectTransform)
    {
        if (rectTransform == null) return false;

        Vector2 localMousePosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            Input.mousePosition,
            null,
            out localMousePosition
        );

        return rectTransform.rect.Contains(localMousePosition);
    }

    private void ChangeCursor(CursorArrow newCursor)
    {
        if (currentCursor != newCursor)
        {
            switch (newCursor)
            {
                case CursorArrow.UP:
                    Cursor.SetCursor(cursorArrowUp, Vector2.zero, CursorMode.Auto);
                    break;
                case CursorArrow.DOWN:
                    Cursor.SetCursor(cursorArrowDown, new Vector2(cursorArrowDown.width, cursorArrowDown.height), CursorMode.Auto);
                    break;
                case CursorArrow.LEFT:
                    Cursor.SetCursor(cursorArrowLeft, Vector2.zero, CursorMode.Auto);
                    break;
                case CursorArrow.RIGHT:
                    Cursor.SetCursor(cursorArrowRight, new Vector2(cursorArrowRight.width, cursorArrowRight.height), CursorMode.Auto);
                    break;
                case CursorArrow.DEFAULT:
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    break;
            }

            currentCursor = newCursor;
        }
    }

    public void FollowUnit(Transform target)
    {
        followTransform = target;
        isTemporaryFollow = false;

        Vector3 targetPos = target.position + followOffset;
        targetPos.y = transform.position.y;

        transform.position = targetPos;
        newPosition = targetPos;
    }

    public void TeleportTo(Vector3 worldPos)
    {
        followTransform = null;
        isTemporaryFollow = false;

        transform.position = worldPos;
        newPosition = worldPos;
    }

    private void HandleMouseDragInput()
    {
        if (Input.GetMouseButtonDown(2) && !EventSystem.current.IsPointerOverGameObject())
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float entry))
            {
                dragStartPosition = ray.GetPoint(entry);
            }
        }

        if (Input.GetMouseButton(2) && !EventSystem.current.IsPointerOverGameObject())
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float entry))
            {
                dragCurrentPosition = ray.GetPoint(entry);
                newPosition = transform.position + dragStartPosition - dragCurrentPosition;
            }
        }
    }
}
