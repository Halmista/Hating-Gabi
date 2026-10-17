
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PressurePlateProjectile : MonoBehaviourPun
{
    [Header("Projectile Settings")]
    public float speed = 8f;
    public float lifetime = 5f;

    private Vector2 direction = Vector2.left;
    private Vector2 respawnPosition;
    private bool hasRespawnPoint;
    private bool initialized;
    private bool hasHit;
    private float elapsedTime;

    public void Initialize(
        Vector2 launchDirection,
        Vector2 targetRespawnPosition,
        bool useRespawnPoint)
    {
        if (!photonView.IsMine)
            return;

        float xDirection =
            launchDirection.x >= 0f ? 1f : -1f;

        photonView.RPC(
            nameof(RPC_Initialize),
            RpcTarget.All,
            xDirection,
            targetRespawnPosition.x,
            targetRespawnPosition.y,
            useRespawnPoint
        );
    }

    [PunRPC]
    private void RPC_Initialize(
        float xDirection,
        float respawnX,
        float respawnY,
        bool useRespawnPoint)
    {
        direction = new Vector2(xDirection, 0f);

        respawnPosition = new Vector2(respawnX, respawnY);
        hasRespawnPoint = useRespawnPoint;

        initialized = true;

        transform.rotation = Quaternion.identity;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * xDirection;
        transform.localScale = scale;
    }

    private void Update()
    {
        if (!initialized || hasHit)
            return;

        transform.position +=
            (Vector3)(speed * Time.deltaTime * direction);

        elapsedTime += Time.deltaTime;

        if (elapsedTime >= lifetime && photonView.IsMine)
        {
            hasHit = true;
            PhotonNetwork.Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!photonView.IsMine || hasHit)
            return;

        LowerHalfController lower =
            other.GetComponentInParent<LowerHalfController>();

        if (lower != null)
        {
            PhotonView lowerView = lower.GetComponent<PhotonView>();

            if (lowerView != null && lowerView.IsMine)
            {
                hasHit = true;

                PlayerHalfRespawn respawn =
                    lower.GetComponent<PlayerHalfRespawn>();

                if (respawn != null)
                {
                    if (hasRespawnPoint)
                    {
                        respawn.RespawnAtPositionFromTrap(
                            respawnPosition
                        );
                    }
                    else
                    {
                        respawn.RespawnAtOriginalSpawn();
                    }
                }
                else
                {
                    Debug.LogError(
                        "Lower Half is missing PlayerHalfRespawn!"
                    );
                }

                PhotonNetwork.Destroy(gameObject);
            }

            return;
        }

        hasHit = true;
        PhotonNetwork.Destroy(gameObject);
    }
}
