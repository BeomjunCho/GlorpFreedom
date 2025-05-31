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

    [Header("=== Ground Check Settings ===")]
    [Tooltip("LayerMask used to detect what counts as ground.")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("Distance for the downward Raycast ground check.")]
    [SerializeField] private float groundCheckDistance = 0.1f;

    [Tooltip("Transform placed at the player's feet for ground-check.")]
    [SerializeField] private Transform groundCheckPoint;

    // Private references and state
    private Rigidbody2D rb;
    private Vector2 moveInput;          // Raw input for horizontal movement
    private bool isGrounded = false;

    // Jump-related
    private bool isJumping = false;
    private float jumpTimeCounter = 0f;

    // Dash-related
    private bool isDashing = false;
    private bool canDash = true;        // Reset to true on landing
    private float originalGravityScale;

    // Tracks the last nonzero direction the player moved (for default dash direction)
    private Vector2 lastNonzeroMoveDir = Vector2.right;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;
    }

    private void Update()
    {
        // 1) Check whether we are grounded (via Raycast)
        isGrounded = CheckGrounded();

        // —— PREVENT ENDLESS JUMP —— 
        // Only reset isJumping (and jumpTimeCounter) once the player has landed (i.e., grounded AND moving downward or at rest).
        if (isGrounded && rb.velocity.y <= 0f)
        {
            isJumping = false;
            jumpTimeCounter = 0f;

            // —— RESET DASH ON GROUND —— 
            canDash = true;
        }

        // 2) Read WASD / arrow-key input
        float xInput = Input.GetAxisRaw("Horizontal");  // -1, 0, or +1
        float yInput = Input.GetAxisRaw("Vertical");    // -1, 0, or +1
        moveInput = new Vector2(xInput, 0f);            // Only x for walking

        // 3) Track last nonzero movement direction (for diagonal dashes)
        Vector2 fullInput = new Vector2(xInput, yInput);
        if (fullInput.sqrMagnitude > 0.01f)
        {
            lastNonzeroMoveDir = fullInput.normalized;
        }

        // 4) Jump input (W or Up Arrow) — only if grounded and not already jumping or dashing
        if (!isDashing && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            if (isGrounded && !isJumping)
            {
                isJumping = true;
                jumpTimeCounter = 0f;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
        }

        // Variable jump height — disabled while dashing
        if (allowVariableJumpHeight && isJumping && !isDashing)
        {
            // While the jump button is held and within jumpHoldTime, keep setting upward velocity
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

        // 5) Dash input (Left Shift) — only if not already dashing and dash is available
        if (!isDashing && Input.GetKeyDown(KeyCode.Space) && canDash)
        {
            StartCoroutine(PerformDash());
        }
    }

    private void FixedUpdate()
    {
        // While dashing, skip normal movement (gravity is disabled inside PerformDash)
        if (isDashing) return;

        // 6) Regular horizontal movement
        Vector2 currentVelocity = rb.velocity;
        currentVelocity.x = moveInput.x * moveSpeed;
        rb.velocity = new Vector2(currentVelocity.x, rb.velocity.y);
    }

    /// <summary>
    /// Coroutine that handles a directional dash (usable in air or ground).
    /// Uses MovePosition to guarantee a fixed dashDistance in the chosen direction.
    /// </summary>
    private IEnumerator PerformDash()
    {
        // CANCEL any ongoing variable jump so no extra force applies after dash
        isJumping = false;
        jumpTimeCounter = jumpHoldTime; // expire any remaining jump-hold

        isDashing = true;
        canDash = false;

        // Determine dash direction (normalized)
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 dashDir = (inputDir.sqrMagnitude > 0.01f) ? inputDir.normalized : lastNonzeroMoveDir;

        // Calculate start and end positions
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

        // canDash will be reset on the next landing
    }

    /// <summary>
    /// Returns true if a short downward Raycast from groundCheckPoint hits groundLayer.
    /// </summary>
    private bool CheckGrounded()
    {
        if (groundCheckPoint == null) return false;

        RaycastHit2D hit = Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

#if UNITY_EDITOR
        Debug.DrawRay(
            groundCheckPoint.position,
            Vector2.down * groundCheckDistance,
            hit.collider != null ? Color.green : Color.red
        );
#endif

        return hit.collider != null;
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
