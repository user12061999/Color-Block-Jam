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

    private float originalZ;
    private float dragZ = -2f;

    private Vector3 targetPosition; // vị trí muốn đi tới

    private void Start()
    {
        grid = FindObjectOfType<GridManager>();
    }

    private void Update()
    {
        if (isDragging)
        {
            // Di chuyển mượt
            transform.position = Vector3.Lerp(transform.position, targetPosition, 15f * Time.deltaTime);
        }
    }

    private void OnMouseDown()
    {
        isDragging = true;
        originalPosition = transform.position;
        originalGridPos = CurrentOrigin;
        dragOffset = transform.position - GetMouseWorldByRay();

        originalZ = transform.position.z;

        // Gỡ occupied để không chặn chính mình
        foreach (var offset in occupiedOffsets)
        {
            Vector2Int pos = originalGridPos + offset;
            if (grid.IsValid(pos)) grid.SetOccupied(pos, false);
        }

        Vector3 raised = transform.position;
        raised.z = dragZ;
        transform.position = raised;
    }

    private void OnMouseDrag()
    {
        if (!isDragging) return;

        Vector3 mouseWorld = GetMouseWorldByRay() + dragOffset;
        mouseWorld.z = dragZ;

        Vector2Int snappedGrid = grid.WorldToGrid(mouseWorld);

        hasPreview = false;

        if (!grid.IsValid(snappedGrid) || !grid.CanPlaceBlock(snappedGrid, this))
        {
            if (grid.TryFindNearestValidPlacement(this, out Vector2Int nearest))
            {
                previewOrigin = nearest;
                hasPreview = true;
            }
        }
        else
        {
            previewOrigin = snappedGrid;
            hasPreview = true;
        }

        grid.ClearAllPreviews();

        if (hasPreview)
        {
            bool valid = grid.CanPlaceBlock(previewOrigin, this);
            grid.SetPreviewAt(previewOrigin, this, valid);

            targetPosition = grid.GridToWorld(previewOrigin) + new Vector3(0, 0, dragZ); // smooth đến vị trí snap
        }
        else
        {
            targetPosition = mouseWorld; // không hợp lệ → theo chuột tự do
        }
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
            // Trả lại vị trí cũ
            transform.position = originalPosition;
            CurrentOrigin = originalGridPos;

            foreach (var offset in occupiedOffsets)
            {
                Vector2Int pos = originalGridPos + offset;
                if (grid.IsValid(pos)) grid.SetOccupied(pos, true);
            }
        }

        Vector3 newPos = transform.position;
        newPos.z = originalZ;
        transform.position = newPos;
    }

    private Vector3 GetMouseWorldByRay()
    {
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (plane.Raycast(ray, out float distance))
        {
            return ray.GetPoint(distance);
        }
        return Vector3.zero;
    }
}
