
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PressurePlateDoor : MonoBehaviourPun
{
    [Header("Door Visuals")]
    public SpriteRenderer[] doorSprites;

    [Header("Door Collision")]
    public Collider2D[] doorColliders;

    private bool isOpen;

    public void SetDoorOpen(bool open)
    {
        if (isOpen == open)
            return;

        // Only the player who owns the pressure plate
        // requests the change. The door broadcasts it.
        photonView.RPC(
            nameof(RPC_SetDoorOpen),
            RpcTarget.All,
            open
        );
    }

    [PunRPC]
    private void RPC_SetDoorOpen(bool open)
    {
        isOpen = open;

        foreach (SpriteRenderer sprite in doorSprites)
        {
            if (sprite != null)
                sprite.enabled = !isOpen;
        }

        foreach (Collider2D col in doorColliders)
        {
            if (col != null)
                col.enabled = !isOpen;
        }

        Debug.Log(isOpen ? "Door opened!" : "Door closed!");
    }
}
