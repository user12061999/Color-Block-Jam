using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(BlockColorData), false)]
public class BlockColorDataDrawer : PropertyDrawer
{
    private const float LineHeight = 20f;
    private const float Padding = 4f;
    private const float ColorBoxSize = 16f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Trường hợp là reference field => vẽ UI mặc định để chọn lại asset
        if (property.propertyType == SerializedPropertyType.ObjectReference)
        {
            EditorGUI.PropertyField(position, property, label);
            EditorGUI.EndProperty();
            return;
        }

        // Nếu đã có reference, vẽ custom drawer
        var obj = property.objectReferenceValue as BlockColorData;
        if (obj == null)
        {
            EditorGUI.LabelField(position, "Invalid BlockColorData");
            EditorGUI.EndProperty();
            return;
        }

        // Tiêu đề chính
        Rect titleRect = new Rect(position.x, position.y, position.width, LineHeight);
        EditorGUI.LabelField(titleRect, label.text, EditorStyles.boldLabel);

        // Dòng 1: [Màu] Tên màu
        Rect colorNameRect = new Rect(position.x, position.y + LineHeight + Padding, position.width, LineHeight);
        DrawColorAndName(colorNameRect, obj);

        // Dòng 2: Material field
        Rect matRect = new Rect(position.x, position.y + LineHeight * 2 + Padding * 2, position.width, LineHeight);
        DrawMaterialField(matRect, obj);
        
        EditorGUI.EndProperty();
    }

    private void DrawColorAndName(Rect rect, BlockColorData data)
    {
        Rect colorRect = new Rect(rect.x, rect.y, ColorBoxSize, ColorBoxSize);
        Rect nameRect = new Rect(colorRect.xMax + Padding, rect.y, rect.width - ColorBoxSize - Padding, LineHeight);

        EditorGUI.DrawRect(colorRect, data.displayColor);
        EditorGUI.LabelField(nameRect, data.colorType.ToString(), EditorStyles.boldLabel);
    }

    private void DrawMaterialField(Rect rect, BlockColorData data)
    {
        EditorGUI.ObjectField(rect, "Material", data.material, typeof(Material), false);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return LineHeight * 3 + Padding * 2;
    }
}