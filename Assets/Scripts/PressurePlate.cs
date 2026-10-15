
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Doors")]
    public List<PressurePlateDoor> targetDoors =
        new List<PressurePlateDoor>();

    [Header("Animation")]
    public PressurePlateAnimation plateAnimation;

    private readonly HashSet<Collider2D> lowerHalfColliders =
        new HashSet<Collider2D>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        LowerHalfController lower =
            other.GetComponentInParent<LowerHalfController>();

        if (lower == null)
            return;

        PhotonView playerView = lower.GetComponent<PhotonView>();

        // Only the owning player handles this trigger.
        if (playerView == null || !playerView.IsMine)
            return;

        bool wasEmpty = lowerHalfColliders.Count == 0;

        lowerHalfColliders.Add(other);

        if (wasEmpty && lowerHalfColliders.Count > 0)
        {
            if (plateAnimation != null)
                plateAnimation.SetPressed(true);

            SetAllDoors(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        LowerHalfController lower =
            other.GetComponentInParent<LowerHalfController>();

        if (lower == null)
            return;

        PhotonView playerView = lower.GetComponent<PhotonView>();

        if (playerView == null || !playerView.IsMine)
            return;

        lowerHalfColliders.Remove(other);

        if (lowerHalfColliders.Count == 0)
        {
            if (plateAnimation != null)
                plateAnimation.SetPressed(false);

            SetAllDoors(false);
        }
    }

    private void SetAllDoors(bool open)
    {
        foreach (PressurePlateDoor door in targetDoors)
        {
            if (door != null)
                door.SetDoorOpen(open);
        }
    }
}
