
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerHalfRespawn : MonoBehaviour
{
    private PhotonView photonView;
    private Rigidbody2D rb;

    private Vector3 originalSpawnPosition;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();

        originalSpawnPosition = transform.position;
    }

    public void SetOriginalSpawn(Vector3 position)
    {
        originalSpawnPosition = position;
    }

    public void RespawnAt(Transform respawnPoint)
    {
        if (respawnPoint == null)
            return;

        RespawnAtPosition(respawnPoint.position);
    }

    public void RespawnAtOriginalSpawn()
    {
        RespawnAtPosition(originalSpawnPosition);
    }

    private void RespawnAtPosition(Vector3 position)
    {
        if (!photonView.IsMine)
            return;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = position;
        }

        transform.position = position;
    }


    public void RespawnAtPositionFromTrap(Vector2 position)
    {
        if (photonView == null || !photonView.IsMine)
            return;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = position;
        }

        transform.position = position;
    }

}
