
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("Item Settings")]
    public string itemId = "Key";
    public int amount = 1;

    private SpriteRenderer[] renderers;
    private Collider2D[] colliders;
    private bool collected;

    private void Awake()
    {
        renderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        colliders =
            GetComponentsInChildren<Collider2D>(true);
    }

    public void Collect()
    {
        if (collected)
            return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogError(
                "InventoryManager is missing from the scene!"
            );
            return;
        }

        // Add the item before hiding it.
        InventoryManager.Instance.AddItem(itemId, amount);

        collected = true;

        foreach (SpriteRenderer sr in renderers)
        {
            if (sr != null)
                sr.enabled = false;
        }

        foreach (Collider2D col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }

        Debug.Log("Collected " + itemId);
    }
}
