using HAVIGAME;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "New Item", menuName = "Game/Data/Item")]
public class Item : ScriptableObject, IIdentify<int> {
    [SerializeField, ConstantField(typeof(ItemID))] private int id;
    [SerializeField] private ItemType type = ItemType.NonConsumable;
    [SerializeField, SpriteField] private Sprite icon;
    [SerializeField] private string displayName;
    [SerializeField, TextArea(3,5)] private string description;
    [SerializeField] private ItemStack price;

    public int Id => id;
    public ItemType Type => type;
    public Sprite Icon => icon;
    public string Name => displayName;
    public string Description => description;
    public ItemStack Price => price;

    public virtual void Use() { }
    public virtual void Equip() { }
    public virtual void Unequip() { }

#if UNITY_EDITOR
    [CustomEditor(typeof(Item), true)]
    private class ItemEditor : Editor {
        private Item data;

        private void OnEnable() {
            data = (Item)target;
        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height) {
            if (data != null && data.Icon != null) {
                return data.Icon.texture;
            }
            else {
                return base.RenderStaticPreview(assetPath, subAssets, width, height);
            }

        }
    }
#endif
}

[System.Flags]
public enum ItemType {
    None = 0,
    NonConsumable = 1 << 0,
    Consumable = 1 << 1,
    Merchantable = 1 << 2,
    Equipmentable = 1 << 3,
}