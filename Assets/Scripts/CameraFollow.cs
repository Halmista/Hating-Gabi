
using UnityEngine;
using Photon.Pun;

public class CameraFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 1f, -10f);

    private Transform target;

    private void Start()
    {
        FindLocalPlayer();
    }

    private void LateUpdate()
    {
        // Find the local player if one hasn't spawned yet.
        if (target == null)
        {
            FindLocalPlayer();
            return;
        }

        Vector3 desiredPosition = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            1f - Mathf.Exp(-smoothSpeed * Time.deltaTime)
        );
    }

    private void FindLocalPlayer()
    {
        PhotonView[] players =
            FindObjectsByType<PhotonView>(FindObjectsSortMode.None);

        foreach (PhotonView view in players)
        {
            if (view.IsMine &&
                (view.GetComponent<LowerHalfController>() != null ||
                 view.GetComponent<UpperHalfController>() != null))
            {
                target = view.transform;
                return;
            }
        }
    }
}
