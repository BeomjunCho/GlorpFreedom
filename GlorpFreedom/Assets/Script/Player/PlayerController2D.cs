using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Horizontal move speed in units per second.")]
    [SerializeField] private float moveSpeed = 3f;

    public Animator animator;

    [Header("Jump Settings")]
    [Tooltip("Upward impulse applied when jumping.")]
    [SerializeField] private float jumpForce = 4f;

    [Tooltip("Allow holding jump key for variable jump height.")]
    [SerializeField] private bool allowVariableJumpHeight = true;

    [Tooltip("Maximum time in seconds for which jump key can be held.")]
    [SerializeField] private float jumpHoldTime = 0.3f;

    [Header("Dash Settings")]
    [Tooltip("Distance in units to travel during the dash.")]
    [SerializeField] private float dashDistance = 3f;

    [Tooltip("Duration in seconds that the dash lasts.")]
    [SerializeField] private float dashTime = 0.1f;

    [Tooltip("Cooldown time in seconds before ground dash can be used again.")]
    [SerializeField] private float groundDashCooldown = 1f;

    [Header("Ground Check Settings")]
    [Tooltip("LayerMask used to detect ground surfaces.")]
    [SerializeField] private LayerMask groundLayer;

    [Tooltip("Distance in units for the downward raycast to check ground.")]
    [SerializeField] private float groundCheckDistance = 0.1f;

    [Tooltip("Transform placed at the player’s feet for ground-check.")]
    [SerializeField] private Transform groundCheckPoint;

    [Header("Audio Settings")]
    [Tooltip("AudioSource component to use for walk looping.")]
    [SerializeField] private AudioSource walkSource;

    [Tooltip("AudioSource component to use for one-shot sound effects.")]
    [SerializeField] private AudioSource sfxSource;

    [Tooltip("Audio clip to play while walking.")]
    [SerializeField] private AudioClip walkClip;

    [Tooltip("Audio clip to play when jumping.")]
    [SerializeField] private AudioClip jumpClip;

    [Tooltip("Audio clip to play when landing.")]
    [SerializeField] private AudioClip landClip;

    [Tooltip("Audio clip to play when dashing.")]
    [SerializeField] private AudioClip dashClip;

    // Reference to Rigidbody2D component
    private Rigidbody2D rb;
    // Input vector for movement
    private Vector2 moveInput;
    // Flag indicating if the player is on the ground
    private bool isGrounded = false;
    // Flag used to detect landing events
    private bool wasGrounded = false;

    // Jump state variables
    private bool isJumping = false;
    private float jumpTimeCounter = 0f;

    // Dash state variables
    private bool isDashing = false;
    public bool IsDashing => isDashing;  // Public getter

    // Allows one air dash per jump
    private bool canDash = true;
    // Timestamp for next allowable ground dash
    private float nextGroundDashTime = 0f;

    private float originalGravityScale;
    private Vector2 lastNonzeroMoveDir = Vector2.right;

    private void Awake()
    {
        // Cache Rigidbody2D reference
        rb = GetComponent<Rigidbody2D>();
        originalGravityScale = rb.gravityScale;

        // Configure walk audio source
        if (walkSource != null)
        {
            walkSource.clip = walkClip;
            walkSource.loop = true;
            walkSource.volume = 1f;
        }

        // Configure SFX audio source
        if (sfxSource != null)
        {
            sfxSource.loop = false;
            sfxSource.volume = 1f;
        }

        // Initialize animator state
        animator.SetBool("isJumping", false);
    }

    private void Update()
    {
        // Check if player is grounded using raycast
        isGrounded = CheckGrounded();

        // If player just landed, play landing sound
        if (!wasGrounded && isGrounded)
        {
            if (landClip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(landClip);
            }
        }

        // When grounded and not moving upward, reset jump and allow air dash
        if (isGrounded && rb.velocity.y <= 0f)
        {
            animator.SetBool("isJumping", false);
            isJumping = false;
            jumpTimeCounter = 0f;
            canDash = true;
        }

        // Read horizontal input (A/D or left/right arrow)
        float xInput = Input.GetAxisRaw("Horizontal");
        // Read vertical input (W/S or up/down arrow)
        float yInput = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(xInput, 0f);

        // Track last nonzero input direction for dash direction
        Vector2 fullInput = new Vector2(xInput, yInput);
        if (fullInput.sqrMagnitude > 0.01f)
        {
            lastNonzeroMoveDir = fullInput.normalized;
        }

        // Handle walk loop: play when moving on ground, stop otherwise
        if (isGrounded && Mathf.Abs(xInput) > 0.01f)
        {
            if (walkSource != null && !walkSource.isPlaying)
            {
                walkSource.volume = 0.5f;
                walkSource.Play();
            }
        }
        else
        {
            if (walkSource != null && walkSource.isPlaying)
            {
                walkSource.volume = 1f;
                walkSource.Stop();
            }
        }

        // Jump input (W or up arrow), only if grounded and not already jumping or dashing
        if (!isDashing && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            if (isGrounded && !isJumping)
            {
                isJumping = true;
                jumpTimeCounter = 0f;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);

                // Play jump sound
                if (jumpClip != null && sfxSource != null)
                {
                    sfxSource.PlayOneShot(jumpClip);
                }
                animator.SetBool("isJumping", true);
            }
        }

        // Variable jump height control, disabled while dashing
        if (allowVariableJumpHeight && isJumping && !isDashing)
        {
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

        // Dash input (Space), handle air dash and ground dash separately
        if (!isDashing && Input.GetKeyDown(KeyCode.Space))
        {
            // Air dash: only if not grounded and air dash available
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

        // Flip sprite based on horizontal input direction
        if (moveInput.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x) * 3f, 3f, 1f);
        }

        // Store previous grounded state for next frame
        wasGrounded = isGrounded;
    }

    private void FixedUpdate()
    {
        // Skip normal movement while dashing (gravity disabled during dash)
        if (isDashing) return;

        // Regular horizontal movement
        Vector2 currentVelocity = rb.velocity;
        currentVelocity.x = moveInput.x * moveSpeed;
        rb.velocity = new Vector2(currentVelocity.x, rb.velocity.y);
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));
    }

    // Coroutine to handle a directional dash movement
    private IEnumerator PerformDash()
    {
        // Cancel any ongoing variable jump
        isJumping = false;
        jumpTimeCounter = jumpHoldTime;

        isDashing = true;

        // Play dash sound effect
        if (dashClip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(dashClip);
        }

        // Determine dash direction based on input or last known direction
        Vector2 inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 dashDir = (inputDir.sqrMagnitude > 0.01f) ? inputDir.normalized : lastNonzeroMoveDir;

        Vector2 startPos = rb.position;
        Vector2 endPos = startPos + dashDir * dashDistance;

        // Disable gravity during dash
        rb.gravityScale = 0f;

        float elapsed = 0f;
        while (elapsed < dashTime)
        {
            elapsed += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(elapsed / dashTime);
            Vector2 newPos = Vector2.Lerp(startPos, endPos, t);
            rb.MovePosition(newPos);
            yield return new WaitForFixedUpdate();
        }

        // Ensure final position is exactly endPos
        rb.MovePosition(endPos);

        // Restore gravity and exit dash state
        rb.gravityScale = originalGravityScale;
        isDashing = false;
    }

    // Returns true if a downward raycast from groundCheckPoint hits groundLayer
    private bool CheckGrounded()
    {
        if (groundCheckPoint == null) return false;

        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(groundLayer);
        filter.useTriggers = false;

        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            filter,
            hits,
            groundCheckDistance
        );

#if UNITY_EDITOR
        Debug.DrawRay(
            groundCheckPoint.position,
            Vector2.down * groundCheckDistance,
            hitCount > 0 ? Color.green : Color.red
        );
#endif

        return hitCount > 0;
    }

#if UNITY_EDITOR
    // Draws a small sphere at the ground check point for visualization
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
