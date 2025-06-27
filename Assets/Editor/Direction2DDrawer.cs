using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Direction2D))]
public class Direction2DDrawer : PropertyDrawer
{
    private static readonly GUIContent[] directionIcons =
    {
        new GUIContent("▲", "Up"),
        new GUIContent("▼", "Down"),
        new GUIContent("◀", "Left"),
        new GUIContent("▶", "Right")
    };

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        EditorGUI.BeginChangeCheck();

        int currentIndex = property.enumValueIndex;
        int selected = GUI.SelectionGrid(position, currentIndex, directionIcons, 4);

        if (EditorGUI.EndChangeCheck())
        {
            property.enumValueIndex = selected;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 1.5f;
    }
}