
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody2D))]
public class LowerHalfNetworkSync : MonoBehaviour, IPunObservable
{
    [Header("Remote Position Smoothing")]
    public float smoothingSpeed = 15f;

    private PhotonView photonView;
    private Rigidbody2D rb;

    private Vector2 networkPosition;
    private bool receivedFirstUpdate;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        rb = GetComponent<Rigidbody2D>();

        networkPosition = rb.position;
    }

    private void FixedUpdate()
    {
        // The owning player controls its own movement and physics.
        if (photonView.IsMine || !receivedFirstUpdate)
            return;

        // Smoothly follow the latest position received over the network.
        float blend = 1f - Mathf.Exp(
            -smoothingSpeed * Time.fixedDeltaTime
        );

        Vector2 nextPosition = Vector2.Lerp(
            rb.position,
            networkPosition,
            blend
        );

        rb.MovePosition(nextPosition);
    }

    public void OnPhotonSerializeView(
        PhotonStream stream,
        PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send the owning player's actual physics position.
            stream.SendNext(rb.position);
        }
        else
        {
            // Receive the owning player's position.
            networkPosition = (Vector2)stream.ReceiveNext();
            receivedFirstUpdate = true;
        }
    }
}