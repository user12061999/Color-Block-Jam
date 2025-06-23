using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField]private Cell cellPrefab;
    [SerializeField]private List<Cell> cells=new List<Cell>();
    [SerializeField]private Transform gridTransform;
    [SerializeField]private Vector2Int gridSize;
    [SerializeField] private int sizeCell;

    [ContextMenu("CreateGrid")]
    public void CreateGrid()
    {
        cells?.Clear();
        foreach (Transform tra in gridTransform)
        {
            Destroy(tra.gameObject);
        }
        
        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                Cell cell = Instantiate(cellPrefab, gridTransform);
                
                cell.transform.localPosition = new Vector3(i * sizeCell, j * sizeCell, 0f);
                cells.Add(cell);
            }
        }
    }
}