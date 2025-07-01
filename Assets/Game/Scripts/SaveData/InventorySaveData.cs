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

    protected override void OnAdded(ItemStack itemStack, string placement=null, bool raiseEventTracking = true) {
        base.OnAdded(itemStack, placement, raiseEventTracking);

        isChanged = true;

        if (raiseEventTracking) {
            ItemData data = ItemDatabase.Instance.GetDataById(itemStack.Id);
            string name = data.name.Replace(" ", "_").ToLower();

            //SetCurrentProperties(itemStack);

            GameAnalytics.LogEvent(GameAnalytics.GameEvent.Create("resource_earn")
                .Add(name, itemStack.Amount)
                .Add("position", placement));
        }

        EventDispatcher.Dispatch(new GameEvent.PlayerInventoryChanged(this, itemStack, true));
    }

    protected override void OnAdded(ItemStack[] itemStacks, string placement=null, bool raiseEventTracking = true) {
        base.OnAdded(itemStacks, placement, raiseEventTracking);

        isChanged = true;

        if (raiseEventTracking) {
            GameAnalytics.GameEvent gameEvent = GameAnalytics.GameEvent.Create("resource_earn").Add("position", placement);

            foreach (var itemStack in itemStacks) {

                ItemData data = ItemDatabase.Instance.GetDataById(itemStack.Id);

                string name = data.name.Replace(" ", "_").ToLower();

                //SetCurrentProperties(itemStack);

                gameEvent.Add($"resouce_{name}", itemStack.Amount.ToString());
            }

            GameAnalytics.LogEvent(gameEvent);
        }

        foreach (var itemStack in itemStacks) {
            EventDispatcher.Dispatch(new GameEvent.PlayerInventoryChanged(this, itemStack, true));
        }
    }

    protected override void OnRemoved(ItemStack itemStack, string placement=null, bool raiseEventTracking = true) {
        base.OnRemoved(itemStack, placement, raiseEventTracking);

        isChanged = true;

        if (raiseEventTracking) {
            ItemData data = ItemDatabase.Instance.GetDataById(itemStack.Id);
            string name = data.name.Replace(" ", "_").ToLower();

            //SetCurrentProperties(itemStack);

            GameAnalytics.LogEvent(GameAnalytics.GameEvent.Create("resource_spend")
                .Add(name, itemStack.Amount)
                .Add("position", placement));
        }

        EventDispatcher.Dispatch(new GameEvent.PlayerInventoryChanged(this, itemStack, false));
    }

    protected override void OnRemoved(ItemStack[] itemStacks, string placement=null, bool raiseEventTracking = true) {
        base.OnAdded(itemStacks, placement, raiseEventTracking);

        isChanged = true;

        if (raiseEventTracking) {
            GameAnalytics.GameEvent gameEvent = GameAnalytics.GameEvent.Create("resource_spend").Add("position", placement);

            foreach (var itemStack in itemStacks) {

                ItemData data = ItemDatabase.Instance.GetDataById(itemStack.Id);

                string name = data.name.Replace(" ", "_").ToLower();

                //SetCurrentProperties(itemStack);

                gameEvent.Add($"resouce_{name}", itemStack.Amount.ToString());
            }

            GameAnalytics.LogEvent(gameEvent);
        }

        foreach (var itemStack in itemStacks) {
            EventDispatcher.Dispatch(new GameEvent.PlayerInventoryChanged(this, itemStack, false));
        }
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
