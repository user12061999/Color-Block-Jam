using UnityEngine;

public class Cell : MonoBehaviour
{
    public Vector2Int Position { get; private set; }
    public bool IsOccupied { get; private set; }
    public Material baseMaterial,hightlightMaterial, redMaterial, greenMaterial;
    public MeshRenderer meshRenderer;

    public void Init(Vector2Int position)
    {
        Position = position;
        IsOccupied = false;
    }

    public void SetOccupied(bool value)
    {
        IsOccupied = value;
    }
    public enum CellPreviewState { None, Valid, Invalid }

    private CellPreviewState previewState = CellPreviewState.None;

    public void SetPreview(CellPreviewState state)
    {
        previewState = state;
        UpdateVisual();
    }

    public void ClearPreview()
    {
        SetPreview(CellPreviewState.None);
    }

    private void UpdateVisual()
    {
        // Giả sử bạn có SpriteRenderer
        if (meshRenderer == null) return;

        if (previewState == CellPreviewState.Valid)
            meshRenderer.sharedMaterial= hightlightMaterial; // màu highlight
        else if (previewState == CellPreviewState.Invalid)
            meshRenderer.sharedMaterial= redMaterial; // màu đỏ
        else if (!IsOccupied)
            meshRenderer.sharedMaterial= baseMaterial; // trống
        else
            meshRenderer.sharedMaterial= greenMaterial; // occupied (nhẹ)
    }
}