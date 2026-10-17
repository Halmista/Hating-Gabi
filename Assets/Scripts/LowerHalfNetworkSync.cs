
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
[RequireComponent(typeof(Rigidbody2D))]
public class LowerHalfNetworkSync : MonoBehaviour, IPunObservable
{
    [Header("Remote Position Smoothing")]
    public float smoothingSpeed = 12f;

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
        if (photonView.IsMine || !receivedFirstUpdate)
            return;

        float blend = 1f - Mathf.Exp(
            -smoothingSpeed * Time.fixedDeltaTime
        );

        Vector2 nextPosition = Vector2.Lerp(
            rb.position,
            networkPosition,
            blend
        );

        rb.MovePosition(nextPosition);
        Debug.DrawLine(rb.position, networkPosition, Color.red);
    }

    public void OnPhotonSerializeView(
        PhotonStream stream,
        PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);
        }
        else
        {
            networkPosition = (Vector2)stream.ReceiveNext(); 
            
            if (!receivedFirstUpdate)
                Debug.Log("First remote position received");

            receivedFirstUpdate = true;
        }
    }
}
