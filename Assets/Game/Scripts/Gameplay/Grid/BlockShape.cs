using HAVIGAME;
using UnityEngine;

public class BlockShape : MonoBehaviour
{
    public Vector2Int CurrentOrigin { get; set; }
    public Vector2Int[] occupiedOffsets;

    private GridManager grid;
    private Vector3 dragOffset;
    private Vector3 originalPosition;
    private Vector2Int originalGridPos;

    private Vector2Int previewOrigin;
    private bool hasPreview = false;
    private bool isDragging = false;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float dragZ = -0.3f;
    [SerializeField] private Vector3 boxCastHalfExtents = new Vector3(0.45f, 0.45f, 0.1f);
    [SerializeField] private LayerMask blockCollisionMask, wallCollisionMask;

    private float originalZ;
    private Vector3 targetPosition;
    [Header("Color Data")] public BlockColorData colorData;

    public Renderer renderer;

    private void Start()
    {
        grid = FindObjectOfType<GridManager>();
        if (colorData != null)
        {
            renderer.material = colorData.material;
        }
    }

    private void Update()
    {
        if (!isDragging) return;

        Vector3 toTarget = targetPosition - transform.position;
        float totalDist = toTarget.magnitude;
        if (totalDist < 0.01f) return;

        float step = speed * Time.deltaTime;
        float moveDist = Mathf.Min(step, totalDist);

        // Ưu tiên trục Y
        Vector3 yDir = new Vector3(0, toTarget.y, 0);
        if (yDir.magnitude > 0.01f)
        {
            Vector3 moveDirY = yDir.normalized;
            float moveY = Mathf.Min(moveDist, Mathf.Abs(yDir.y));

            if (!CheckCollision(moveDirY, moveY))
            {
                transform.position += moveDirY * moveY;
                moveDist -= moveY;
            }
            else if (Physics.BoxCast(transform.position, boxCastHalfExtents, moveDirY, out RaycastHit hitY,
                         Quaternion.identity, moveY, blockCollisionMask))
            {
                Vector3 slideDir = Vector3.ProjectOnPlane(moveDirY, hitY.normal).normalized;
                if (!CheckSlide(slideDir, moveY))
                    transform.position += slideDir * moveY;
            }
        }

        // Sau đó mới di chuyển theo trục X
        Vector3 xDir = new Vector3(toTarget.x, 0, 0);
        if (xDir.magnitude > 0.01f)
        {
            Vector3 moveDirX = xDir.normalized;
            float moveX = Mathf.Min(moveDist, Mathf.Abs(xDir.x));

            if (!CheckCollision(moveDirX, moveX))
            {
                transform.position += moveDirX * moveX;
            }
            else if (Physics.BoxCast(transform.position, boxCastHalfExtents, moveDirX, out RaycastHit hitX,
                         Quaternion.identity, moveX, blockCollisionMask))
            {
                Vector3 slideDir = Vector3.ProjectOnPlane(moveDirX, hitX.normal).normalized;
                if (!CheckSlide(slideDir, moveX))
                    transform.position += slideDir * moveX;
            }
        }

        // Snap preview
        Vector2Int snapped = grid.WorldToGrid(transform.position);
        hasPreview = false;

        if (grid.IsValid(snapped) && grid.CanPlaceBlock(snapped, this))
        {
            previewOrigin = snapped;
            hasPreview = true;
        }
        else if (grid.TryFindNearestValidPlacement(this, out Vector2Int nearest))
        {
            previewOrigin = nearest;
            hasPreview = true;
        }

        grid.ClearAllPreviews();
        if (hasPreview)
        {
            grid.SetPreviewAt(previewOrigin, this, grid.CanPlaceBlock(previewOrigin, this));
        }
    }

    private bool CheckCollision(Vector3 moveDir, float moveDist)
    {
        foreach (var offset in occupiedOffsets)
        {
            Vector3 pos = transform.position + new Vector3(offset.x, offset.y, 0) * grid.cellSize;
            if (Physics.BoxCast(pos, boxCastHalfExtents, moveDir, out _, Quaternion.identity, moveDist,
                    blockCollisionMask))
                return true;
        }

        return false;
    }

    private bool CheckSlide(Vector3 slideDir, float moveDist)
    {
        foreach (var offset in occupiedOffsets)
        {
            Vector3 pos = transform.position + new Vector3(offset.x, offset.y, 0) * grid.cellSize;
            if (Physics.BoxCast(pos, boxCastHalfExtents, slideDir, out _, Quaternion.identity, moveDist,
                    blockCollisionMask))
                return true;
        }

        return false;
    }

    private void OnMouseDown()
    {
        isDragging = true;
        originalPosition = transform.position;
        originalGridPos = CurrentOrigin;
        dragOffset = transform.position - GetMouseWorldByRay();
        originalZ = transform.position.z;

        // Gỡ mark occupied
        foreach (var offset in occupiedOffsets)
        {
            Vector2Int pos = originalGridPos + offset;
            if (grid.IsValid(pos)) grid.SetOccupied(pos, false);
        }

        // Kéo z lên
        Vector3 raised = transform.position;
        raised.z = dragZ;
        transform.position = raised;
    }

    private void OnMouseDrag()
{
    if (!isDragging) return;

    Vector3 mouseWorld = GetMouseWorldByRay() + dragOffset;
    mouseWorld.z = dragZ;

    // Chuyển đổi vị trí con trỏ sang vị trí trong grid
    Vector2Int gridPos = grid.WorldToGrid(mouseWorld);

    if (grid.IsValid(gridPos))
    {
        targetPosition = mouseWorld;
    }
    else
    {
        // Tìm vị trí hợp lệ gần nhất trên biên grid theo hướng con trỏ
        Vector2Int nearestEdgePos = FindNearestEdgePosition(gridPos, mouseWorld);
        targetPosition = grid.GridToWorld(nearestEdgePos) + new Vector3(0, 0, dragZ);
    }
}

private Vector2Int FindNearestEdgePosition(Vector2Int invalidPos, Vector3 mouseWorld)
{
    Vector2Int nearestEdgePos = invalidPos;
    float minDistance = float.MaxValue;

    foreach (var cellPos in grid.GetAllValidCells())
    {
        // Chỉ xét các vị trí nằm trên biên grid
        if (IsOnGridEdge(cellPos))
        {
            Vector3 cellWorldPos = grid.GridToWorld(cellPos);
            float distance = Vector3.Distance(cellWorldPos, mouseWorld);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestEdgePos = cellPos;
            }
        }
    }

    return nearestEdgePos;
}

    private bool IsOnGridEdge(Vector2Int pos)
    {
        // Kiểm tra nếu vị trí nằm trên biên grid
        return !grid.IsValid(new Vector2Int(pos.x - 1, pos.y)) ||
               !grid.IsValid(new Vector2Int(pos.x + 1, pos.y)) ||
               !grid.IsValid(new Vector2Int(pos.x, pos.y - 1)) ||
               !grid.IsValid(new Vector2Int(pos.x, pos.y + 1));
    }

    private void OnMouseUp()
    {
        isDragging = false;
        grid.ClearAllPreviews();
        if (hasPreview && grid.CanPlaceBlock(previewOrigin, this))
        {
            grid.MoveBlockTo(this, previewOrigin);
        }
        else
        {
            // Trả về vị trí cũ
            transform.position = originalPosition;
            CurrentOrigin = originalGridPos;

            foreach (var offset in occupiedOffsets)
            {
                Vector2Int pos = originalGridPos + offset;
                if (grid.IsValid(pos)) grid.SetOccupied(pos, true);
            }
        }

        // Hạ z
        Vector3 final = transform.position;
        final.z = originalZ;
        transform.position = final;
    }

    private Vector3 GetMouseWorldByRay()
    {
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
            return ray.GetPoint(distance);
        return Vector3.zero;
    }
}