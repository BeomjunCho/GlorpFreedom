using UnityEngine;
using System;

public class PlayerHealth : MonoBehaviour
{
    // Event invoked when the player dies
    public static event Action OnPlayerDied;

    private int health = 1;    // One-Hit Kill
    private bool isDead = false;

    // Cache component references for performance
    private Rigidbody2D rb2d;
    private SpriteRenderer sr;
    private PlayerController2D pc;

    // Store original gravity scale so we can restore it on respawn
    private float originalGravityScale;

    private void Awake()
    {
        health = 1;
        isDead = false;

        // Cache references to the Rigidbody2D, SpriteRenderer, and PlayerController2D
        rb2d = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        pc = GetComponent<PlayerController2D>();

        // Save the initial gravityScale (default is usually 1)
        if (rb2d != null)
        {
            originalGravityScale = rb2d.gravityScale;
        }
    }
    /// <summary>
    /// Debug
    /// </summary>
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) TakeDamage(1);
        if (Input.GetKeyDown(KeyCode.L)) ResetHealth();
    }

    // Called externally by traps or bullets when a collision occurs
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        // 1) Disable player controller
        if (pc != null)
            pc.enabled = false;

        // 2) Disable sprite renderer (hide the sprite)
        if (sr != null)
            sr.enabled = false;

        // 3) Remove only gravity by setting gravityScale to zero
        if (rb2d != null)
        {
            rb2d.gravityScale = 0f;
            // Zero out any existing velocity so the player stops immediately:
            rb2d.velocity = Vector2.zero;
        }

        // 4) Invoke death event for other managers
        OnPlayerDied?.Invoke();
    }

    // Called by CheckpointManager when respawning
    public void ResetHealth()
    {
        health = 1;
        isDead = false;

        // 1) Re-enable sprite and controller
        if (sr != null)
            sr.enabled = true;
        if (pc != null)
            pc.enabled = true;

        // 2) Restore the original gravity scale
        if (rb2d != null)
        {
            rb2d.gravityScale = originalGravityScale;
            // Optionally, reset velocity as well:
            rb2d.velocity = Vector2.zero;
        }
    }
}
