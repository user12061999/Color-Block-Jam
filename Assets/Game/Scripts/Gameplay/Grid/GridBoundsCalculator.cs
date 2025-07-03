using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class GridBoundsCalculator : MonoBehaviour
{
    [SerializeField] private GridManager gridManager;
    [SerializeField] private bool calculateOnStart = true;

    public Bounds gridBounds;

    private void Awake()
    {
        if (calculateOnStart)
            CalculateBoundsFromValidCells();
    }

    [ContextMenu("Calculate Grid Bounds From Valid Cells")]
    public void CalculateBoundsFromValidCells()
    {
        if (gridManager == null)
        {
            Debug.LogError("GridManager not assigned.");
            return;
        }

        List<Vector2Int> validCells = new List<Vector2Int>(gridManager.shape.ToVector2IntList());
        float cellSize = gridManager.cellSize;

        if (validCells.Count == 0)
        {
            Debug.LogWarning("No valid cells in grid.");
            return;
        }

        Vector2Int min = validCells[0];
        Vector2Int max = validCells[0];

        foreach (var cell in validCells)
        {
            min = Vector2Int.Min(min, cell);
            max = Vector2Int.Max(max, cell);
        }

        // Correct world space bounds
        Vector2 minWorld = new Vector2(min.x * cellSize, min.y * cellSize);
        Vector2 maxWorld = new Vector2(max.x * cellSize , max.y * cellSize); // include full last cell

        Vector2 size = maxWorld - minWorld;
        Vector2 center = minWorld + size / 2f;

        gridBounds = new Bounds(center, size+ new Vector2(cellSize, cellSize)); // include full last cell
        Debug.Log($"✅ Grid Bounds: Center={cellSize}, Size={size} min={min}, max={max} minWorld={minWorld}, maxWorld={maxWorld}");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f); // orange
        Gizmos.DrawCube(gridBounds.center, gridBounds.size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(gridBounds.center, gridBounds.size);
    }
}