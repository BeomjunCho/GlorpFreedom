using UnityEngine;

/// <summary>
/// Bullet behavior: moves in a specified direction at a constant speed,
/// and destroys itself after a set lifetime or on collision (except checkpoint triggers).
/// </summary>
public class Bullet : MonoBehaviour
{
    [Tooltip("Speed at which the bullet travels.")]
    [SerializeField] private float speed = 10f;

    [Tooltip("Time (in seconds) before the bullet is automatically destroyed.")]
    [SerializeField] private float lifetime = 2f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Destroy the bullet after 'lifetime' seconds to avoid clutter
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        // Move the bullet forward along its local right direction
        rb.velocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // If the collided object has a CheckpointTrigger component, do nothing
        if (other.GetComponent<CheckpointTriggerVolume>() != null)
        {
            return;
        }

        // If collided with the player, deal 1 damage (if PlayerHealth exists)
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1);
            }
        }

        // Destroy the bullet in all other cases (including walls, enemies, etc.)
        Destroy(gameObject);
    }
}
