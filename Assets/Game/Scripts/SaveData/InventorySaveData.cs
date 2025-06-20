using HAVIGAME;
using HAVIGAME.SaveLoad;

[System.Serializable]
public class InventorySaveData : ItemContainer, ISaveData {
    [System.NonSerialized] private bool isChanged;

    public bool IsChanged => isChanged;

    public InventorySaveData() : base() {
        Add(ConfigDatabase.Instance.DefaultInventory);

        isChanged = false;
    }

    public void Dispose() {

    }

    public void OnAfterLoad() {

    }

    public void OnBeforeSave() {
        isChanged = false;
    }

    protected override void OnAdded(ItemStack itemStack) {
        base.OnAdded(itemStack);

        isChanged = true;

        EventDispatcher.Dispatch(new GameEvent.PlayerInventoryChanged(this, itemStack, true));
    }

    protected override void OnRemoved(ItemStack itemStack) {
        base.OnRemoved(itemStack);

        isChanged = true;

        EventDispatcher.Dispatch(new GameEvent.PlayerInventoryChanged(this, itemStack, false));
    }

    protected override void OnCleared() {
        base.OnCleared();

        isChanged = true;
    }

    public void OnCreate() {

    }

    public void OnDestroy() {

    }
}
