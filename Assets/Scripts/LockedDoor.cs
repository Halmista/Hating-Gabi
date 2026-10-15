
using System.Collections;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class LockedDoor : MonoBehaviourPun
{
    [Header("Door Settings")]
    public string requiredItemId = "Key";
    public bool consumeKey = true;

    [Header("Disappearance Animation")]
    public float disappearDuration = 0.5f;

    [Header("Next Door")]
    public LevelExitDoor nextDoor;

    private SpriteRenderer[] renderers;
    private Collider2D[] colliders;
    private bool isOpening;
    private bool isOpen;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        colliders = GetComponentsInChildren<Collider2D>(true);
    }

    public void TryOpen()
    {
        if (isOpening || isOpen)
            return;

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("InventoryManager not found!");
            return;
        }

        InventoryManager inventory = InventoryManager.Instance;

        if (!inventory.HasItem(requiredItemId))
        {
            Debug.Log("The door is locked. You need: " + requiredItemId);
            return;
        }

        if (consumeKey && !inventory.RemoveItem(requiredItemId))
            return;

        photonView.RPC(nameof(RPC_BeginOpening), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_BeginOpening()
    {
        if (isOpening || isOpen)
            return;

        isOpening = true;

        // Allow the players to pass through immediately.
        foreach (Collider2D col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }

        StartCoroutine(DisappearDoor());
    }

    private IEnumerator DisappearDoor()
    {
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, disappearDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - Mathf.Clamp01(elapsed / duration);

            foreach (SpriteRenderer sr in renderers)
            {
                if (sr != null)
                {
                    Color color = sr.color;
                    color.a = alpha;
                    sr.color = color;
                }
            }

            yield return null;
        }

        // Completely hide the elevator door.
        foreach (SpriteRenderer sr in renderers)
        {
            if (sr != null)
                sr.enabled = false;
        }

        isOpening = false;
        isOpen = true;

        // Door 2 becomes usable only after Door 1 disappears.
        if (nextDoor != null && PhotonNetwork.IsMasterClient)
        {
            nextDoor.SetUnlocked(true);
        }

        Debug.Log("Elevator door disappeared!");
    }
}
