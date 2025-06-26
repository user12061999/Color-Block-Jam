using System;
using DG.Tweening;
using UnityEngine;

public class BlockCutter : MonoBehaviour
{
    [Header("Detect Box Settings")]
    public Vector3 boxCenterOffset = new Vector3(0, 1f, 0);
    public Vector3 boxSize = new Vector3(2, 2, 1);
    public Vector3 boxRotation = Vector3.zero;

    [SerializeField] private LayerMask blockLayer;
    [SerializeField] private GridManager grid;
    [SerializeField] private float checkInterval = 0.2f;

    [Header("Color Cutter Config")]
    public BlockColorData colorData;
    public Renderer renderer;

    [Header("Cutter Pull Settings")]
    public Direction cutterDirection;

    private void Start()
    {
        if (grid == null)
            grid = FindObjectOfType<GridManager>();

        if (renderer != null && colorData != null)
            renderer.material = colorData.material;

        InvokeRepeating(nameof(CheckAndCut), 0f, checkInterval);
    }
    private void OnValidate()
    {
        if (colorData)
        {
            SetColorData();
        }
    }

    [ContextMenu("Set Color Data")]
    public void SetColorData()
    {
        renderer.material = colorData.material;
    }
    private void CheckAndCut()
    {
        Quaternion rotation = Quaternion.Euler(boxRotation);
        Vector3 center = transform.position + rotation * boxCenterOffset;

        Collider[] hits = Physics.OverlapBox(center, boxSize * 0.5f, rotation, blockLayer);

        foreach (var col in hits)
        {
            BlockShape block = col.GetComponentInParent<BlockShape>();
            if (block != null && IsFullyInside(block, center, rotation))
            {
                if (CanCut(block))
                {
                    CutBlock(block);
                }
                else
                {
                    Debug.Log($"❌ Không thể cắt {block.name} vì màu không khớp: {block.colorData.colorType}");
                }
            }
        }
    }

    private bool CanCut(BlockShape block)
    {
        return block != null && block.colorData != null && block.colorData.colorType == colorData.colorType;
    }

    private bool IsFullyInside(BlockShape block, Vector3 boxCenter, Quaternion rotation)
    {
        foreach (var offset in block.occupiedOffsets)
        {
            Vector3 worldPos = grid.GridToWorld(block.CurrentOrigin + offset);
            if (!PointInBox(worldPos, boxCenter, boxSize, rotation)) return false;
            if (IsOccluded(worldPos, block)) return false;
        }
        return true;
    }

    private bool PointInBox(Vector3 point, Vector3 boxCenter, Vector3 boxSize, Quaternion rotation)
    {
        Vector3 localPoint = Quaternion.Inverse(rotation) * (point - boxCenter);
        return Mathf.Abs(localPoint.x) <= boxSize.x / 2f &&
               Mathf.Abs(localPoint.y) <= boxSize.y / 2f &&
               Mathf.Abs(localPoint.z) <= boxSize.z / 2f;
    }

    private bool IsOccluded(Vector3 targetPos, BlockShape self)
    {
        Vector3 cutterPos = transform.position;

        foreach (var offset in self.occupiedOffsets)
        {
            Vector3 worldPos = grid.GridToWorld(self.CurrentOrigin + offset);
            Vector3 direction = cutterDirection.GetVector();
            float distance = 7;

            // Raycast từ từng khối nhỏ của BlockShape
            if (Physics.Raycast(worldPos, direction, out RaycastHit hit, distance, blockLayer))
            {
                BlockShape hitBlock = hit.collider.GetComponentInParent<BlockShape>();
                if (hitBlock != null && hitBlock != self && hitBlock.colorData.colorType != colorData.colorType)
                {
                    Debug.DrawRay(worldPos, direction * distance, Color.red, 1f);
                    return true; // Bị chặn bởi khối khác màu
                }
            }
        }

        return false; // Không bị chặn
    }

    private void CutBlock(BlockShape block)
    {
        Vector3 moveDirection = cutterDirection.GetVector3().normalized;
        float cellSize = grid.cellSize;
        int offset = GetBlockLength(block, cutterDirection);

        // Vị trí bắt đầu là máy cắt
        Vector3 basePosition = transform.position;

        // Tính vị trí đến → chỉ di chuyển theo đúng trục cắt (X hoặc Y)
        Vector3 destination = basePosition + moveDirection * (offset * cellSize + 0.5f);

        // Giữ nguyên trục còn lại
        if (cutterDirection.IsHorizontal)
        {
            destination.y = block.transform.position.y;
        }
        else
        {
            destination.x = block.transform.position.x;
        }

        destination.z = block.transform.position.z;

        // Tween di chuyển
        block.transform.DOMove(destination, 0.35f)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                foreach (var off in block.occupiedOffsets)
                {
                    Vector2Int pos = block.CurrentOrigin + off;
                    if (grid.IsValid(pos)) grid.SetOccupied(pos, false);
                }
                Destroy(block.gameObject);
            });
    }

    private int GetBlockLength(BlockShape block, Direction direction)
    {
        int min = int.MaxValue, max = int.MinValue;

        foreach (var offset in block.occupiedOffsets)
        {
            int val = direction.IsHorizontal ? offset.x : offset.y;
            min = Mathf.Min(min, val);
            max = Mathf.Max(max, val);
        }

        return Mathf.Abs(max - min) + 1;
    }

    private void OnDrawGizmos()
    {
        Quaternion rot = Quaternion.Euler(boxRotation);
        Vector3 center = transform.position + rot * boxCenterOffset;
        Gizmos.color = new Color(1f, 0f, 0f, 0.4f);
        Gizmos.matrix = Matrix4x4.TRS(center, rot, Vector3.one);
        Gizmos.DrawCube(Vector3.zero, boxSize);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(Vector3.zero, boxSize);
    }
}
