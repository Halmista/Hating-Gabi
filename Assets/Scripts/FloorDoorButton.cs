
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

public class FloorDoorButton : MonoBehaviourPun
{
    [Header("Door")]
    public PressurePlateDoor targetDoor;

    [Header("Interaction")]
    public float interactionRange = 1.5f;

    private UpperHalfController nearbyUpper;
    private bool isOpen;

    private void Update()
    {
        if (nearbyUpper == null)
            return;

        PhotonView upperView =
            nearbyUpper.GetComponent<PhotonView>();

        // Only the player controlling the Upper Half can interact.
        if (upperView == null || !upperView.IsMine)
            return;

        if (Keyboard.current == null)
            return;

        if (!Keyboard.current.eKey.wasPressedThisFrame)
            return;

        float distance = Vector2.Distance(
            nearbyUpper.transform.position,
            transform.position
        );

        if (distance > interactionRange)
            return;

        photonView.RPC(
            nameof(RPC_ToggleDoor),
            RpcTarget.All
        );
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        UpperHalfController upper =
            other.GetComponentInParent<UpperHalfController>();

        if (upper != null)
            nearbyUpper = upper;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        UpperHalfController upper =
            other.GetComponentInParent<UpperHalfController>();

        if (upper != null && nearbyUpper == upper)
            nearbyUpper = null;
    }

    [PunRPC]
    private void RPC_ToggleDoor()
    {
        if (targetDoor == null)
        {
            Debug.LogError(
                "Floor Door Button: Target Door is not assigned!"
            );
            return;
        }

        isOpen = !isOpen;
        targetDoor.SetDoorOpen(isOpen);

        Debug.Log(
            isOpen ? "Floor door opened." : "Floor door closed."
        );
    }
}
