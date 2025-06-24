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
    [SerializeField] private LayerMask blockCollisionMask;

    private float originalZ;
    private Vector3 targetPosition;

    private void Start()
    {
        grid = FindObjectOfType<GridManager>();
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
        targetPosition = mouseWorld;
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