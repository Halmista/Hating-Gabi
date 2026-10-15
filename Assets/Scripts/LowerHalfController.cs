
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PhotonView))]
public class LowerHalfController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 8f;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.08f;
    public float groundCheckWidth = 0.85f;

    [Header("Sprite Facing")]
    public SpriteRenderer spriteRenderer;
    public bool facesRightByDefault = true;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private Animator animator;
    private PhotonView photonView;

    private float ownerGravityScale;
    private float moveInput;
    private bool grounded;
    private bool lastWalking;
    private bool connected;

    public bool IsConnected => connected;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        photonView = GetComponent<PhotonView>();

        ownerGravityScale = rb.gravityScale;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Start()
    {
        rb.freezeRotation = true;

        if (photonView.IsMine)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = ownerGravityScale;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode =
                CollisionDetectionMode2D.Continuous;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;

            SetAnimationState(false);
        }
    }

    private void Update()
    {
        if (!photonView.IsMine || Keyboard.current == null)
            return;

        moveInput = 0f;

        if (Keyboard.current.aKey.isPressed)
            moveInput -= 1f;

        if (Keyboard.current.dKey.isPressed)
            moveInput += 1f;

        if (spriteRenderer != null && Mathf.Abs(moveInput) > 0.01f)
        {
            bool movingRight = moveInput > 0f;
            spriteRenderer.flipX = movingRight != facesRightByDefault;
        }

        GetComponent<NetworkSpriteFacing>()?.SetFacing(moveInput);

        grounded = IsGrounded();

        if (Keyboard.current.wKey.wasPressedThisFrame && grounded)
        {
            rb.linearVelocity =
                new Vector2(rb.linearVelocity.x, jumpForce);

            grounded = false;
        }

        // Keep movement active while connected.
        // Only the separate Lower Half animation is skipped.
        if (connected)
            return;

        bool walking =
            grounded && Mathf.Abs(moveInput) > 0.01f;

        if (walking != lastWalking)
        {
            SetAnimationState(walking);

            photonView.RPC(
                nameof(RPC_SetWalking),
                RpcTarget.Others,
                walking
            );

            lastWalking = walking;
        }
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine || Keyboard.current == null)
            return;

        rb.linearVelocity = new Vector2(
            moveInput * moveSpeed,
            rb.linearVelocity.y
        );
    }


    public void SetConnectedMode(bool value)
    {
        connected = value;
        moveInput = 0f;
        lastWalking = false;

        if (connected)
        {
            SetAnimationState(false);
        }
        else
        {
            SetAnimationState(false);
        }
    }


    private void SetAnimationState(bool walking)
    {
        if (animator == null || connected)
            return;

        animator.speed = 1f;

        if (walking)
        {
            animator.speed = 1f;
        }
        else
        {
            animator.Play(0, 0, 0f);
            animator.Update(0f);
            animator.speed = 0f;
        }
    }

    [PunRPC]
    public void RPC_SetWalking(bool walking)
    {
        if (photonView.IsMine || connected)
            return;

        SetAnimationState(walking);
    }

    private bool IsGrounded()
    {
        if (bodyCollider == null)
            return false;

        Bounds bounds = bodyCollider.bounds;

        float width = Mathf.Max(
            0.05f,
            bounds.size.x * groundCheckWidth
        );

        float height = Mathf.Max(
            0.04f,
            groundCheckDistance
        );

        Vector2 checkPosition = new Vector2(
            bounds.center.x,
            bounds.min.y - height * 0.5f
        );

        return Physics2D.OverlapBox(
            checkPosition,
            new Vector2(width, height),
            0f,
            groundLayer
        ) != null;
    }

    private void OnDrawGizmosSelected()
    {
        Collider2D col = GetComponent<Collider2D>();

        if (col == null)
            return;

        Bounds bounds = col.bounds;

        float width = Mathf.Max(
            0.05f,
            bounds.size.x * groundCheckWidth
        );

        float height = Mathf.Max(
            0.04f,
            groundCheckDistance
        );

        Vector2 checkPosition = new Vector2(
            bounds.center.x,
            bounds.min.y - height * 0.5f
        );

        Gizmos.color = Color.yellow;

        Gizmos.DrawWireCube(
            checkPosition,
            new Vector2(width, height)
        );
    }
}