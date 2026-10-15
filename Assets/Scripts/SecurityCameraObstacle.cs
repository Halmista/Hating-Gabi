
using System.Collections;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(PolygonCollider2D))]
public class SecurityCameraObstacle : MonoBehaviourPun
{
    [Header("Visuals")]
    public GameObject detectionConeVisual;

    [Header("Player Spawn Points")]
    public Transform lowerHalfSpawn;
    public Transform upperHalfSpawn;

    [Header("Reset Settings")]
    public float resetCooldown = 1.5f;

    private bool resetInProgress;

    private void Awake()
    {
        PolygonCollider2D zone =
            GetComponent<PolygonCollider2D>();

        zone.isTrigger = true;
    }

    private void Start()
    {
        // Actor 1 controls Lower Half in this prototype.
        // Only that player's screen shows the detection cone.
        if (detectionConeVisual != null &&
            PhotonNetwork.InRoom)
        {
            detectionConeVisual.SetActive(
                PhotonNetwork.LocalPlayer.ActorNumber == 1
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (resetInProgress || !PhotonNetwork.InRoom)
            return;

        LowerHalfController lower =
            other.GetComponentInParent<LowerHalfController>();

        // Only the Lower Half can trigger this obstacle.
        if (lower == null)
            return;

        PhotonView playerView =
            lower.GetComponent<PhotonView>();

        // Only the owning client requests the reset.
        if (playerView == null || !playerView.IsMine)
            return;

        // The Lower Half is safe while connected.
        if (lower.IsConnected)
            return;

        photonView.RPC(
            nameof(RPC_ResetPlayers),
            RpcTarget.All
        );
    }

    [PunRPC]
    private void RPC_ResetPlayers()
    {
        if (resetInProgress)
            return;

        resetInProgress = true;

        // Each client moves its own character.
        // This avoids competing with remote-player synchronization.
        PhotonView[] views =
            FindObjectsByType<PhotonView>(
                FindObjectsSortMode.None
            );

        foreach (PhotonView view in views)
        {
            if (view == null || !view.IsMine)
                continue;

            LowerHalfController lower =
                view.GetComponent<LowerHalfController>();

            UpperHalfController upper =
                view.GetComponent<UpperHalfController>();

            Transform targetSpawn = null;

            if (lower != null)
                targetSpawn = lowerHalfSpawn;
            else if (upper != null)
                targetSpawn = upperHalfSpawn;

            if (targetSpawn == null)
                continue;

            Rigidbody2D rb =
                view.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.position = targetSpawn.position;
            }
            else
            {
                view.transform.position =
                    targetSpawn.position;
            }
        }

        Debug.Log("Security camera detected the Lower Half!");

        StartCoroutine(ResetCooldown());
    }

    private IEnumerator ResetCooldown()
    {
        yield return new WaitForSeconds(resetCooldown);
        resetInProgress = false;
    }
}
