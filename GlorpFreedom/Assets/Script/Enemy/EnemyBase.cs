using UnityEngine;

/// <summary>
/// Base class for any enemy in the game.
/// - Manages health and damage logic (one-hit death or multi-hit as needed).
/// - Receives a PlayerController2D reference from an external manager for dash detection.
/// </summary>
public class EnemyBase : MonoBehaviour
{
    [Tooltip("Total health points of the enemy.")]
    [SerializeField] protected int health = 1;

    // Reference to the player's controller for dash detection (injected externally)
    protected PlayerController2D playerController;

    // Flag to prevent multiple kills
    protected bool isDead = false;

    /// <summary>
    /// Initializes this enemy with a reference to the player's controller.
    /// Called by EnemyManager after instantiation or on scene start.
    /// </summary>
    /// <param name="controller">The PlayerController2D instance to track.</param>
    public void Initialize(PlayerController2D controller)
    {
        playerController = controller;
    }

    /// <summary>
    /// Called when the enemy takes damage.
    /// Subclasses can override to implement multi-hit behavior.
    /// </summary>
    /// <param name="amount">Amount of damage to apply.</param>
    public virtual void TakeDamage(int amount)
    {
        if (isDead) return;

        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Kills the enemy. Override this to add effects (animations, drops, etc.).
    /// </summary>
    protected virtual void Die()
    {
        if (isDead) return;

        isDead = true;
        Destroy(gameObject);
    }

    /// <summary>
    /// Checks if a given collider belongs to a dashing player.
    /// </summary>
    /// <param name="other">The collider to check.</param>
    /// <returns>True if the collider is the player and the player is dashing.</returns>
    protected bool IsPlayerDashing(Collider2D other)
    {
        if (other.CompareTag("Player") && playerController != null)
        {
            return playerController.IsDashing;
        }
        return false;
    }
}
