
using UnityEngine;
using Photon.Pun;

public class CameraFollow : MonoBehaviour
{
    [Header("Camera Follow")]
    public float smoothTime = 0.08f;
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    private LowerHalfController lowerHalf;
    private UpperHalfController upperHalf;
    private Transform target;
    private Vector3 currentVelocity;

    private void LateUpdate()
    {
        FindCharacters();

        Transform desiredTarget = GetDesiredTarget();

        if (desiredTarget != target)
        {
            target = desiredTarget;
            currentVelocity = Vector3.zero;
        }

        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            smoothTime
        );
    }

    private void FindCharacters()
    {
        PhotonView[] views =
            FindObjectsByType<PhotonView>(FindObjectsSortMode.None);

        foreach (PhotonView view in views)
        {
            if (view == null)
                continue;

            LowerHalfController lower =
                view.GetComponent<LowerHalfController>();

            UpperHalfController upper =
                view.GetComponent<UpperHalfController>();

            // Find the Lower Half regardless of ownership.
            if (lower != null)
                lowerHalf = lower;

            // The camera belongs to the local player's Upper Half.
            if (upper != null && view.IsMine)
                upperHalf = upper;
        }
    }

    private Transform GetDesiredTarget()
    {
        // Player 2: switch between Upper and Lower.
        if (upperHalf != null)
        {
            if (upperHalf.IsConnected && lowerHalf != null)
                return lowerHalf.transform;

            return upperHalf.transform;
        }

        // Player 1 always follows their Lower Half.
        if (lowerHalf != null)
            return lowerHalf.transform;

        return null;
    }
}
