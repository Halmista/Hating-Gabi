
using System.Collections;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody2D))]
public class SecurityGuardObstacle : MonoBehaviourPun
{
    [Header("Guard Visual")]
    public SpriteRenderer guardSprite;
    public bool spriteFacesRightByDefault = true;
    public Transform detectionRange;
    public float lookInterval = 2f;

    [Header("Spawn Points")]
    public Transform lowerHalfSpawn;
    public Transform upperHalfSpawn;

    [Header("Respawn Point")]
    public Transform upperHalfRespawnPoint;

    [Header("Reset")]
    public float resetCooldown = 1.5f;

    private bool facingRight = true;
    private bool resetInProgress;
    private bool resetRequestSent;
    private Vector3 detectionOriginalScale;

    private SpriteRenderer[] detectionSprites;

    private void Awake()
    {
        if (detectionRange != null)
        {
            detectionOriginalScale = detectionRange.localScale;

            detectionSprites =
                detectionRange.GetComponentsInChildren<SpriteRenderer>(true);
        }
    }

    private void Start()
    {
        // The Upper Half player alone sees the detection cone.
        bool showCone =
            PhotonNetwork.InRoom &&
            PhotonNetwork.LocalPlayer.ActorNumber == 2;

        if (detectionSprites != null)
        {
            foreach (SpriteRenderer sr in detectionSprites)
            {
                if (sr != null)
                    sr.enabled = showCone;
            }
        }

        ApplyFacing();

        // The Master Client controls the guard's looking direction.
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(LookLeftAndRight());
    }

    private IEnumerator LookLeftAndRight()
    {
        while (true)
        {
            yield return new WaitForSeconds(
                Mathf.Max(0.1f, lookInterval)
            );

            facingRight = !facingRight;

            photonView.RPC(
                nameof(RPC_SetFacing),
                RpcTarget.All,
                facingRight
            );
        }
    }

    [PunRPC]
    private void RPC_SetFacing(bool lookRight)
    {
        facingRight = lookRight;
        ApplyFacing();
    }

    private void ApplyFacing()
    {
        if (guardSprite != null)
        {
            // Flip the entire GuardSprite object.
            // Its DetectionRange child will flip too.
            Vector3 scale = guardSprite.transform.localScale;

            float direction = facingRight
                ? (spriteFacesRightByDefault ? 1f : -1f)
                : (spriteFacesRightByDefault ? -1f : 1f);

            scale.x = Mathf.Abs(scale.x) * direction;
            guardSprite.transform.localScale = scale;
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (resetInProgress || resetRequestSent)
            return;

        UpperHalfController upper =
            other.GetComponentInParent<UpperHalfController>();

        if (upper == null)
            return;

        PhotonView playerView = upper.GetComponent<PhotonView>();

        if (playerView == null || !playerView.IsMine)
            return;

        resetRequestSent = true;

        photonView.RPC(
            nameof(RPC_ResetUpperHalf),
            RpcTarget.All
        );
    }



    [PunRPC]
    private void RPC_ResetUpperHalf()
    {
        if (resetInProgress)
            return;

        resetInProgress = true;
        resetRequestSent = true;

        PhotonView[] views =
            FindObjectsByType<PhotonView>(
                FindObjectsSortMode.None
            );

        foreach (PhotonView view in views)
        {
            if (view == null || !view.IsMine)
                continue;

            UpperHalfController upper =
                view.GetComponent<UpperHalfController>();

            // Ignore the lower half and all other objects.
            if (upper == null)
                continue;

            if (upperHalfRespawnPoint == null)
            {
                Debug.LogError(
                    "Security Guard: Upper Half Respawn Point is not assigned!"
                );
                break;
            }

            PlayerHalfRespawn respawn =
                upper.GetComponent<PlayerHalfRespawn>();

            if (respawn != null)
            {
                respawn.RespawnAt(upperHalfRespawnPoint);
            }
            else
            {
                Rigidbody2D rb = upper.GetComponent<Rigidbody2D>();

                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.position = upperHalfRespawnPoint.position;
                }

                upper.transform.position =
                    upperHalfRespawnPoint.position;
            }

            Debug.Log("Security guard caught the Upper Half!");
            break;
        }

        StopAllCoroutines();
        StartCoroutine(ResetCooldownRoutine());
    }


    private IEnumerator ResetCooldownRoutine()
    {
        yield return new WaitForSeconds(resetCooldown);

        resetInProgress = false;
        resetRequestSent = false;

        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(LookLeftAndRight());
    }
}
