using HAVIGAME;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemContainer: IEnumerable<ItemStack> {
    [SerializeField] private List<ItemStack> itemStacks;

    public int Count => itemStacks.Count;

    public ItemContainer() {
        itemStacks = new List<ItemStack>();
    }

    public ItemStack[] All() {
        return itemStacks.ToArray();
    }

    public ItemStack Get(int id) {
        foreach (var item in itemStacks) {
            if (item.Id == id) {
                return item;
            }
        }
        return new ItemStack(id, 0);
    }

    public int GetCount(int id) {
        foreach (var item in itemStacks) {
            if (item.Id == id) {
                return item.Amount;
            }
        }

        return 0;
    }

    public bool Has(int id) {
        foreach (var item in itemStacks) {
            if (item.Id == id) {
                return item.Amount > 0;
            }
        }

        return false;
    }

    public void Add(ItemStack[] itemStacks) {
        foreach (var item in itemStacks) {
            Add(item);
        }
    }

    public void Add(IEnumerable<ItemStack> itemStacks) {
        foreach (var item in itemStacks) {
            Add(item);
        }
    }

    public void Add(ItemStack itemStack) {
        for (int i = 0; i < itemStacks.Count; i++) {
            ItemStack currentStack = itemStacks[i];
            if (currentStack.Id == itemStack.Id) {
                currentStack.Stack(itemStack.Amount);
                itemStacks[i] = currentStack;
                OnAdded(itemStack);
                return;
            }
        }

        itemStacks.Add(itemStack);
        OnAdded(itemStack);
    }

    public void Remove(ItemStack[] itemStacks) {
        foreach (var item in itemStacks) {
            Remove(item);
        }
    }

    public void Remove(ItemStack itemStack) {
        for (int i = 0; i < itemStacks.Count; i++) {
            ItemStack currentStack = itemStacks[i];
            if (currentStack.Id == itemStack.Id) {
                currentStack.Destack(itemStack.Amount);
                itemStacks[i] = currentStack;
                OnRemoved(itemStack);
                return;
            }
        }
    }

    public void Clear() {
        itemStacks.Clear();
    }

    public bool IsEnought(ItemStack itemStack) {
        foreach (var item in itemStacks) {
            if (item.Id == itemStack.Id) {
                return item.Amount >= itemStack.Amount;
            }
        }
        return false;
    }

    protected virtual void OnAdded(ItemStack itemStack) {
        Log.Debug("[ItemContainer] +{0} {1}.", itemStack.Amount, itemStack.Id);
    }

    protected virtual void OnRemoved(ItemStack itemStack) {
        Log.Debug("[ItemContainer] -{0} {1}.", itemStack.Amount, itemStack.Id);
    }

    protected virtual void OnCleared() {
        Log.Debug("[ItemContainer] Clear.");
    }

    public IEnumerator<ItemStack> GetEnumerator() {
        return itemStacks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return itemStacks.GetEnumerator();
    }
}

public static class ItemContainerExtension {
    public static void Add(this ItemContainer container, ItemStack[] itemStacks) {
        foreach (var item in itemStacks) {
            container.Add(item);
        }
    }
    public static void Remove(this ItemContainer container, ItemStack[] itemStacks) {
        foreach (var item in itemStacks) {
            container.Remove(item);
        }
    }
}