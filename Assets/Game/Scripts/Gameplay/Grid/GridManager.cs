using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour
{
    [Header("Prefab Cell")]
    [SerializeField] private Cell cellPrefab;
    [Header("Grid Shape Editor")]
    [SerializeField]
    public SerializableBoolGrid shape = new SerializableBoolGrid();

    [Header("Vị trí hợp lệ của các cell (Vector2Int)")]
    [SerializeField] private List<Vector2Int> validCellPositions;

    [Header("Grid Holder")]
    [SerializeField] private Transform cellParent;
    [SerializeField] public Transform CellParent => cellParent;
    [SerializeField] public Transform BlockParent => blockParent;

    [Header("Kích thước mỗi ô")]
    [SerializeField]
    public float cellSize = 2f;

    private Dictionary<Vector2Int, Cell> cellMap = new Dictionary<Vector2Int, Cell>();

    [Header("Cấu hình spawn")]
    [SerializeField] private List<BlockShape> blockPrefabs;
    [SerializeField] private int numberToSpawn = 5;
    [SerializeField] private Transform blockParent;

    private void Awake()
    {
        //shape.EnsureInitialized();
        GenerateGrid();
    }
    

    private void Start()
    {
        
    }

    public void SetPreviewAt(Vector2Int origin, BlockShape block, bool valid)
    {
        foreach (var offset in block.occupiedOffsets)
        {
            Vector2Int pos = origin + offset;
            if (cellMap.TryGetValue(pos, out var cell))
            {
                cell.SetPreview(valid ? Cell.CellPreviewState.Valid : Cell.CellPreviewState.Invalid);
            }
        }
    }

    public void SpawnSubBlock(BlockShape blockShape)
    {
        if (blockShape.SubBlockColorData)
        {
            Instantiate(blockShape, blockParent).SetColorData(
                blockShape.SubBlockColorData);
            PlaceBlock(blockShape.PreviewOrigin, blockShape);
        }
    }

    public void OnCompleteCutBlock(Action callBack=null)
    {
        callBack?.Invoke();
    }
    public void ClearAllPreviews()
    {
        foreach (var cell in cellMap.Values)
        {
            cell.ClearPreview();
        }
    }
    public void SetOccupied(Vector2Int pos, bool value)
    {
        if (cellMap.TryGetValue(pos, out var cell))
        {
            cell.SetOccupied(value);
        }
    }
    public void ClearGrid()
    {
        foreach (Transform child in cellParent) DestroyImmediate(child.gameObject);
        cellMap.Clear();
    }
    public void GenerateGrid()
    {
        validCellPositions = shape.ToVector2IntList();


        foreach (var pos in validCellPositions)
        {
            Vector3 worldPos = new Vector3(pos.x * cellSize, pos.y * cellSize, 0f);
            Cell cell = Instantiate(cellPrefab, worldPos, Quaternion.identity, cellParent);
            cell.Init(pos);
            cellMap[pos] = cell;
        }
    }

    public bool IsValid(Vector2Int pos) => cellMap.ContainsKey(pos);
    public bool IsOccupied(Vector2Int pos) => cellMap.TryGetValue(pos, out var cell) && cell.IsOccupied;

    public bool CanPlaceBlock(Vector2Int origin, BlockShape shape)
    {
        foreach (var offset in shape.occupiedOffsets)
        {
            Vector2Int pos = origin + offset;
            if (!IsValid(pos) || IsOccupied(pos)) return false;
        }
        return true;
    }
    public bool TryFindNearestValidPlacement(BlockShape block, out Vector2Int bestPos)
    {
        bestPos = Vector2Int.zero;
        float bestDist = float.MaxValue;
        bool found = false;

        foreach (var cellPos in cellMap.Keys)
        {
            if (CanPlaceBlock(cellPos, block))
            {
                float dist = Vector2.Distance(GridToWorld(cellPos), block.transform.position);
                if (dist < bestDist)
                {
                    bestDist = dist;
                    bestPos = cellPos;
                    found = true;
                }
            }
        }

        return found;
    }
    public void PlaceBlock(Vector2Int origin, BlockShape shape)
    {

        foreach (var offset in shape.occupiedOffsets)
        {
            Vector2Int pos = origin + offset;
            if (cellMap.TryGetValue(pos, out var cell))
                cell.SetOccupied(true);
        }
        Debug.Log($"Placing block at {origin} with offsets: {string.Join(", ", shape.occupiedOffsets)}");
        Vector3 worldPos = GridToWorld(origin); // Không trừ center nữa
        shape.transform.position = worldPos;
        shape.CurrentOrigin = origin;
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * cellSize, gridPos.y * cellSize, 0f);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / cellSize);
        int y = Mathf.RoundToInt(worldPos.y / cellSize);
        return new Vector2Int(x, y);
    }
    private void OnDrawGizmosSelected()
    {
        if (shape == null)
            return;

        shape.EnsureInitialized();

        Gizmos.color = Color.green;

        for (int y = 0; y < shape.height; y++)
        {
            for (int x = 0; x < shape.width; x++)
            {
                if (shape.Get(x, y))
                {
                    Vector3 pos = GridToWorld(new Vector2Int(x, y));
                    Gizmos.DrawWireCube(pos, Vector3.one * cellSize * 0.9f);
                }
            }
        }

        if (shape == null || cellMap == null) return;


        foreach (var pair in cellMap)
        {
            Vector2Int pos = pair.Key;
            Cell cell = pair.Value;
            Vector3 worldPos = GridToWorld(pos);

            if (cell.IsOccupied)
            {
                Gizmos.color = new Color(1f, 0f, 0f, 0.4f); // đỏ nhạt
                Gizmos.DrawCube(worldPos, Vector3.one * cellSize * 0.9f);
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(worldPos, Vector3.one * cellSize * 0.9f);
            }
        }
    }
    public bool MoveBlockTo(BlockShape block, Vector2Int newOrigin)
    {
        // Gỡ occupied cũ
        foreach (var offset in block.occupiedOffsets)
        {
            Vector2Int pos = block.CurrentOrigin + offset;
            if (cellMap.TryGetValue(pos, out var cell))
                cell.SetOccupied(false);
        }

        // Kiểm tra vị trí mới
        foreach (var offset in block.occupiedOffsets)
        {
            Vector2Int pos = newOrigin + offset;
            if (!IsValid(pos) || IsOccupied(pos))
            {
                // Re-mark lại vị trí cũ nếu không thể di chuyển
                foreach (var offset2 in block.occupiedOffsets)
                {
                    Vector2Int pos2 = block.CurrentOrigin + offset2;
                    if (cellMap.TryGetValue(pos2, out var cell2))
                        cell2.SetOccupied(true);
                }

                return false;
            }
        }

        // Mark vị trí mới
        foreach (var offset in block.occupiedOffsets)
        {
            Vector2Int pos = newOrigin + offset;
            if (cellMap.TryGetValue(pos, out var cell))
                cell.SetOccupied(true);
        }

        // Cập nhật transform
        Vector3 worldPos = GridToWorld(newOrigin);
        block.transform.position = worldPos;
        block.CurrentOrigin = newOrigin;

        return true;
    }
    public IEnumerable<Vector2Int> GetAllValidCells()
    {
        return cellMap.Keys;
    }

    [ContextMenu("Spawn One Blocks")]
    public void SpawnOneBlocksOnGrid()
    {
        List<Vector2Int> possibleOrigins = new List<Vector2Int>(GetAllValidCells());

        int attemptsPerBlock = 100;

        numberToSpawn = 1;
        for (int i = 0; i < numberToSpawn; i++)
        {
            BlockShape prefab = blockPrefabs[Random.Range(0, blockPrefabs.Count)];
            bool placed = false;

            for (int attempt = 0; attempt < attemptsPerBlock; attempt++)
            {
                Vector2Int origin = possibleOrigins[Random.Range(0, possibleOrigins.Count)];

                if (CanPlaceBlock(origin, prefab))
                {
                    BlockShape newBlock = Instantiate(prefab, blockParent);
                    PlaceBlock(origin, newBlock); // Tự căn giữa và mark occupied
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"Không thể đặt block #{i} - Không đủ chỗ.");
            }
        }
    }
    [ContextMenu("Spawn Blocks")]
    public void SpawnBlocksOnGrid()
    {
        List<Vector2Int> possibleOrigins = new List<Vector2Int>(GetAllValidCells());

        int attemptsPerBlock = 100;

        for (int i = 0; i < numberToSpawn; i++)
        {
            BlockShape prefab = blockPrefabs[Random.Range(0, blockPrefabs.Count)];
            bool placed = false;

            for (int attempt = 0; attempt < attemptsPerBlock; attempt++)
            {
                Vector2Int origin = possibleOrigins[Random.Range(0, possibleOrigins.Count)];

                if (CanPlaceBlock(origin, prefab))
                {
                    BlockShape newBlock = Instantiate(prefab, blockParent);
                    newBlock.SetColorData(ColorConfig.Instance.blockColorDatas[Random.Range(0, ColorConfig.Instance.blockColorDatas.Length)]);
                    PlaceBlock(origin, newBlock); // Tự căn giữa và mark occupied
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"Không thể đặt block #{i} - Không đủ chỗ.");
            }
        }
    }
    [ContextMenu("Set Occupied")]
    public void SetOrccupied()
    {
        foreach (Transform VARIABLE in blockParent)
        {
            Vector2Int origin = WorldToGrid(VARIABLE.transform.position);
            PlaceBlock(origin,VARIABLE.GetComponent<BlockShape>());
        }
    }
}

