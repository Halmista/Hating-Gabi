
using Photon.Pun;
using UnityEngine;

public class BreakableWall : MonoBehaviourPun
{
    [Header("Wall Settings")]
    public bool isDestroyed;

    public void DestroyWall()
    {
        if (isDestroyed)
            return;

        if (!PhotonNetwork.InRoom)
        {
            Destroy(gameObject);
            return;
        }

        // Only the owning player requests destruction.
        photonView.RPC(
            nameof(RPC_DestroyWall),
            RpcTarget.AllBuffered
        );
    }

    [PunRPC]
    private void RPC_DestroyWall()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;
        Destroy(gameObject);
    }
}
