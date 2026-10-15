
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PhotonView))]
public class NetworkInteractable : MonoBehaviour
{
    [Header("Interaction Event")]
    public UnityEvent onInteract;

    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void Interact()
    {
        photonView.RPC(
            nameof(RPC_Interact),
            RpcTarget.All
        );
    }

    [PunRPC]
    private void RPC_Interact()
    {
        onInteract?.Invoke();
    }
}