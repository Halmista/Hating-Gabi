
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class NetworkSpriteFacing : MonoBehaviourPun, IPunObservable
{
    public SpriteRenderer spriteRenderer;
    public bool facesRightByDefault = true;

    private bool facingRight = true;
    private bool receivedFacing;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void SetFacing(float horizontalInput)
    {
        if (!photonView.IsMine || Mathf.Abs(horizontalInput) < 0.01f)
            return;

        facingRight = horizontalInput > 0f;
        ApplyFacing();
    }

    private void ApplyFacing()
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = facingRight != facesRightByDefault;
    }

    public void OnPhotonSerializeView(
        PhotonStream stream,
        PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(facingRight);
        }
        else
        {
            facingRight = (bool)stream.ReceiveNext();
            receivedFacing = true;
        }
    }

    private void Update()
    {
        if (!photonView.IsMine && receivedFacing)
            ApplyFacing();
    }
}
