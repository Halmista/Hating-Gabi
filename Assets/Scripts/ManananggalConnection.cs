
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PhotonView))]
public class ManananggalConnection : MonoBehaviour
{
    [Header("Connection")]
    public float connectionRange = 1.5f;
    public float upperMustBeAbove = 0.1f;

    [Header("Connected Prefab")]
    public string connectedPrefabName = "ConnectedManananggal";
    public string connectedStateName = "Connected";
    public Vector3 connectedOffset = Vector3.zero;

    [Header("Connection Timer")]
    public float connectionDuration = 5f;

    private PhotonView photonView;
    private LowerHalfController lowerController;

    private SpriteRenderer[] lowerRenderers;
    private Animator[] lowerAnimators;

    private GameObject connectedInstance;
    public bool isConnected;

    public bool IsConnected => isConnected;

    public Transform ConnectedVisual =>
        connectedInstance != null ? connectedInstance.transform : null;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
        lowerController = GetComponent<LowerHalfController>();

        lowerRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        lowerAnimators =
            GetComponentsInChildren<Animator>(true);
    }

    private void Update()
    {
        if (!photonView.IsMine || isConnected)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.enterKey.wasPressedThisFrame)
            TryConnect();
    }

    private void TryConnect()
    {
        UpperHalfController upper =
            FindFirstObjectByType<UpperHalfController>();

        if (upper == null)
            return;

        Vector2 lowerPosition = transform.position;
        Vector2 upperPosition = upper.transform.position;

        // if (upperPosition.y <= lowerPosition.y + upperMustBeAbove)
        //    return;

        // if (Vector2.Distance(lowerPosition, upperPosition) > connectionRange)
        //     return;

        // Allow the halves to overlap slightly or stand on top
        // of one another, as long as they are close enough.
        if (Vector2.Distance(lowerPosition, upperPosition) > connectionRange)
            return;

        // Only reject the connection if the Upper Half is clearly below.
        if (upperPosition.y < lowerPosition.y - 0.3f)
            return;

        photonView.RPC(
            nameof(RPC_Connect),
            RpcTarget.AllBuffered
        );
    }

    [PunRPC]
    private void RPC_Connect()
    {
        if (isConnected)
            return;

        isConnected = true;

        Invoke(nameof(RequestDisconnect), connectionDuration);

        if (lowerController != null)
            lowerController.SetConnectedMode(true);

        foreach (SpriteRenderer renderer in lowerRenderers)
        {
            if (renderer != null)
                renderer.enabled = false;
        }

        foreach (Animator animator in lowerAnimators)
        {
            if (animator != null)
                animator.enabled = false;
        }

        // Hide the separate Upper Half on this client.
        UpperHalfController[] upperHalves =
            FindObjectsByType<UpperHalfController>(
                FindObjectsSortMode.None
            );

        foreach (UpperHalfController upper in upperHalves)
        {
            upper.SetConnectedMode(true, transform);
        }

        // Create a local visual, not another networked character.
        GameObject prefab =
            Resources.Load<GameObject>(connectedPrefabName);

        if (prefab == null)
        {
            Debug.LogError(
                "Could not find connected prefab in Resources: " +
                connectedPrefabName
            );
            return;
        }

        connectedInstance = Instantiate(
            prefab,
            transform.position + connectedOffset,
            transform.rotation
        );

        Animator connectedAnimator =
            connectedInstance.GetComponentInChildren<Animator>();

        if (connectedAnimator != null &&
            !string.IsNullOrWhiteSpace(connectedStateName))
        {
            int stateHash =
                Animator.StringToHash(connectedStateName);

            if (connectedAnimator.HasState(0, stateHash))
                connectedAnimator.Play(stateHash, 0, 0f);
        }
    }

    private void RequestDisconnect()
    {
        if (!photonView.IsMine)
            return;

        photonView.RPC(nameof(RPC_Disconnect), RpcTarget.AllBuffered);
    }

    [PunRPC]
    private void RPC_Disconnect()
    {
        if (!isConnected)
            return;

        isConnected = false;

        // Restore the Lower Half's appearance and controls.
        if (lowerController != null)
            lowerController.SetConnectedMode(false);

        foreach (SpriteRenderer renderer in lowerRenderers)
        {
            if (renderer != null)
                renderer.enabled = true;
        }

        foreach (Animator animator in lowerAnimators)
        {
            if (animator != null)
                animator.enabled = true;
        }

        // Restore the Upper Half.
        UpperHalfController[] upperHalves =
            FindObjectsByType<UpperHalfController>(
                FindObjectsSortMode.None
            );

        //foreach (UpperHalfController upper in upperHalves)
        //    upper.SetConnectedMode(false, null);
        // Place the Upper Half where the connected body currently is.
        Vector3 finalPosition = connectedInstance != null
            ? connectedInstance.transform.position
            : transform.position;

        // Restore the Upper Half at the connected body's position.
        foreach (UpperHalfController upper in upperHalves)
        {
            upper.transform.position = finalPosition;
            upper.SetConnectedMode(false, null);
        }
        // Remove the connected visual.
        if (connectedInstance != null)
            Destroy(connectedInstance);

        connectedInstance = null;
    }


    private void LateUpdate()
    {
        if (!isConnected || connectedInstance == null)
            return;

        // Follow the Lower Half with the connected visual.
        connectedInstance.transform.SetPositionAndRotation(
            transform.position + connectedOffset,
            transform.rotation
        );

        // Copy the Lower Half's facing direction.
        if (lowerController != null &&
            lowerController.spriteRenderer != null)
        {
            bool flipX = lowerController.spriteRenderer.flipX;

            SpriteRenderer[] connectedRenderers =
                connectedInstance.GetComponentsInChildren<SpriteRenderer>(true);

            foreach (SpriteRenderer sr in connectedRenderers)
            {
                if (sr != null)
                    sr.flipX = flipX;
            }
        }

        // Move only the locally owned Upper Half so Player 2's
        // camera can continue following its own character.
        UpperHalfController[] upperHalves =
            FindObjectsByType<UpperHalfController>(
                FindObjectsSortMode.None
            );

        /*foreach (UpperHalfController upper in upperHalves)
        {
            PhotonView upperView = upper.GetComponent<PhotonView>();

            if (upperView != null &&
                upperView.IsMine &&
                upper.IsConnected)
            {
                upper.transform.SetPositionAndRotation(
                    connectedInstance.transform.position,
                    connectedInstance.transform.rotation
                );
            }
        }*/
    }


}