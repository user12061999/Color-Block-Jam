using HAVIGAME;
using UnityEngine;

[System.Serializable]
public struct ItemStack {
    [SerializeField, ConstantField(typeof(ItemID))] private int id;
    [SerializeField] private int amount;

    public int Id => id;
    public int Amount => amount;

    public ItemStack(int id, int amount) {
        this.id = id;
        this.amount = amount;
    }

    public void Stack(int amount) {
        this.amount += amount;
    }

    public void Destack(int amount) {
        if (amount >= this.amount) {
            this.amount = 0;
        }
        else {
            this.amount -= amount;
        }
    }
}
