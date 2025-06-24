using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BlockColorData), true)]
public class BlockColorDataDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty objectRef = property;

        if (objectRef.objectReferenceValue == null)
        {
            EditorGUI.PropertyField(position, objectRef, label);
            EditorGUI.EndProperty();
            return;
        }

        BlockColorData data = objectRef.objectReferenceValue as BlockColorData;
        if (data == null)
        {
            EditorGUI.LabelField(position, "Invalid BlockColorData reference");
            EditorGUI.EndProperty();
            return;
        }

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float padding = 4f;

        // Draw label
        EditorGUI.LabelField(new Rect(position.x, position.y, position.width, lineHeight), label);

        // Draw color + name
        Rect colorRect = new Rect(position.x + padding, position.y + lineHeight + padding, lineHeight, lineHeight);
        EditorGUI.DrawRect(colorRect, data.displayColor);

        Rect nameRect = new Rect(colorRect.xMax + padding, colorRect.y, position.width - lineHeight - 3 * padding, lineHeight);
        EditorGUI.LabelField(nameRect, data.colorType.ToString(), EditorStyles.boldLabel);

        // Draw material field
        Rect matRect = new Rect(position.x + padding, colorRect.yMax + padding, position.width - 2 * padding, lineHeight);
        EditorGUI.ObjectField(matRect, "Material", data.material, typeof(Material), false);

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 3 + 10f;
    }
}
