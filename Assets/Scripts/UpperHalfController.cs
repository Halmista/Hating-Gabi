
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PhotonView))]
public class UpperHalfController : MonoBehaviour
{
    [Header("Flight")]
    public float flySpeed = 4f;
    public float flapBoost = 5f;
    public float flapDuration = 0.25f;

    [Header("Interaction")]
    public float interactRadius = 1.5f;
    public LayerMask interactableLayer;

    [Header("Sprite Facing")]
    public SpriteRenderer spriteRenderer;
    public bool facesRightByDefault = true;

    private Rigidbody2D rb;
    private PhotonView photonView;
    private float boostTimer;

    private bool connected;
    public bool IsConnected => connected;
    private Transform interactionOrigin;

    private SpriteRenderer[] originalRenderers;
    private Animator[] originalAnimators;


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        photonView = GetComponent<PhotonView>();

        originalRenderers =
            GetComponentsInChildren<SpriteRenderer>(true);

        originalAnimators =
            GetComponentsInChildren<Animator>(true);

        // NEW: Allow interaction while the Upper Half is separated.
        interactionOrigin = transform;

        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }


    private void Start()
    {
        if (photonView.IsMine)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        rb.freezeRotation = true;
    }

    private void Update()
    {
        if (!photonView.IsMine || Keyboard.current == null)
            return;

        /*if (connected)
        {
            if (Keyboard.current.eKey.wasPressedThisFrame)
                TryInteract();

            return;
        }*/
        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            TryInteract();
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            boostTimer = flapDuration;

            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                flySpeed + flapBoost
            );
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine || Keyboard.current == null)
            return;

        if (connected)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float x = 0f;
        float y = 0f;

        if (Keyboard.current.leftArrowKey.isPressed) x -= 1f;
        if (Keyboard.current.rightArrowKey.isPressed) x += 1f;

        if (spriteRenderer != null && Mathf.Abs(x) > 0.01f)
        {
            bool movingRight = x > 0f;
            spriteRenderer.flipX = movingRight != facesRightByDefault;
        }
        GetComponent<NetworkSpriteFacing>()?.SetFacing(x);
        if (Keyboard.current.upArrowKey.isPressed) y += 1f;
        if (Keyboard.current.downArrowKey.isPressed) y -= 1f;

       

        Vector2 velocity = rb.linearVelocity;
        velocity.x = x * flySpeed;

        if (y != 0f)
        {
            velocity.y = y * flySpeed;
            boostTimer = 0f;
        }
        else if (boostTimer > 0f)
        {
            boostTimer -= Time.fixedDeltaTime;
        }
        else
        {
            velocity.y = 0f;
        }

        rb.linearVelocity = velocity;
    }


    public void SetConnectedMode(bool value, Transform lowerHalf)
    {
        connected = value;
        //interactionOrigin = value ? lowerHalf : null;
        interactionOrigin = value ? lowerHalf : transform;
        
        if (value)
        {
            boostTimer = 0f;
            rb.linearVelocity = Vector2.zero;
        }

        foreach (SpriteRenderer renderer in originalRenderers)
        {
            if (renderer != null)
                renderer.enabled = !value;
        }

        foreach (Animator animator in originalAnimators)
        {
            if (animator != null)
                animator.enabled = !value;
        }
    }



    private void TryInteract()
    {
        Debug.Log("E interaction search started.");

        if (interactionOrigin == null)
        {
            Debug.LogError("Interaction Origin is NULL!");
            return;
        }

        Debug.Log("Interaction origin position: "
            + interactionOrigin.position);

        Debug.Log("Interaction radius: " + interactRadius);
        Debug.Log("Interactable layer mask: "
            + interactableLayer.value);

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            interactionOrigin.position,
            interactRadius,
            interactableLayer
        );

        Debug.Log("Colliders detected: " + hits.Length);

        NetworkInteractable nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            Debug.Log(
                "Detected collider: " + hit.name
                + " | Layer: " + LayerMask.LayerToName(hit.gameObject.layer)
                + " | Enabled: " + hit.enabled
            );

            NetworkInteractable candidate =
                hit.GetComponentInParent<NetworkInteractable>();

            if (candidate == null)
            {
                Debug.LogWarning(
                    hit.name + " has no NetworkInteractable "
                    + "on itself or its parents."
                );
                continue;
            }

            Debug.Log("Found interactable: " + candidate.name);

            float distance = Vector2.Distance(
                interactionOrigin.position,
                candidate.transform.position
            );

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = candidate;
            }
        }

        if (nearest != null)
        {
            Debug.Log("Interacting with: " + nearest.name);
            nearest.Interact();
        }
        else
        {
            Debug.LogWarning(
                "No interactable found. Check the collider, layer mask, "
                + "and NetworkInteractable component."
            );
        }
    }

}