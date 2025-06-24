using UnityEngine;

[CreateAssetMenu(menuName = "Block/Color Data", fileName = "BlockColorData")]
public class BlockColorData : ScriptableObject
{
    public ColorType colorType;
    public Color displayColor;
    public Material material;
}
public enum ColorType
{
    Red,
    Green,
    Blue,
    Yellow,
    Purple,
    Orange,
    // Thêm màu mới ở đây
}