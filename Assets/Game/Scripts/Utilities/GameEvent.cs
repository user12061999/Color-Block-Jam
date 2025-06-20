using HAVIGAME;
using System;

public static class GameEvent {
    public class PlayerInventoryChanged : IEventArgs {
        public ItemContainer Inventory { get; private set; }
        public ItemStack ItemStackChange { get; private set; }
        public bool IsAdded { get; private set; }

        public PlayerInventoryChanged(ItemContainer inventory, ItemStack itemStackChange, bool isAdded) {
            Inventory = inventory;
            ItemStackChange = itemStackChange;
            IsAdded = isAdded;
        }
    }

    public class OnDailyLogin : IEventArgs {
        public DateTime DateTime { get; private set; }

        public OnDailyLogin(DateTime dateTime) {
            DateTime = dateTime;
        }
    }

    public class MissionCompleted : IEventArgs {
        public string MissionId { get; private set; }

        public MissionCompleted(string missionId) {
            MissionId = missionId;
        }
    }
}
