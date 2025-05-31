using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("=== Movement Settings ===")]
    [Tooltip("Horizontal move speed (units/sec).")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("=== Jump Settings ===")]
    [Tooltip("Upward impulse applied when jumping.")]
    [SerializeField] private float jumpForce = 4f;

    [Tooltip("Allow holding jump for variable jump height.")]
    [SerializeField] private bool allowVariableJumpHeight = true;

    [Tooltip("How long (in seconds) the jump button can be held for extra height.")]
    [SerializeField] private float jumpHoldTime = 0.3f;

    [Header("=== Dash Settings ===")]
    [Tooltip("Distance to travel during the dash.")]
    [SerializeField] private float dashDistance = 3f;

    [Tooltip("Duration (in seconds) that the dash takes.")]
    [SerializeField] private float dashTime = 0.1f;

    [Tooltip("Cooldown time (in seconds) for ground dash.")]
    [SerializeField] private float groundDashCooldown = 1f;

    [Header("=== Ground Check Settings ===")]
    [Tooltip("LayerMask used to detect what counts as ground.")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("Distance for the downward Raycast ground check.")]
    [SerializeField] private float groundCheckDistance = 0.1f;

    [Tooltip("Transform placed at the player's feet for ground-check.")]
    [SerializeField] private Transform groundCheckPoint;

    // Private references and state
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded = false;

    // Jump-related
    private bool isJumping = false;
    private float jumpTimeCounter = 0f;

    // Dash-related
    private bool isDashing = false;
    public bool IsDashing => isDashing;  // Public getter for external checks

    // Air dash: one dash allowed until landing
    private bool canDash = true;

    // Ground dash cooldown control
    private float nextGroundDashTime = 0f;

    private float originalGravityScale;
    private Vector2 lastNonzeroMoveDir = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        // 1) Check whether the player is grounded via Raycast
        isGrounded = CheckGrounded();

        // When the player is grounded and not moving upward, reset jump and allow air dash
        if (isGrounded && rb.velocity.y <= 0f)
        {
            isJumping = false;
            jumpTimeCounter = 0f;

            // Reset air dash availability
            canDash = true;
        }

        // 2) Read horizontal (WASD/arrow) input
        float xInput = Input.GetAxisRaw("Horizontal");  // -1, 0, or +1
        float yInput = Input.GetAxisRaw("Vertical");    // -1, 0, or +1
        moveInput = new Vector2(xInput, 0f);            // Only horizontal for movement

        // 3) Track last non-zero movement direction for dash direction
        Vector2 fullInput = new Vector2(xInput, yInput);
        if (fullInput.sqrMagnitude > 0.01f)
        {
            lastNonzeroMoveDir = fullInput.normalized;
        }

        // 4) Jump input (W or Up Arrow) - only if grounded and not already jumping or dashing
        if (!isDashing && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            if (isGrounded && !isJumping)
            {
                isJumping = true;
                jumpTimeCounter = 0f;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        // Variable jump height control (disabled while dashing)
        if (allowVariableJumpHeight && isJumping && !isDashing)
        {
            // While jump key is held and within jumpHoldTime, maintain upward velocity
            if ((Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) &&
                jumpTimeCounter < jumpHoldTime)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpTimeCounter += Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        // Stop variable jump on key release
        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            isJumping = false;
        }

        // 5) Dash input (Space) - handle air and ground separately
        if (!isDashing && Input.GetKeyDown(KeyCode.Space))
        {
            // Air dash: only if not grounded and air dash is available
            if (!isGrounded && canDash)
            {
                canDash = false;
                StartCoroutine(PerformDash());
            }
            // Ground dash: check cooldown
            else if (isGrounded && Time.time >= nextGroundDashTime)
            {
                nextGroundDashTime = Time.time + groundDashCooldown;
                StartCoroutine(PerformDash());
            }
        }
    }

    private void FixedUpdate()
    {
        // Skip normal movement while dashing (gravity is disabled during dash)
        if (isDashing) return;

        // 6) Regular horizontal movement
        Vector2 currentVelocity = rb.velocity;
        currentVelocity.x = moveInput.x * moveSpeed;
        rb.velocity = new Vector2(currentVelocity.x, rb.velocity.y);
    }

    /// <summary>
    /// Coroutine that handles a directional dash.
    /// Uses MovePosition to guarantee a fixed dashDistance in the chosen direction.
    /// </summary>
    private IEnumerator PerformDash()
    {
        // Cancel any ongoing variable jump
        isJumping = false;
        jumpTimeCounter = jumpHoldTime; // Expire any remaining jump hold

        isDashing = true;

        // Determine dash direction (normalized) based on input or last move direction
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 dashDir = (inputDir.sqrMagnitude > 0.01f) ? inputDir.normalized : lastNonzeroMoveDir;

        Vector2 startPos = rb.position;
        Vector2 endPos = startPos + dashDir * dashDistance;

        // Disable gravity so the player won't be pulled down during dash
        rb.gravityScale = 0f;

        float elapsed = 0f;
        while (elapsed < dashTime)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / dashTime);

            // Interpolate position and move the Rigidbody
            Vector2 newPos = Vector2.Lerp(startPos, endPos, t);
            rb.MovePosition(newPos);

            yield return new WaitForFixedUpdate();
        }

        // Ensure final position is exactly the target
        rb.MovePosition(endPos);

        // Restore gravity and exit dash state
        rb.gravityScale = originalGravityScale;
        isDashing = false;
    }

    /// <summary>
    /// Returns true if a short downward Raycast from groundCheckPoint hits groundLayer, ignoring any trigger colliders.
    /// </summary>
    private bool CheckGrounded()
    {
        if (groundCheckPoint == null) return false;

        // Create a ContactFilter2D that only hits the groundLayer and ignores triggers
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        filter.useTriggers = false; // Ignore trigger colliders

        // Prepare a single-element array to receive the hit result
        RaycastHit2D[] hits = new RaycastHit2D[1];

        // Perform the raycast with the contact filter
        int hitCount = Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            filter,
            hits,
            groundCheckDistance
        );

#if UNITY_EDITOR
        // Draw a debug ray: green if ground, red otherwise
        Debug.DrawRay(
            groundCheckPoint.position,
            Vector2.down * groundCheckDistance,
            hitCount > 0 ? Color.green : Color.red
        );
#endif

        return hitCount > 0;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheckPoint.position, 0.02f);
        }
    }
#endif
}
