using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GridManager grid = (GridManager)target;

        if (grid.shape == null)
            return;

        grid.shape.EnsureInitialized();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Grid Layout Editor", EditorStyles.boldLabel);

        int width = grid.shape.width;
        int height = grid.shape.height;

        for (int y = height - 1; y >= 0; y--)
        {
            EditorGUILayout.BeginHorizontal();
            for (int x = 0; x < width; x++)
            {
                bool current = grid.shape.Get(x, y);
                bool changed = GUILayout.Toggle(current, "", GUILayout.Width(20), GUILayout.Height(20));

                if (changed != current)
                {
                    Undo.RecordObject(grid, "Toggle Cell");
                    grid.shape.Set(x, y, changed);
                    EditorUtility.SetDirty(grid);
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Resize Grid"))
        {
            Undo.RecordObject(grid, "Resize Grid");
            grid.shape.Resize(grid.shape.width, grid.shape.height);
            EditorUtility.SetDirty(grid);
        }

        if (GUILayout.Button("Clear All"))
        {
            Undo.RecordObject(grid, "Clear Grid");
            grid.shape.Resize(grid.shape.width, grid.shape.height);
            EditorUtility.SetDirty(grid);
        }
        if (GUILayout.Button("Tick All"))
        {
            for (int y = height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int x = 0; x < width; x++)
                {
                    bool changed = GUILayout.Toggle(true, "", GUILayout.Width(20), GUILayout.Height(20));


                    grid.shape.Set(x, y, changed);
                    EditorUtility.SetDirty(grid);
                }

                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndHorizontal();

        // ✅ Nút Create Grid
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Operations", EditorStyles.boldLabel);
        if (GUILayout.Button("Clear Grid"))
        {
            GridManager gridManager = (GridManager)target;
            Undo.RecordObject(gridManager, "Clear Grid");
            gridManager.ClearGrid();
            EditorUtility.SetDirty(gridManager);
        }
        if (GUILayout.Button("Create Grid"))
        {
            GridManager gridManager = (GridManager)target;
            Undo.RecordObject(gridManager, "Create Grid");
            gridManager.GenerateGrid();
            EditorUtility.SetDirty(gridManager);
        }
    }

}