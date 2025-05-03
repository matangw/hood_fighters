using UnityEngine;
using UnityEngine.InputSystem;

public class Movements : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float jumpHorizontalForce = 5f;
    public float wallSlideSpeed = 2f;
    public float directionChangeDelay = 0.4f;

    [Header("Ground Detection")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;

    [Header("Wall Detection")]
    public Transform wallCheckHead;
    public Transform wallCheckFeet;
    public float wallCheckRadius = 0.1f;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private bool isWallSliding;
    private bool isWallJumping = false;
    private bool hasDoubleJumped = false;
    public float moveInput;
    private float directionChangeTimer;
    private float originalXScale;

    // Input action references
    private Player_actions playerActions;
    private float movementInput;
    private bool jumpPressed;
    private PlayerInput playerInput;

    // Public property to access isGrounded from other scripts
    public bool IsGrounded => isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.freezeRotation = true;
        originalXScale = Mathf.Abs(transform.localScale.x);
        playerInput = GetComponent<PlayerInput>();

        // Initialize input actions
        playerActions = new Player_actions();
        playerActions.player_actions_map.Enable();
    }

    void OnEnable()
    {
        if (playerActions != null)
        {
            playerActions.player_actions_map.Enable();
        }
    }

    void OnDisable()
    {
        if (playerActions != null)
        {
            playerActions.player_actions_map.Disable();
        }
    }

    void Update()
    {
        HandleInput();
        CheckGrounded();
        CheckWallSlide();
        HandleMovement();
        HandleJumping();
        UpdateAnimations();
        Debug.DrawLine(transform.position, transform.position + (Vector3)rb.linearVelocity.normalized * 2, Color.red);
    }

    private void HandleInput()
    {
        if (playerInput == null || playerInput.actions == null)
            return;

        // Read movement input from the correct device-bound actions
        var movementAction = playerInput.actions["movement"];
        Vector2 movementValue = movementAction.ReadValue<Vector2>();

        if (isWallSliding)
        {
            float facingDirection = Mathf.Sign(transform.localScale.x);
            if (Mathf.Sign(movementValue.x) == facingDirection)
            {
                movementValue.x = 0;
            }
        }

        moveInput = movementValue.x;

        // Read jump input
        var jumpAction = playerInput.actions["jump"];
        jumpPressed = jumpAction.triggered;
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, LayerMask.GetMask("ground"));
        if (isGrounded)
        {
            hasDoubleJumped = false;
        }
    }

    private void CheckWallSlide()
    {
        bool isTouchingWallHead = Physics2D.OverlapCircle(wallCheckHead.position, wallCheckRadius, LayerMask.GetMask("ground"));
        bool isTouchingWallFeet = Physics2D.OverlapCircle(wallCheckFeet.position, wallCheckRadius, LayerMask.GetMask("ground"));
        bool isTouchingWall = isTouchingWallHead || isTouchingWallFeet;
        isWallSliding = isTouchingWall && !isGrounded && rb.linearVelocity.y < 0 && !isWallJumping;

        if (animator != null)
        {
            animator.SetBool("touches_wall", isTouchingWall);
        }

        if (isWallSliding)
        {
            // Calculate the desired wall slide velocity
            float currentVerticalVelocity = rb.linearVelocity.y;
            float targetWallSlideVelocity = -wallSlideSpeed;

            // Only apply force if we're falling faster than wall slide speed
            if (currentVerticalVelocity < targetWallSlideVelocity)
            {
                float velocityDiff = targetWallSlideVelocity - currentVerticalVelocity;
                rb.AddForce(new Vector2(0, velocityDiff), ForceMode2D.Impulse);
            }
        }
    }

    private void HandleMovement()
    {
        if (isWallJumping) return;

        // Calculate the target velocity based on input
        float currentMoveSpeed = isWallSliding ? moveSpeed * 0.5f : moveSpeed;
        float targetVelocityX = moveInput * currentMoveSpeed;

        // Calculate the difference between current and target velocity
        float velocityDiff = targetVelocityX - rb.linearVelocity.x;

        // Apply force to reach target velocity
        rb.AddForce(new Vector2(velocityDiff, 0), ForceMode2D.Impulse);

        // Handle character flipping
        if (moveInput != 0)
        {
            float xScale = originalXScale * Mathf.Sign(moveInput);
            transform.localScale = new Vector3(xScale, transform.localScale.y, transform.localScale.z);
        }
    }

    private void HandleJumping()
    {
        // Check for jump input
        if (jumpPressed)
        {
            if (isGrounded || isWallSliding)
            {
                if (animator != null) animator.SetTrigger("jump");

                // Add a small horizontal force away from the wall when wall jumping
                if (isWallSliding)
                {
                    WallJump();
                }
                else
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
            }
            else if (!hasDoubleJumped)
            {
                hasDoubleJumped = true;
                if (animator != null) animator.SetTrigger("jump");
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 0.8f);
            }
        }
    }

    public void WallJump()
    {
        float facingDirection = transform.localScale.x > 0 ? 1f : -1f;

        // Set the velocity directly
        rb.linearVelocity = new Vector2(-facingDirection * jumpHorizontalForce, jumpForce * 0.8f);

        // Disable movement & wall sliding for a moment to avoid interference
        isWallJumping = true;
        isWallSliding = false;
        moveInput = 0;
        hasDoubleJumped = false;

        Invoke(nameof(ResetWallJump), 0.6f);
    }

    private void ResetWallJump()
    {
        isWallJumping = false;
    }

    private void UpdateAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("moving", Mathf.Abs(moveInput) > 0.1f);
            animator.SetBool("isGrounded", isGrounded);
            //animator.SetBool("isWallSliding", isWallSliding);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(wallCheckFeet.position, wallCheckRadius);
        Gizmos.DrawWireSphere(wallCheckHead.position, wallCheckRadius);
    }
}
