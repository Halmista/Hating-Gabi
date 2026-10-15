
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    private Dictionary<string, int> items =
        new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddItem(string itemId, int amount = 1)
    {
        if (string.IsNullOrWhiteSpace(itemId) || amount <= 0)
            return;

        if (!items.ContainsKey(itemId))
            items[itemId] = 0;

        items[itemId] += amount;

        Debug.Log("Inventory: " + itemId
            + " x" + items[itemId]);
    }

    public bool HasItem(string itemId)
    {
        return items.ContainsKey(itemId)
            && items[itemId] > 0;
    }

    public bool RemoveItem(string itemId, int amount = 1)
    {
        if (!HasItem(itemId) || amount <= 0)
            return false;

        if (items[itemId] < amount)
            return false;

        items[itemId] -= amount;

        if (items[itemId] <= 0)
            items.Remove(itemId);

        Debug.Log("Used " + itemId);
        return true;
    }

    public int GetItemCount(string itemId)
    {
        return items.TryGetValue(itemId, out int count)
            ? count
            : 0;
    }
}
