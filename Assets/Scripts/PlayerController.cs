using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float crouchSpeedMultiplier = 0.5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private Vector2 standingSize;
    private Vector2 standingOffset;

    private float moveInput;
    private bool jumpQueued;
    private bool isCrouching;
    private bool isGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        standingSize = col.size;
        standingOffset = col.offset;
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        moveInput = 0f;
        if (kb.aKey.isPressed) moveInput -= 1f;
        if (kb.dKey.isPressed) moveInput += 1f;

        if (kb.wKey.wasPressedThisFrame && isGrounded)
        {
            jumpQueued = true;
        }

        isCrouching = kb.sKey.isPressed;

        UpdateCrouchCollider();
        FlipSprite();
    }

    private void FixedUpdate()
    {
        isGrounded = groundCheck != null &&
            Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        float currentSpeed = moveSpeed * (isCrouching ? crouchSpeedMultiplier : 1f);
        rb.linearVelocity = new Vector2(moveInput * currentSpeed, rb.linearVelocity.y);

        if (jumpQueued)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpQueued = false;
        }
    }

    private void UpdateCrouchCollider()
    {
        if (isCrouching)
        {
            col.size = new Vector2(standingSize.x, standingSize.y * 0.5f);
            col.offset = new Vector2(standingOffset.x, standingOffset.y - standingSize.y * 0.25f);
        }
        else
        {
            col.size = standingSize;
            col.offset = standingOffset;
        }
    }

    private void FlipSprite()
    {
        if (Mathf.Abs(moveInput) < 0.01f) return;
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveInput);
        transform.localScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
